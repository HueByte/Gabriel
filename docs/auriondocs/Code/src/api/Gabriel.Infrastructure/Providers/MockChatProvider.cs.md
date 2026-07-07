# MockChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`  
> **Kind:** class

```csharp
public class MockChatProvider : IChatProvider
```


MockChatProvider is a lightweight test double that implements IChatProvider by streaming canned, word-delimited replies, enabling UI and flow testing without a real LLM. It also exposes a mock model via the UI model catalog and, on the first turn when tools are present, emits a ToolCallReadyEvent to demonstrate the tool-invocation path before continuing with text deltas and a final Finish event.

## Remarks
It exists to provide a deterministic, fast proxy for development and automated tests so you can exercise the ReAct loop end-to-end without relying on external services. The provider is not meant for production; IsActive is false in the included model so real providers defined in the configuration still bootstrap when configured. It wires a single mock model with a compact context window, and it deliberately triggers a tool call on the first turn when tools are supplied to illustrate the end-to-end interaction between the chat provider, the agent, and tools.

## Example
```csharp
// Example: consuming the mock stream in a simple app
var provider = new MockChatProvider();
var history = new List<ChatProviderMessage>();
var tools = new List<ToolDescriptor> { new ToolDescriptor { Name = "echo" } };

await foreach (var e in provider.StreamAsync(history, tools, "mock-default"))
{
    // Handle events (TextDeltaEvent, ToolCallReadyEvent, FinishEvent) as they arrive.
}
```

## Notes
- The Templates initialization in the snippet uses bracket syntax which would not compile in a real C# file; treat this class as a mock for development/testing.
- This provider is a non-production stand-in meant to exercise the chat-to-tool flow and streaming UI; swap it out for a real provider via DI in a deployed environment.
- When tools are provided, the first stream turn emits a ToolCallReadyEvent followed by a FinishEvent to demonstrate the tool invocation path before continuing with text deltas.
