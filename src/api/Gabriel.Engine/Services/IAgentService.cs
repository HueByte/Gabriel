namespace Gabriel.Engine.Services;

public interface IAgentService
{
    // Runs the ReAct loop for a single user turn. Pre-flight validation
    // (empty input, missing conversation) throws synchronously so the API layer
    // can respond with the right HTTP status before SSE headers are sent.
    // In-stream failures are yielded as AgentError events.
    Task<IAsyncEnumerable<AgentEvent>> RunAsync(
        Guid conversationId,
        string userInput,
        CancellationToken ct = default);

    // Edit-and-resend: removes the given user message and everything after it
    // (Conversation.TruncateFrom), then runs a normal turn with newContent -
    // same events as RunAsync, starting with userMessagePersisted for the
    // replacement message. Pre-flight throws for: empty content, not found,
    // not a user message.
    Task<IAsyncEnumerable<AgentEvent>> EditAndRunAsync(
        Guid conversationId,
        Guid userMessageId,
        string newContent,
        CancellationToken ct = default);

    // Asks the conversation's running turn to stop. The turn ends gracefully on
    // its own stream: the partial reply is persisted and arrives as a normal
    // assistantMessage, then done. False when no turn of the current user's is
    // running for that conversation.
    bool StopTurn(Guid conversationId);

    // Regenerates the assistant reply at the given message id. The existing
    // variant (and any tool aftermath linked to it via tool_call_id) is marked
    // inactive; a new variant is produced and shares the same VariantGroupId
    // so the UI can offer a picker between the alternatives. Pre-flight throws
    // for: not found, not an assistant message, or already-inactive variant.
    Task<IAsyncEnumerable<AgentEvent>> RegenerateAsync(
        Guid conversationId,
        Guid assistantMessageId,
        CancellationToken ct = default);

    // Snapshot of the context-window state for the given conversation -
    // exposes the numbers MaybeCompactAsync compares against the configured
    // threshold so the UI can render a matching progress indicator.
    Task<ContextMetrics> GetContextMetricsAsync(
        Guid conversationId,
        CancellationToken ct = default);
}
