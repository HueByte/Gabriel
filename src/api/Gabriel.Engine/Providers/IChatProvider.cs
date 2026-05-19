namespace Gabriel.Engine.Providers;

// Streaming abstraction over LLM providers (mock, Grok/xAI, OpenAI, etc).
// Providers yield a sequence of events (text deltas, completed tool calls, finish)
// so the agent loop can react incrementally - important for ReAct flows where
// every tool call would otherwise block the UI for seconds.
public interface IChatProvider
{
    string Name { get; }

    // Total tokens the provider's current model can handle (input + output).
    // Used by AgentService to decide when to trigger rolling compact.
    int ContextWindowTokens { get; }

    IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        CancellationToken ct = default);
}
