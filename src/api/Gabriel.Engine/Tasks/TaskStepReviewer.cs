using System.Text;
using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;
using Microsoft.Extensions.Logging;

namespace Gabriel.Engine.Tasks;

// Parsed = false means the reviewer's reply was unusable and the verdict is
// the conservative default (progress assumed, done NOT confirmed).
public sealed record StepReview(bool Progress, bool Done, string? Feedback, bool Parsed);

public interface ITaskStepReviewer
{
    Task<StepReview> ReviewAsync(
        AgentTask task, string stepTranscript, IChatProvider provider, string modelName, CancellationToken ct);
}

// An independent check after every step. The agent's own claim ("done") is
// not trusted on its own - completion requires this reviewer to agree that
// every done-criterion is met - and "no progress" verdicts are what trip the
// stuck detector. Deliberately a separate, tool-less call with its own prompt,
// not the agent grading itself in-context.
public sealed class TaskStepReviewer : ITaskStepReviewer
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
    private const int RecentLogLines = 12;

    private const string SystemPrompt =
        "You review one step of an autonomous AI agent's work against its contract. Be strict and " +
        "concrete; judge only from the evidence shown. Reply with JSON only, no prose: " +
        "{\"progress\": boolean, \"done\": boolean, \"feedback\": string}. " +
        "progress: this step moved the work measurably closer to the done-criteria - produced or changed " +
        "an artifact, established a needed fact, completed a sub-task. Planning, re-reading, or repeating " +
        "an earlier attempt is not progress. " +
        "done: EVERY done-criterion is satisfied, judging from this step and the progress log. When unsure, false. " +
        "feedback: one or two sentences for the agent's next step - what is missing or what to do next. " +
        "Empty string if there is nothing useful to add.";

    private readonly ILogger<TaskStepReviewer> _logger;

    public TaskStepReviewer(ILogger<TaskStepReviewer> logger) => _logger = logger;

    public async Task<StepReview> ReviewAsync(
        AgentTask task, string stepTranscript, IChatProvider provider, string modelName, CancellationToken ct)
    {
        var fallback = new StepReview(Progress: true, Done: false, Feedback: null, Parsed: false);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(Timeout);
        try
        {
            var reply = await ProviderText.CompleteAsync(
                provider, modelName, SystemPrompt, BuildPrompt(task, stepTranscript), timeout.Token);
            using var json = ProviderText.ExtractJsonObject(reply);
            if (json is null)
            {
                _logger.LogWarning("Step review was not JSON | task={TaskId}", task.Id);
                return fallback;
            }

            var root = json.RootElement;
            var feedback = root.TryGetProperty("feedback", out var f) && f.ValueKind == JsonValueKind.String
                ? f.GetString()!.Trim()
                : null;
            return new StepReview(
                Progress: ReadBool(root, "progress") ?? true,
                Done: ReadBool(root, "done") ?? false,
                Feedback: string.IsNullOrEmpty(feedback) ? null : feedback,
                Parsed: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Step review failed | task={TaskId}", task.Id);
            return fallback;
        }
    }

    private static string BuildPrompt(AgentTask task, string stepTranscript)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Contract");
        sb.Append("Objective: ").AppendLine(task.Objective);
        sb.AppendLine("Done when:");
        foreach (var c in task.DoneCriteria) sb.Append("- ").AppendLine(c);
        if (task.Constraints.Count > 0)
        {
            sb.AppendLine("Constraints:");
            foreach (var c in task.Constraints) sb.Append("- ").AppendLine(c);
        }

        var recent = task.Log.Where(e => e.Kind == AgentTaskLogKind.Step).TakeLast(RecentLogLines).ToList();
        if (recent.Count > 0)
        {
            sb.AppendLine().AppendLine("## Progress log (earlier steps)");
            foreach (var e in recent) sb.Append("- ").AppendLine(e.Text);
        }

        sb.AppendLine().Append("## Step ").Append(task.StepsCompleted + 1).AppendLine(" transcript");
        sb.AppendLine(stepTranscript);

        if (task.CurrentStepCheckpoint is { } status)
        {
            sb.AppendLine().AppendLine("## Agent's own report");
            sb.Append("Status: ").AppendLine(status.ToString().ToLowerInvariant());
            sb.Append("Progress: ").AppendLine(task.CheckpointProgress);
            if (task.CheckpointSummary is not null) sb.Append("Summary: ").AppendLine(task.CheckpointSummary);
        }
        else
        {
            sb.AppendLine().AppendLine("## Agent's own report").AppendLine("(none - the agent did not call task_checkpoint)");
        }
        return sb.ToString();
    }

    private static bool? ReadBool(JsonElement root, string name)
        => root.TryGetProperty(name, out var v) && v.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? v.GetBoolean()
            : null;
}
