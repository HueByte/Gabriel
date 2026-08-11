using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Gabriel.Engine.Tools;
using Gabriel.Engine.Tools.Tasks;
using Xunit;

namespace Gabriel.Tests.Tools;

public class TodoToolTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static (TodoWriteTool Write, TodoReadTool Read, Conversation Conversation) Create()
    {
        var conversation = Conversation.Create(UserId, Guid.NewGuid());
        var repo = new FakeConversationRepository(conversation);
        var context = new ToolExecutionContext();
        context.Set(conversation.Id, UserId, conversation.ProjectId);
        return (
            new TodoWriteTool(repo, new FakeUnitOfWork(), context),
            new TodoReadTool(repo, context),
            conversation);
    }

    [Fact]
    public async Task Write_persists_list_and_renders_checklist()
    {
        var (write, read, conversation) = Create();

        var result = await write.ExecuteAsync(
            """{"todos":[{"content":"scan files","status":"completed"},{"content":"fix bug","status":"in_progress"},{"content":"run tests","status":"pending"}]}""",
            CancellationToken.None);

        Assert.Contains("[x] scan files", result);
        Assert.Contains("[~] fix bug", result);
        Assert.Contains("[ ] run tests", result);
        Assert.Contains("(1/3 completed)", result);
        Assert.NotNull(conversation.TodoListJson);

        var readBack = await read.ExecuteAsync("{}", CancellationToken.None);
        Assert.Contains("[~] fix bug", readBack);
    }

    [Fact]
    public async Task Write_rejects_two_in_progress_items()
    {
        var (write, _, _) = Create();
        var result = await write.ExecuteAsync(
            """{"todos":[{"content":"a","status":"in_progress"},{"content":"b","status":"in_progress"}]}""",
            CancellationToken.None);
        Assert.StartsWith("Error", result);
    }

    [Fact]
    public async Task Write_rejects_unknown_status()
    {
        var (write, _, _) = Create();
        var result = await write.ExecuteAsync(
            """{"todos":[{"content":"a","status":"doing"}]}""",
            CancellationToken.None);
        Assert.StartsWith("Error", result);
        Assert.Contains("doing", result);
    }

    [Fact]
    public async Task Empty_list_clears_persisted_plan()
    {
        var (write, read, conversation) = Create();
        await write.ExecuteAsync(
            """{"todos":[{"content":"a","status":"pending"}]}""", CancellationToken.None);
        var cleared = await write.ExecuteAsync("""{"todos":[]}""", CancellationToken.None);

        Assert.Contains("cleared", cleared, StringComparison.OrdinalIgnoreCase);
        Assert.Null(conversation.TodoListJson);
        Assert.Contains("No todo list", await read.ExecuteAsync("{}", CancellationToken.None));
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Conversation _conversation;
        public FakeConversationRepository(Conversation conversation) => _conversation = conversation;

        public Task<Conversation?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
            => Task.FromResult<Conversation?>(Match(id, userId));

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, Guid userId, CancellationToken ct = default)
            => Task.FromResult<Conversation?>(Match(id, userId));

        public Task<IReadOnlyList<Conversation>> ListAsync(Guid userId, Guid? projectId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Conversation>>([_conversation]);

        public Task AddAsync(Conversation conversation, CancellationToken ct = default) => Task.CompletedTask;
        public void AddMessage(Message message) { }
        public void RemoveMessages(IEnumerable<Message> messages) { }
        public void Update(Conversation conversation) { }
        public void Remove(Conversation conversation) { }

        private Conversation? Match(Guid id, Guid userId)
            => _conversation.Id == id && _conversation.UserId == userId ? _conversation : null;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
    }
}
