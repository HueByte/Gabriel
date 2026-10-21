using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;

namespace Gabriel.Engine.Tools.Tasks;

// The model's structured end-of-step signal inside an autonomous task. The
// task runner reads it after the turn instead of parsing free text: continue
// (more steps needed), done (claims the contract is met - a reviewer still has
// to agree), or blocked (needs the user). Outside a task conversation it is a
// soft error, so a stray call in normal chat is harmless.
//
// Not parallel-safe: loads + saves the task through the scoped DbContext.
public sealed class TaskCheckpointTool : ITool
{
    private readonly IAgentTaskRepository _tasks;
    private readonly IUnitOfWork _uow;
    private readonly IToolExecutionContext _context;

    public TaskCheckpointTool(IAgentTaskRepository tasks, IUnitOfWork uow, IToolExecutionContext context)
    {
        _tasks = tasks;
        _uow = uow;
        _context = context;
    }

    public string Name => "task_checkpoint";

    public string Description =>
        "ONLY inside an autonomous task (the user message starts with '[Autonomous task'). " +
        "Call exactly once at the end of every step to report it: status 'continue' when more " +
        "work remains, 'done' when every done-criterion of the contract is met (an independent " +
        "reviewer checks this), or 'blocked' when you cannot proceed without the user - then put " +
        "one specific question in `question`. `progress` says concretely what this step achieved. " +
        "With 'done', `summary` is the final report for the user: what was produced, where, and " +
        "anything left undone. Do not call this in normal conversations.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "status":   { "type": "string", "enum": ["continue", "done", "blocked"] },
            "progress": { "type": "string", "description": "What this step concretely achieved (1-3 sentences)." },
            "question": { "type": "string", "description": "Required when status is 'blocked': the one question the user must answer." },
            "summary":  { "type": "string", "description": "Required when status is 'done': the final report for the user." }
          },
          "required": ["status", "progress"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ConversationId is not { } conversationId)
            return "Error: no conversation context available.";

        var task = await _tasks.FindByConversationAsync(conversationId, ct);
        if (task is null || task.Status != AgentTaskStatus.Running)
            return "Error: task_checkpoint only works inside a running autonomous task. In a normal conversation, just answer.";

        Args args;
        try
        {
            args = JsonSerializer.Deserialize<Args>(argumentsJson, JsonOpts) ?? throw new JsonException("null args");
        }
        catch (JsonException ex)
        {
            return $"Error: invalid arguments JSON - {ex.Message}";
        }

        TaskCheckpointStatus status;
        switch (args.Status?.Trim().ToLowerInvariant())
        {
            case "continue": status = TaskCheckpointStatus.Continue; break;
            case "done": status = TaskCheckpointStatus.Done; break;
            case "blocked": status = TaskCheckpointStatus.Blocked; break;
            default: return "Error: status must be 'continue', 'done', or 'blocked'.";
        }
        if (string.IsNullOrWhiteSpace(args.Progress))
            return "Error: 'progress' is required - say what this step achieved.";
        if (status == TaskCheckpointStatus.Blocked && string.IsNullOrWhiteSpace(args.Question))
            return "Error: a 'blocked' checkpoint needs a 'question' for the user.";
        if (status == TaskCheckpointStatus.Done && string.IsNullOrWhiteSpace(args.Summary))
            return "Error: a 'done' checkpoint needs a 'summary' - the final report for the user.";

        task.RecordCheckpoint(status, args.Progress, args.Question, args.Summary);
        await _uow.SaveChangesAsync(ct);

        return status switch
        {
            TaskCheckpointStatus.Done => "Checkpoint recorded: done. A reviewer will check the done-criteria. End your reply now.",
            TaskCheckpointStatus.Blocked => "Checkpoint recorded: blocked. The user will be asked your question. End your reply now.",
            _ => "Checkpoint recorded. End your reply now; the next step will continue the work.",
        };
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record Args(string? Status, string? Progress, string? Question, string? Summary);
}
