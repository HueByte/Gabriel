A streaming chat provider that wraps xAI's OpenAI-compatible /v1/chat/completions endpoint and exposes incremental events as an IAsyncEnumerable<ChatProviderEvent>. Use this when you want token- or chunk-level streaming of model reasoning, text deltas and tool-invocation information rather than waiting for a final completion.

## Remarks
GrokChatProvider resolves a named HttpClient (HttpClientName = "Grok") from IHttpClientFactory for each call so the underlying handler pooling and DNS refresh behavior is preserved. It snapshots the configured model catalog from GrokOptions at construction time to present a stable Models list for UI population. The StreamAsync method consumes an SSE-style streaming response (lines beginning with "data:"), deserializes JSON chunks into StreamChunk objects, and yields typed ChatProviderEvent instances (e.g. ReasoningDeltaEvent, TextDeltaEvent, FinishEvent) as data arrives.

## Example
```csharp
// provider is obtained from DI (IChatProvider / GrokChatProvider registered)
IChatProvider provider = /* resolve from IServiceProvider */;
var cts = new CancellationTokenSource();
await foreach (var ev in provider.StreamAsync(history, tools, "grok-small", cts.Token))
{
    switch (ev)
    {
        case TextDeltaEvent t: Console.Write(t.Content); break;
        case ReasoningDeltaEvent r: /* append/display reasoning */ break;
        case FinishEvent f: /* check f.Reason and finalize */ break;
        default: /* handle other provider-specific events (e.g. tool calls) */ break;
    }
}
```

## Notes
- The HTTP client must be registered under the GrokChatProvider.HttpClientName ("Grok") so the provider can create it via IHttpClientFactory.
- A non-success HTTP response is logged and results in an immediate FinishEvent with reason Error; no further events are produced.
- Malformed JSON chunks are skipped and logged; the stream continues unless the server sends the terminal "[DONE]" marker.
- Models is a snapshot taken at construction; updates to configuration after construction are not reflected in GrokChatProvider.Models.
- StreamAsync expects SSE-style lines beginning with "data:" and uses ResponseHeadersRead to stream incrementally — consumers should respect the provided CancellationToken to stop reading promptly.