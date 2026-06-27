A streaming chat provider that sends chat completion requests to xAI's Grok (OpenAI-compatible) /v1/chat/completions endpoint and exposes the response as an IAsyncEnumerable<ChatProviderEvent>. Use this provider when you need incremental updates (text deltas, reasoning deltas, and tool-call assembly) from Grok rather than waiting for a single completed response.

## Remarks
This class is a thin streaming adapter: it builds the request body, posts to the Grok endpoint using a named HttpClient (resolved per-call via IHttpClientFactory) and then parses the server-sent-events-like stream line-by-line. It intentionally resolves the HttpClient per call so the underlying pooled handler remains healthy (DNS refresh and lifetime management handled by IHttpClientFactory). Models are captured once at construction (a snapshot of GrokOptions.Models) so the UI/picker can display a stable catalog.

StreamAsync yields fine-grained ChatProviderEvent instances as the remote service emits them (text deltas, reasoning deltas, tool call fragments, and finish events). The implementation reads response headers first (HttpCompletionOption.ResponseHeadersRead) to start processing the stream immediately and tolerates malformed JSON chunks by logging and skipping them. Non-success HTTP responses are logged and result in a FinishEvent with an error reason.

## Example
```csharp
// Iterate streaming events from the provider and handle them as they arrive
var provider = serviceProvider.GetRequiredService<IChatProvider>();
await foreach (var evt in provider.StreamAsync(history, tools, modelName, cancellationToken))
{
    switch (evt)
    {
        case TextDeltaEvent t: Console.Write(t.Content); break;
        case ReasoningDeltaEvent r: /* show chain-of-thought incrementally */ break;
        case ToolCallEvent tc: /* invoke tool when complete */ break;
        case FinishEvent f when f.Reason == FinishReason.Error: /* handle error */ break;
    }
}
```

## Notes
- The DI setup must register a named HttpClient with the name "Grok" and configure authentication/timeout as expected; otherwise requests will fail.
- Models returns a snapshot taken at construction; changes to GrokOptions.Models after construction are not reflected.
- StreamAsync respects the provided CancellationToken; consumers should cancel to abort long-lived streams promptly.
- Malformed JSON chunks are skipped and logged; callers should not rely on every chunk being well-formed.