# IChatProvider

> **File:** `src/api/Gabriel.Engine/Providers/IChatProvider.cs`  
> **Kind:** interface

```csharp
public interface IChatProvider
```


IChatProvider defines a streaming abstraction for LLM backends; implementors yield a sequence of ChatProviderEvent as the conversation unfolds, enabling incremental UI updates and ReAct-style tool integration. Model selection is performed per-call by passing a modelName, while the provider remains stateless with respect to the chosen model.

## Remarks

This interface separates the concerns of transport and orchestration: the provider handles streaming events from a given model, while the surrounding system supplies the conversation history and available tools, then reacts to events as they arrive. The Name property serves as a stable identifier for registry and user preferences, and Models exposes the catalog of models the provider can serve so the UI can present options. By keeping the provider stateless regarding model choice, per-call decisions can be made by the caller (e.g., based on user preferences or config) without requiring the provider to manage per-model state.

## Notes

- Use await foreach to consume the `IAsyncEnumerable<ChatProviderEvent>` to keep the UI responsive; respect CancellationToken to cancel streaming when the user navigates away or the operation is aborted.
- Validate modelName against provider.Models before passing it in; mismatch may lead to errors or degraded behavior, depending on the provider implementation.
- Remember that IChatProvider is stateless with respect to model selection, so each StreamAsync call is independent with respect to the chosen model; keep any cross-call state in the surrounding orchestration layer if needed.