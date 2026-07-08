# MockChatProvider

> **File:** `src/api/Gabriel.Infrastructure/Providers/MockChatProvider.cs`  
> **Kind:** class

```csharp
public class MockChatProvider : IChatProvider
```


MockChatProvider is a development-time mock implementation of IChatProvider. It streams canned, word-delimited replies to simulate a real provider without credentials and, on the first user turn when tools are present, kicks off the ReAct loop by emitting a ToolCallReadyEvent for the first tool before continuing with text deltas.

## Remarks
MockChatProvider serves as a lightweight stand-in to exercise the streaming interaction path and tool invocation in the UI and integration tests. It configures a single mock LLM model (mock-default) and uses simple templates to produce predictable, repeatable replies while keeping the real provider's interface intact, which makes it easy to swap in a real provider via DI without changing the consuming code.

## Notes
- The Templates array initializer uses square brackets, which is invalid C# (should be an array initializer with curly braces).