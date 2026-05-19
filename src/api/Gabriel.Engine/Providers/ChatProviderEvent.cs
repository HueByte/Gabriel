namespace Gabriel.Engine.Providers;

// Streaming events emitted by IChatProvider.StreamAsync. The provider buffers
// partial tool-call deltas internally and only emits ToolCallReady once a call
// is fully assembled, so consumers don't need to reassemble JSON fragments.
public abstract record ChatProviderEvent;

// Incremental assistant-text token. Many of these arrive per turn; consumers
// concatenate them to form the final message content.
public sealed record TextDeltaEvent(string Delta) : ChatProviderEvent;

// Incremental REASONING token (the "thinking" stream). Surfaced by Grok 4,
// DeepSeek-R1, OpenAI o-series, Anthropic extended-thinking, etc. as a
// separate stream from the final answer. Providers that don't support a
// reasoning channel simply never emit these events.
//
// Consumers should treat reasoning as the model's internal chain-of-thought:
// useful for transparency / debugging / UI thinking displays, but not the
// "answer". Persistence is separate from the final assistant content.
public sealed record ReasoningDeltaEvent(string Delta) : ChatProviderEvent;

// A complete, ready-to-execute tool call.
public sealed record ToolCallReadyEvent(string Id, string Name, string ArgumentsJson) : ChatProviderEvent;

// Terminal event - the provider has nothing more to emit for this turn.
public sealed record FinishEvent(FinishReason Reason) : ChatProviderEvent;

public enum FinishReason
{
    Stop,        // Assistant finished naturally with content
    ToolCalls,   // Assistant wants to call tools - agent loop should execute them and re-stream
    Length,      // Hit max tokens / model output limit
    Error,       // Provider-side failure
}
