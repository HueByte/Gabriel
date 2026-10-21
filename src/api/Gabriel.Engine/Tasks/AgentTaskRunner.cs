using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tasks;

public interface IAgentTaskRunner
{
    // Advances one Running task by one step. Expects to run in its own DI scope
    // with CurrentUserOverride set to the task owner. Every outcome - success,
    // stop, error, budget - ends in a persisted state change, so a task can
    // never be stuck re-running the same broken step.
    Task RunStepAsync(Guid taskId, CancellationToken ct);
}

public sealed class AgentTaskRunner : IAgentTaskRunner
{
    // The reviewer sees the tail of the step - where the outcome is.
    private const int TranscriptCharBudget = 8_000;
    private const int AssistantTextCap = 1_500;
    private const int ToolArgsCap = 200;
    private const int ToolResultCap = 400;

    private readonly IAgentTaskRepository _tasks;
    private readonly IConversationRepository _conversations;
    private readonly IUnitOfWork _uow;
    private readonly IAgentService _agent;
    private readonly ITaskStepReviewer _reviewer;
    private readonly IUserPreferences _userPrefs;
    private readonly IModelCatalog _modelCatalog;
    private readonly IChatProviderRegistry _providers;
    private readonly AgentTaskOptions _options;
    private readonly ILogger<AgentTaskRunner> _logger;

