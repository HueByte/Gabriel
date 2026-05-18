namespace Gabriel.Core.Services;

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
}
