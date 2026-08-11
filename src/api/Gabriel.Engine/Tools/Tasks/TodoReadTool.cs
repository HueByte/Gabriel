using System.Text.Json;
using Gabriel.Core.Repositories;

namespace Gabriel.Engine.Tools.Tasks;

// Companion to todo_write: re-reads the persisted list so a fresh turn (or a
// resumed conversation) can pick the plan back up without scanning history.
//
// Not parallel-safe: reads the conversation through the scoped repository.
public sealed class TodoReadTool : ITool
{
    private readonly IConversationRepository _conversations;
    private readonly IToolExecutionContext _context;

    public TodoReadTool(IConversationRepository conversations, IToolExecutionContext context)
    {
        _conversations = conversations;
        _context = context;
    }

    public string Name => "todo_read";

    public string Description =>
        "Read the current todo list for this conversation (as last written by " +
        "todo_write). Use when resuming multi-step work to see where you left off.";

    public string ParametersJsonSchema => """{"type":"object","properties":{}}""";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        if (_context.ConversationId is not { } conversationId || _context.UserId is not { } userId)
        {
            return "Error: no conversation context available.";
        }

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct);
        if (conversation is null)
        {
            return "Error: conversation not found.";
        }

        if (string.IsNullOrWhiteSpace(conversation.TodoListJson))
        {
            return "No todo list recorded for this conversation.";
        }

        try
        {
            var todos = JsonSerializer.Deserialize<List<TodoWriteTool.TodoItem>>(
                conversation.TodoListJson, TodoWriteTool.JsonOpts) ?? [];
            return todos.Count == 0
                ? "No todo list recorded for this conversation."
                : TodoWriteTool.Render(todos);
        }
        catch (JsonException)
        {
            return "Error: stored todo list is corrupted; rewrite it with todo_write.";
        }
    }
}
