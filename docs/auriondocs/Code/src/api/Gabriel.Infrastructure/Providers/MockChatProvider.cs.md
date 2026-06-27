# MockChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`  
> **Kind:** class

Streams a simple, canned LLM-like reply token-by-token (word-by-word) for development and testing. Use this provider when you need an offline substitute to exercise the streaming event flow and the agent/tool-calling loop (for example in UI/dev builds where a real LLM provider is not configured). The provider also demonstrates a single automated ToolCallReadyEvent on the first user turn if any tools are registered.

## Remarks
This class exists purely as a development/testing shim: it exposes a "mock" model in the catalog so the UI picker can show a provider, uses a small context window (8,000 tokens) for compact behavior, and is intentionally not marked active by default. Its StreamAsync implementation either (a) emits a ToolCallReadyEvent followed immediately by a FinishEvent with FinishReason.ToolCalls when the conversation has no prior tool results and registered tools exist, or (b) emits incremental TextDeltaEvent values for a canned, formatted reply and then a FinishEvent with FinishReason.Stop. The streaming is simulated by splitting the reply on spaces and yielding each word with a short delay to exercise consumers that expect deltas over time.

## Example
```csharp
// Consume the mock provider's stream and handle the most common events.
await foreach (var ev in mockProvider.StreamAsync(history, tools, "mock-default", cancellationToken))
{
    switch (ev)
    {
        case ToolCallReadyEvent call:
            Console.WriteLine($"Tool requested: {call.Name}, args: {call.ArgumentsJson}");
            // Invoke the tool and append its result to the conversation history.
            break;

        case TextDeltaEvent delta:
            Console.Write(delta.Content); // delta.Content contains a word + trailing space
            break;

        case FinishEvent finish:
            Console.WriteLine($"\nFinished: {finish.Reason}");
            break;
    }
}
```

## Notes
- The provider ignores the modelName parameter and always uses the same canned behavior.
- A ToolCallReadyEvent is emitted only once per conversation if there are no prior MessageRole.Tool entries in history and tools.Count > 0; in that case the stream finishes immediately with FinishReason.ToolCalls.
- Replies are constructed from the last user message (truncated to 80 characters) and split on spaces; each yielded TextDeltaEvent contains a single word plus a trailing space and is emitted with a small delay. CancellationToken is honored during the delays.
