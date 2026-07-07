# GrokChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`  
> **Kind:** class

```csharp
public class GrokChatProvider : IChatProvider
```


Streams chat completions from xAI's OpenAI-compatible /v1/chat/completions endpoint and exposes them via `IAsyncEnumerable<ChatProviderEvent>`. Use this provider when you need token- or delta-level streaming (including partial reasoning content and tool-call fragments) rather than waiting for a final non-streaming completion. The provider resolves a named HttpClient (HttpClientName = "Grok") from an IHttpClientFactory on each StreamAsync call and emits ChatProviderEvent instances as the server sends incremental chunks.

## Remarks
This class is a thin, streaming adapter between Gabriel's IChatProvider abstraction and xAI's streaming chat API. It parses server-sent event (SSE)-style lines, deserializes StreamChunk payloads, emits incremental events for reasoning and text deltas, and accumulates piecewise tool-call fragments until a completion chunk indicates the tool call is finished. Creating the HttpClient via IHttpClientFactory per call preserves connection pooling and handler lifetime behavior (DNS refresh, handler recycling) while snapshotting the configured model catalog at construction time exposes a stable Models list for UI consumption.

## Notes
- Enumeration is lazy/cold: the HTTP request is made when you start iterating the IAsyncEnumerable returned by StreamAsync.  
- Non-success HTTP responses are logged and cause the provider to yield a FinishEvent with FinishReason.Error and then complete.  
- Malformed JSON chunks are skipped with a warning; the stream continues unless the server signals completion ("[DONE]").  
- Tool call fragments are accumulated by index and only emitted once the provider receives the final chunk that completes the tool call; consumers should not expect fully-formed tool-call events for every intermediate chunk.