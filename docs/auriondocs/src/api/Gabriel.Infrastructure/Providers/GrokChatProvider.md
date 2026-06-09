Streams chat completions from xAI's OpenAI-compatible /v1/chat/completions endpoint and exposes the response as a sequence of ChatProviderEvent values. Use this provider when you need token- or delta-level streaming (text deltas, reasoning deltas) and structured handling of model-initiated tool calls instead of making a single blocking completion request.

## Remarks
This class is an infrastructure-level adapter that translates the service's server-sent-event (SSE)-style streaming chunks into the application's ChatProviderEvent model. It resolves a named HttpClient ("Grok") per call via IHttpClientFactory so HTTP handler lifetimes and DNS refresh are handled by the DI-managed pool, and it snapshots the configured model catalog at construction so the UI can present a stable model picker. The StreamAsync implementation accumulates piecewise tool-call fragments until the model signals the tool-calls are finished, emits delta events as they arrive, and yields a FinishEvent on terminal conditions.

## Example
```csharp
// Iterate the streaming events and handle the common event types
await foreach (var ev in grokProvider.StreamAsync(history, tools, "grok-1" , cancellationToken))
{
    switch (ev)
    {
        case TextDeltaEvent t:    Console.Write(t.Content); break;
        case ReasoningDeltaEvent r: Console.WriteLine($"[reasoning] {r.Content}"); break;
        case ToolCallEvent tc:    /* invoke tool by name / args */ break;
        case FinishEvent f:       Console.WriteLine($"Finished: {f.Reason}"); break;
    }
}
```

## Notes
- The provider expects a named HttpClient registered as "Grok" and GrokOptions configured at startup; missing DI registration will cause client resolution or requests to fail.
- Models exposes a snapshot (List) taken at construction; updating GrokOptions after construction does not change this list.
- The streaming parser only processes lines prefixed with "data:" and will log and skip malformed JSON chunks; consumers should be prepared for partial or missing deltas and for seeing a FinishEvent(FinishReason.Error) when the HTTP response is non-success rather than an exception.