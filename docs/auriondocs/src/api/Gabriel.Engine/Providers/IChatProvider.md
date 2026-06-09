# IChatProvider

> **File:** `src/api/Gabriel.Engine/Providers/IChatProvider.cs`  
> **Kind:** interface

Represents a streaming chat-capable LLM provider that yields an asynchronous sequence of ChatProviderEvent items (text deltas, tool-call completions, finalization events). Use this when the agent or UI must react incrementally to model output and tool invocations rather than waiting for a single completed response.

## Remarks
This abstraction exists to support ReAct-style agent loops and responsive UIs: providers emit events as the model and any called tools produce partial output so the application can update the user or make decisions mid-turn. Model selection is resolved by the caller (the agent supplies a concrete modelName per call); the provider itself is expected to be stateless with respect to that choice. The Name property is a stable identifier used by the provider registry and stored on user preferences, while Models exposes the provider's catalog (it may be empty for providers not meant to appear in a model picker).

## Example
```csharp
// Consume streamed events from a provider and handle them incrementally
IChatProvider provider = chatProviderRegistry.Resolve("grok-provider");
CancellationTokenSource cts = new CancellationTokenSource();

await foreach (var ev in provider.StreamAsync(history, tools, modelName, cts.Token))
{
    // ev is a ChatProviderEvent (e.g. text delta, tool result, finished)
    // Update UI, call tool handlers, or append to response buffer as events arrive.
    HandleChatProviderEvent(ev);
}

// Remember to cancel cts if you need to abort the streaming operation.
```

## Notes
- The returned IAsyncEnumerable begins producing events only when enumerated; failing to consume it may keep resources allocated.
- Honor the CancellationToken: callers should pass one to allow prompt shutdown; implementers should stop streaming when the token is cancelled.
- Name is treated as a stable identifier (used by the registry and persisted as a user preference); do not change it for an existing provider implementation without migration.
- Models can be empty to indicate the provider should not appear in a model selection UI.
- Providers must emit incremental events (text deltas, tool completion events, finish) so callers can react mid-stream rather than waiting for a single final string.