# IChatProvider

> **File:** `src/api/Gabriel.Engine/Providers/IChatProvider.cs`  
> **Kind:** interface

```csharp
public interface IChatProvider
```


IChatProvider defines a streaming contract for chat providers that can yield incremental events to the agent loop. Implementations expose StreamAsync, which returns an async stream (IAsyncEnumerable) of ChatProviderEvent objects as history, tools, and a per-call modelName are processed. This enables incremental UI updates and responsive ReAct-style flows, since text deltas, tool results, and finish signals can be observed as they arrive. The provider is stateless with respect to the chosen model; the caller resolves the user’s PreferredModel (or the config default) to a concrete model name and passes it in per call.

## Remarks
By abstracting away the concrete provider from the agent orchestration, this interface enables easy swapping, testing, and mock implementations while keeping model selection at the call site. It also centralizes streaming semantics (how deltas and results are surfaced) so the rest of the system can react uniformly to provider events regardless of provider.

## Example
```csharp
// Example: consuming streamed events from a provider
IChatProvider provider = GetProvider("Mock");
IReadOnlyList<ChatProviderMessage> history = GetHistory();
IReadOnlyList<ToolDescriptor> tools = GetTools();
string modelName = "gpt-4";

await foreach (ChatProviderEvent ev in provider.StreamAsync(history, tools, modelName))
{
    // Handle ev (e.g., delta text, tool invocation, or completion)
}
```

## Notes
- Honor CancellationToken; implementers should stop streaming when ct is canceled.
- Ensure modelName corresponds to a model in provider.Models; mismatches should be handled gracefully.
- Providers with an empty Models collection should still be implemented gracefully; the UI should not present such providers in the model picker.