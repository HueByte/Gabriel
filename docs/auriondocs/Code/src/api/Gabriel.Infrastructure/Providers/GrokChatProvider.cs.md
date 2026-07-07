# GrokChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`  
> **Kind:** class

```csharp
public class GrokChatProvider : IChatProvider
```


Streams chat completions from xAI's OpenAI-compatible /v1/chat/completions endpoint and yields incremental ChatProviderEvent items (text deltas, reasoning deltas, tool call events, finish/error). Use this provider when you need low-latency, token- or chunk-level streaming from Grok models and want the HTTP plumbing (base URL, timeout, auth) managed via a named HttpClient configured in DI.

## Remarks
GrokChatProvider encapsulates the HTTP streaming mechanics and event parsing so callers receive a sequence of high-level events instead of raw HTTP/SSE frames. It resolves a named HttpClient from IHttpClientFactory per call to preserve handler health and DNS refresh semantics, and exposes a snapshot of available models from GrokOptions for UI model selection. The implementation accumulates piecewise tool-call fragments (indexed by call) and emits complete tool call events only when the provider receives the terminal chunk, which simplifies consumers that react to tool invocations.

## Notes
- The stream is read line-by-line and expects server lines prefixed with "data:"; malformed JSON chunks are logged and skipped rather than failing the whole stream.
- If the HTTP response is not successful the provider logs the response body, yields a FinishEvent with FinishReason.Error, and ends the stream.
- Models is a snapshot taken at construction from configuration; updates to the underlying options after construction are not reflected in the Models property.