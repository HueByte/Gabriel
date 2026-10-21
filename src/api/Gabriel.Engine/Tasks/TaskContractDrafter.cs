using System.Text.Json;
using Gabriel.Engine.Providers;
using Microsoft.Extensions.Logging;

namespace Gabriel.Engine.Tasks;

public sealed record TaskContract(string Objective, IReadOnlyList<string> DoneCriteria, IReadOnlyList<string> Constraints);

public interface ITaskContractDrafter
{
    // Never throws: on any failure the contract is the raw goal with no
    // criteria, which the user must complete before approval anyway.
    Task<TaskContract> DraftAsync(string goal, IChatProvider provider, string modelName, int maxDurationMinutes, CancellationToken ct);
}

// Turns a free-form request into the contract the whole run is judged
// against. The done-criteria are the important part: they are what lets an
// unattended run stop for the right reason instead of whenever the model
// feels finished.
public sealed class TaskContractDrafter : ITaskContractDrafter
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(45);

    private readonly ILogger<TaskContractDrafter> _logger;

    public TaskContractDrafter(ILogger<TaskContractDrafter> logger) => _logger = logger;

    public async Task<TaskContract> DraftAsync(
        string goal, IChatProvider provider, string modelName, int maxDurationMinutes, CancellationToken ct)
    {
        var fallback = new TaskContract(goal.Trim(), [], []);

        var system =
            "You turn a user's request into a contract for an autonomous AI agent that will work on it " +
            $"unattended for up to {maxDurationMinutes} minutes. The agent can search and read the web, " +
            "read and write text files in the project's file area, save memories, and keep a todo list. " +
            "Reply with JSON only, no prose: " +
            "{\"objective\": string, \"doneCriteria\": [string], \"constraints\": [string]}. " +
            "objective: one or two specific sentences restating what success means. " +
            "doneCriteria: 2 to 6 checkable conditions that together mean the work is finished - prefer " +
            "observable outcomes (a named file exists containing X; each question answered with a source). " +
            "constraints: what the agent must respect or avoid, including every limit the user stated; " +
            "may be empty. Write in the language of the request.";

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(Timeout);
        try
        {
            var reply = await ProviderText.CompleteAsync(provider, modelName, system, goal, timeout.Token);
            using var json = ProviderText.ExtractJsonObject(reply);
            if (json is null)
            {
                _logger.LogWarning("Task contract draft was not JSON; using the raw goal");
                return fallback;
            }

            var root = json.RootElement;
            var objective = root.TryGetProperty("objective", out var o) && o.ValueKind == JsonValueKind.String
                ? o.GetString()!.Trim()
                : "";
            return new TaskContract(
                objective.Length > 0 ? objective : fallback.Objective,
                ReadList(root, "doneCriteria"),
                ReadList(root, "constraints"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Task contract draft failed; using the raw goal");
            return fallback;
        }
    }

    private static IReadOnlyList<string> ReadList(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var arr) || arr.ValueKind != JsonValueKind.Array) return [];
        return arr.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString()!.Trim())
            .Where(s => s.Length > 0)
            .Take(10)
            .ToList();
    }
}
