# IChatProvider

> **File:** `src/api/Gabriel.Engine/Providers/IChatProvider.cs`  
> **Kind:** interface

An abstraction for chat-capable LLM providers that emits a stream of incremental events (text deltas, tool-call completions, finish signals) so callers can react as the model produces output. Use this when you need a provider-agnostic, per-call model selection and incremental output delivery — for example in interactive UIs or ReAct-style agent loops where blocking for a full response would hurt responsiveness.

## Remarks
IChatProvider separates model selection from provider implementation: callers resolve a PreferredModel into a concrete modelName and pass it to StreamAsync, while implementations remain stateless with respect to that choice. The interface exposes a stable Name (used by the provider registry and stored on user preferences) and a Models catalog for UI discovery; a provider may intentionally leave Models empty to opt out of being shown in pickers. StreamAsync returns an `IAsyncEnumerable<ChatProviderEvent>` so consumers can process partial output and tool events incrementally and cancel via CancellationToken.

## Example
```csharp
// Consume incremental events from a provider (simplified)
IAsyncEnumerable<ChatProviderEvent> stream = provider.StreamAsync(history, tools, modelName, ct);
await foreach (var evt in stream.WithCancellation(ct))
{
    // Handle different event kinds (text delta, tool call, finish) here.
    // This lets the UI render partial text and react to tool invocations
    // without waiting for the provider to produce a final response.
}
```

## Notes
- StreamAsync is incremental: callers must use await foreach (or otherwise enumerate the IAsyncEnumerable) to receive partial output rather than expecting a single completed response.
- An empty Models list is a signal that the provider should not be advertised in model pickers; Name remains the canonical identifier for resolution and user preferences.
- Honor the CancellationToken: providers and callers should respect cancellation to avoid UI hangs or leaked operations.