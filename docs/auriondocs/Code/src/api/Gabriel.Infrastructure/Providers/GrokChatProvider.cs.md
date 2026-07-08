# GrokChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`  
> **Kind:** class

```csharp
public class GrokChatProvider : IChatProvider
```


Streams chat completions from xAI's OpenAI-compatible /v1/chat/completions endpoint and exposes them as an IAsyncEnumerable of ChatProviderEvent. Use this provider when you need progressive, token- or chunk-level updates (reasoning deltas, text deltas, and assembled tool calls) instead of waiting for a single completed response. The provider resolves a named HttpClient (constant HttpClientName = "Grok") from IHttpClientFactory for each StreamAsync call, reads the response as a streaming body, parses server-sent `data:` chunks into StreamChunk objects and yields the appropriate ChatProviderEvent instances. On non-success HTTP responses it logs the error and yields a FinishEvent with FinishReason.Error.

## Remarks
GrokChatProvider encapsulates the streaming and HTTP plumbing required to speak to xAI while keeping DI configuration and connection lifetime concerns out of the caller. It intentionally resolves the named HttpClient per call (via IHttpClientFactory) so the underlying handlers remain pooled and refresh DNS/lifetimes correctly. The Models property returns a snapshot of the configured model catalog (a ToList copy taken from GrokOptions) so the UI can populate a model picker without depending on mutation of the options object.

The implementation expects chunked SSE-style output: lines prefixed with "data:" containing JSON-serialized StreamChunk objects. It tolerates and logs malformed JSON chunks and skips them rather than failing the whole stream. Tool call fragments are accumulated by index until the stream emits the chunk that indicates tool calls are finished, at which point assembled tool-call events are emitted.

## Notes
- Models returns a copy of the configured list (ToList()); updates to GrokOptions after construction are not reflected in this collection.
- The stream is driven by enumerating the IAsyncEnumerable; if the caller does not iterate StreamAsync, no request will be sent.
- Tool calls are emitted only after the provider receives the terminal chunk for that tool-call sequence; if the service never sends that terminal chunk, those tool calls will not be emitted.