    public AgentTaskRunner(
        IAgentTaskRepository tasks,
        IConversationRepository conversations,
        IUnitOfWork uow,
        IAgentService agent,
        ITaskStepReviewer reviewer,
        IUserPreferences userPrefs,
        IModelCatalog modelCatalog,
        IChatProviderRegistry providers,
        IOptions<AgentTaskOptions> options,
        ILogger<AgentTaskRunner> logger)
    {
        _tasks = tasks;
        _conversations = conversations;
        _uow = uow;
        _agent = agent;
        _reviewer = reviewer;
        _userPrefs = userPrefs;
        _modelCatalog = modelCatalog;
        _providers = providers;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunStepAsync(Guid taskId, CancellationToken ct)
    {
        var task = await _tasks.GetForWorkerAsync(taskId, ct);
        if (task is null || task.Status != AgentTaskStatus.Running) return;

        if (task.BudgetExhaustedReason(DateTimeOffset.UtcNow) is { } budget)
        {
            task.Pause(budget + " Resume to continue with a fresh budget.", AgentTaskLogKind.System);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        var step = task.StepsCompleted + 1;
        var stepMessage = BuildStepMessage(task, step, task.TakeGuidance());
        _logger.LogInformation("Task step start | task={TaskId} step={Step}/{Max}", task.Id, step, task.MaxSteps);

        var outcome = await RunAgentTurnAsync(task, stepMessage, ct);
        if (ct.IsCancellationRequested) return; // host shutting down; the step re-runs after restart

        // The API may have paused or cancelled the task while the step ran.
        await _tasks.ReloadAsync(task, ct);
        if (task.Status != AgentTaskStatus.Running)
        {
            task.AddLog(AgentTaskLogKind.System, $"Step {step} interrupted ({task.Status.ToString().ToLowerInvariant()}).", step);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        if (outcome.Error is { } error && outcome.UserMessageId is null)
        {
            // The turn never started (conversation gone, provider rejected the
            // request) - nothing to review.
            await RecordErrorAsync(task, error, ct);
            return;
        }

        var transcript = await BuildTranscriptAsync(task, outcome.UserMessageId, ct);
        if (outcome.Error is { } streamError && transcript.Length == 0)
        {
            await RecordErrorAsync(task, streamError, ct);
            return;
        }

        await DecideAsync(task, step, transcript, outcome.LastAssistantText, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task DecideAsync(AgentTask task, int step, string transcript, string? lastText, CancellationToken ct)
    {
        StepReview? review = null;
        if (TaskStepPolicy.NeedsReview(task))
        {
            var (provider, model) = await ResolveModelAsync(ct);
            review = await _reviewer.ReviewAsync(task, transcript, provider, model, ct);
        }
        TaskStepPolicy.Apply(task, step, review, lastText, _options.MaxStepsWithoutProgress, DateTimeOffset.UtcNow);
    }

    private async Task RecordErrorAsync(AgentTask task, string error, CancellationToken ct)
    {
        task.RecordStepError(error);
        if (task.ConsecutiveErrors >= _options.MaxConsecutiveErrors)
            task.Fail($"{task.ConsecutiveErrors} steps in a row failed. Last error: {error}", DateTimeOffset.UtcNow);
        await _uow.SaveChangesAsync(ct);
    }

    private sealed record TurnOutcome(Guid? UserMessageId, string? LastAssistantText, string? Error);

    // Drains a normal agent turn - no SSE, no typing tempo. Exceptions become
    // an Error outcome so the step is recorded instead of crashing the worker.
    private async Task<TurnOutcome> RunAgentTurnAsync(AgentTask task, string stepMessage, CancellationToken ct)
    {
        Guid? userMessageId = null;
        string? lastText = null;
        string? error = null;
        try
        {
            var stream = await _agent.RunAsync(task.ConversationId, stepMessage, ct);
            await foreach (var evt in stream.WithCancellation(ct))
            {
                switch (evt)
                {
                    case AgentUserMessagePersisted p: userMessageId = p.MessageId; break;
                    case AgentAssistantMessage a: lastText = a.Content; break;
                    case AgentError e: error = e.Message; break;
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Task step turn failed | task={TaskId}", task.Id);
            error = ex.Message;
        }
        return new TurnOutcome(userMessageId, lastText, error);
    }

    internal static string BuildStepMessage(AgentTask task, int step, string? guidance)
    {
        var sb = new StringBuilder();
        sb.Append("[Autonomous task - step ").Append(step).Append(" of at most ").Append(task.MaxSteps).AppendLine("]");
        sb.AppendLine("You are working unattended on a task the user delegated. Do the next concrete piece of work toward the done-criteria, then report it with task_checkpoint.");
        sb.AppendLine();
        sb.Append("Objective: ").AppendLine(task.Objective);
        sb.AppendLine("Done when:");
        foreach (var c in task.DoneCriteria) sb.Append("- ").AppendLine(c);
        if (task.Constraints.Count > 0)
        {
            sb.AppendLine("Constraints:");
            foreach (var c in task.Constraints) sb.Append("- ").AppendLine(c);
        }
        if (!string.IsNullOrWhiteSpace(guidance))
        {
            sb.AppendLine().Append("Guidance for this step: ").AppendLine(guidance);
        }
        if (step == 1)
        {
            sb.AppendLine().AppendLine("This is the first step: if the work has several parts, lay out a plan with todo_write first, then start on it.");
        }
        sb.AppendLine();
        sb.AppendLine("Rules: work only toward the done-criteria and respect the constraints. Deliver results as files with write_project_file rather than long chat replies. If an approach fails, try a different one instead of repeating it. Use status 'blocked' only when you truly cannot proceed without the user. End the step by calling task_checkpoint exactly once.");
        return sb.ToString();
    }

    // Everything the agent did after this step's message, rendered compactly;
    // over budget, the head is dropped so the outcome stays visible.
    private async Task<string> BuildTranscriptAsync(AgentTask task, Guid? userMessageId, CancellationToken ct)
    {
        if (userMessageId is null) return "";
        var conversation = await _conversations.GetByIdWithMessagesAsync(task.ConversationId, task.UserId, ct);
        if (conversation is null) return "";

        var messages = conversation.Messages.ToList();
        var start = messages.FindIndex(m => m.Id == userMessageId);
        if (start < 0) return "";

        var sb = new StringBuilder();
        foreach (var m in messages.Skip(start + 1).Where(m => m.IsActiveVariant))
        {
            switch (m.Role)
            {
                case MessageRole.Assistant:
                    if (!string.IsNullOrWhiteSpace(m.Content))
                        sb.Append("Assistant: ").AppendLine(Truncate(m.Content, AssistantTextCap));
                    foreach (var (name, args) in ParseToolCalls(m.ToolCallsJson))
                        sb.Append("Tool call: ").Append(name).Append('(').Append(Truncate(args, ToolArgsCap)).AppendLine(")");
                    break;
                case MessageRole.Tool:
                    sb.Append("Tool result: ").AppendLine(Truncate(m.Content, ToolResultCap));
                    break;
            }
        }

        var text = sb.ToString();
        return text.Length <= TranscriptCharBudget ? text : "…" + text[^TranscriptCharBudget..];
    }

    private static IEnumerable<(string Name, string Args)> ParseToolCalls(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) yield break;
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException) { yield break; }
        using (doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Array) yield break;
            foreach (var call in doc.RootElement.EnumerateArray())
            {
                if (!call.TryGetProperty("function", out var fn)) continue;
                var name = fn.TryGetProperty("name", out var n) ? n.GetString() ?? "?" : "?";
                var args = fn.TryGetProperty("arguments", out var a) ? a.GetString() ?? "" : "";
                yield return (name, args);
            }
        }
    }

    private async Task<(IChatProvider Provider, string Model)> ResolveModelAsync(CancellationToken ct)
    {
        var prefs = await _userPrefs.GetAsync(ct);
        var selection = _modelCatalog.Resolve(prefs.PreferredProvider, prefs.PreferredModel);
        return (_providers.Resolve(selection.Provider), selection.Name);
    }

    private static string? Truncate(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var flat = text.Trim();
        return flat.Length <= max ? flat : flat[..max] + "…";
    }
}
