namespace Gabriel.Engine.Tools;

// Scoped per-turn context the agent populates before executing tools. Tools
// that need to know "which conversation / project am I acting on?" read it
// here instead of being passed the values through their JSON args (which the
// model would otherwise have to fill in).
//
// Registered as Scoped so a single HTTP request's tools all see the same
// values. The AgentService sets the context once per turn before invoking
// any tool.
public interface IToolExecutionContext
{
    Guid? ConversationId { get; }
    Guid? UserId { get; }
    Guid? ProjectId { get; }

    void Set(Guid conversationId, Guid userId, Guid? projectId);
}

public sealed class ToolExecutionContext : IToolExecutionContext
{
    public Guid? ConversationId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ProjectId { get; private set; }

    public void Set(Guid conversationId, Guid userId, Guid? projectId)
    {
        ConversationId = conversationId;
        UserId = userId;
        ProjectId = projectId;
    }
}
