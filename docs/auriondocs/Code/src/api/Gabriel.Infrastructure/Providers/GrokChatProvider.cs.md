GrokChatProvider is a streaming IChatProvider wrapper that drives the OpenAI-compatible /v1/chat/completions endpoint. It encapsulates the HTTP plumbing and streaming semantics so callers receive incremental deltas (reasoning and text) from the model, while also surfacing any tool invocations the model requests. The provider is designed for DI-driven applications: it resolves an HttpClient via IHttpClientFactory using a named client (Grok), reads a pre-built request body, and streams the response to the caller as a sequence of ChatProviderEvent objects. The catalog of available models is exposed via Models and is snapshot at construction from GrokOptions, allowing the UI to present a stable catalog without repeatedly touching configuration. This abstraction hides the low-level streaming protocol and keeps the rest of the system focused on rendering or orchestrating the resulting events.

## Remarks
GrokChatProvider centralizes the streaming protocol, deserialization, and event mapping for xAI-style chat. It translates server deltas into concrete events (ReasoningDeltaEvent and TextDeltaEvent) that a consumer can render in real time, while buffering and emitting tool calls when the server signals a tool_calls block. The design relies on a named HttpClient configured in DI and uses a startup-time model catalog to provide a stable UI surface, aligning with the application's DI and logging strategy and ensuring HTTP resource management via HttpClientFactory.

## Example
```csharp
// Example usage (simplified)
using var ct = new CancellationTokenSource().Token;
IReadOnlyList<ChatProviderMessage> history = ...; // initial conversation
IReadOnlyList<ToolDescriptor> tools = ...; // available tools
var provider = /* resolved via DI, e.g. IServiceProvider.GetRequiredService<GrokChatProvider>() */;
await foreach (var ev in provider.StreamAsync(history, tools, "Grok-XL", ct))
{
    switch (ev)
    {
        case ReasoningDeltaEvent r:
            Console.Write(r.ReasoningContent ?? string.Empty);
            break;
        case TextDeltaEvent t:
            Console.Write(t.Content ?? string.Empty);
            break;
        case FinishEvent f:
            // handle finish/failure
            break;
    }
}
```

## Notes
- The streaming protocol expects lines prefixed with data: and handles a terminal [DONE] marker; malformed chunks are skipped with a warning. 
- The provider uses HttpCompletionOption.ResponseHeadersRead for streaming efficiency and relies on the DI-configured Grok HttpClient to manage connections; ensure the named client is correctly registered. 
- Tool calls arrive in chunks and are buffered per index until a tool_calls section completes; the consumer should be prepared to handle asynchronous tool invocation results arriving at any point in the stream.
