namespace Gabriel.Core.Providers;

// Streaming events emitted by IChatProvider.StreamAsync. The provider buffers
// partial tool-call deltas internally and only emits ToolCallReady once a call
// is fully assembled, so consumers don't need to reassemble JSON fragments.
public abstract record ChatProviderEvent;

// Incremental assistant-text token. Many of these arrive per turn; consumers
// concatenate them to form the final message content.
public sealed record TextDeltaEvent(string Delta) : ChatProviderEvent;

// A complete, ready-to-execute tool call.
public sealed record ToolCallReadyEvent(string Id, string Name, string ArgumentsJson) : ChatProviderEvent;

// Terminal event — the provider has nothing more to emit for this turn.
public sealed record FinishEvent(FinishReason Reason) : ChatProviderEvent;

public enum FinishReason
{
    Stop,        // Assistant finished naturally with content
    ToolCalls,   // Assistant wants to call tools — agent loop should execute them and re-stream
    Length,      // Hit max tokens / model output limit
    Error,       // Provider-side failure
}
