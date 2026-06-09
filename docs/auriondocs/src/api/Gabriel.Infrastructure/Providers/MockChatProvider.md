# MockChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`  
> **Kind:** class

```csharp
// Stand-in for a real LLM. Streams a canned text reply word-by-word so the
// streaming flow is exercisable without xAI credentials, and on the first
// user turn (when tools are available) calls the first registered tool to
// demonstrate the ReAct loop.
public class MockChatProvider : IChatProvider
```


A development-only IChatProvider that streams a canned text reply word-by-word and, on the first user turn when tools are available, emits a single ToolCallReadyEvent to demonstrate the ReAct/tool-invocation loop. Use this provider in local development or tests when you need predictable streaming behavior and a way to exercise tool-calling without a real LLM.

## Remarks
This mock implements the same streaming contract as a real provider but remains intentionally simple: it exposes a single non-active model ("mock-default") so developer tooling can list a provider, ignores the provided modelName, and constructs a short canned reply based on the last user message (truncated to 80 characters). If the conversation history contains no Tool result message and at least one tool is registered, the provider emits a ToolCallReadyEvent followed by a FinishEvent(FinishReason.ToolCalls) once and stops; otherwise it streams a text reply by yielding TextDeltaEvent entries for each space-separated word and finishes with FinishEvent(FinishReason.Stop). The small, fixed delays and word-by-word deltas make UI streaming and cancellation behavior easy to exercise.

## Example
```csharp
// Read streamed events from the mock provider and react to a tool call or text deltas.
await foreach (var evt in mockProvider.StreamAsync(history, tools, "mock-default", ct))
{
    switch (evt)
    {
        case ToolCallReadyEvent tool:
            Console.WriteLine($"Tool requested: {tool.Name}, args: {tool.ArgumentsJson}");
            break;
        case TextDeltaEvent delta:
            Console.Write(delta.Text);
            break;
        case FinishEvent finish:
            Console.WriteLine($"\nFinished: {finish.Reason}");
            break;
    }
}
```

## Notes
- This provider is not a replacement for a real LLM: replies are deterministic, short, and intentionally simplistic.
- It ignores the modelName parameter — the Models list exists for discovery only.
- The reply is split on spaces and emitted word-by-word with small delays; punctuation remains attached to the preceding word.
- CancellationToken is observed for the artificial delays used during streaming.