# MockChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`  
> **Kind:** class

```csharp
public class MockChatProvider : IChatProvider
```


MockChatProvider is a test implementation of IChatProvider that streams a canned, deterministic reply to simulate a live chat provider without invoking a real LLM. It yields the reply word-by-word to exercise the streaming path, and on the first user turn (if any tools are registered) it emits a ToolCallReadyEvent for the first tool to demonstrate the ReAct loop, then completes the interaction with a FinishEvent indicating ToolCalls. Use this mock during development to validate the UI, streaming, and tool-invocation wiring without credentials or real model usage.

## Remarks
By design, this provider is not intended for production; it exists to exercise end-to-end flow in isolation. It uses the same event types as the real provider (TextDeltaEvent, ToolCallReadyEvent, FinishEvent), so consuming code can be tested without depending on a real model. The embedded mock's small context window (8k tokens) and the note that it is not IsDefault (the real provider wins bootstrap when configured) help keep dev scenarios lightweight while preserving the real provider selection semantics.

## Example
```csharp
// Example: consuming the mock stream
var provider = new MockChatProvider();
var history = new List<ChatProviderMessage>();
var tools = new List<ToolDescriptor> { new ToolDescriptor { Name = "FakeTool" } };

await foreach (var ev in provider.StreamAsync(history, tools, "mock-default"))
{
    // handle events as they arrive (TextDeltaEvent, ToolCallReadyEvent, FinishEvent, etc.)
    // e.g., accumulate TextDeltaEvent.Text, react to ToolCallReadyEvent, etc.
}
```

## Notes
- The Templates array in the snippet uses square brackets; in valid C# initialization, braces should be used (e.g., new string[] { ... }). This is the minimal syntactic correction needed for compilation.
- The mock emits a ToolCallReadyEvent on the first turn when tools are provided, which intentionally exercises the tool-invocation path of the host application. If there are no tools, the mock will proceed to stream a canned reply and finish.
