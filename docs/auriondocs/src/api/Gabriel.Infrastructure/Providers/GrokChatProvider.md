# GrokChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/GrokChatProvider.cs`  
> **Kind:** class

Provides a streaming IChatProvider implementation that talks to xAI's OpenAI-compatible /v1/chat/completions endpoint (branded here as "Grok"). Use this provider when you need token-by-token and tool-call streaming from the xAI service rather than getting a single completed response. The provider expects a named HttpClient (HttpClientName = "Grok") to be configured via DI with base URL, timeouts and authentication.

## Remarks
This class resolves an IHttpClientFactory per call so the underlying HttpMessageHandler pool stays healthy (DNS refresh, lifetime management). It snapshots the configured model catalog from GrokOptions at construction and exposes it via Models so UI components can present a stable list. The StreamAsync method implements server-sent-event style streaming: it reads lines starting with the "data:" prefix, deserializes JSON chunks into StreamChunk instances, emits delta events (reasoning/text) as they arrive, accumulates piecewise tool-call fragments by index and emits completed tool calls once the stream signals them, and yields a FinishEvent on error or when the stream ends.

## Example
```csharp
// Consume streaming events from the provider
await foreach (var ev in grokProvider.StreamAsync(history, tools, "grok-small", ct))
{
    switch (ev)
    {
        case TextDeltaEvent t: Console.Write(t.Content); break;
        case ReasoningDeltaEvent r: /* handle reasoning */ break;
        case ToolCallEvent tc: /* invoke tool */ break;
        case FinishEvent f: Console.WriteLine($"Finished: {f.Reason}"); break;
    }
}
```

## Notes
- The Models property is a snapshot taken at construction; changes to GrokOptions after construction are not reflected.
- StreamAsync expects server-sent-event lines prefixed with "data:" and treats a line of "[DONE]" as stream termination.
- Malformed JSON chunks are skipped with a warning (the provider logs and continues) — consumers should tolerate missing or delayed deltas.
- If the initial HTTP response is non-successful the provider logs the response body and yields a FinishEvent with an error reason.
- Callers should pass a CancellationToken when enumerating StreamAsync to ensure the underlying HTTP stream is closed promptly on cancellation.
```