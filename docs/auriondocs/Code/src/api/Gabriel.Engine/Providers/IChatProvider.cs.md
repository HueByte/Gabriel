# IChatProvider

> **File:** `src/api/Gabriel.Engine/Providers/IChatProvider.cs`  
> **Kind:** interface

```csharp
public interface IChatProvider
```


IChatProvider defines a streaming, pluggable backend for chatting with LLM providers. It yields a sequence of ChatProviderEvent values incrementally through StreamAsync, enabling the agent loop to react in real time (e.g., for ReAct-style workflows) and surface partial results to the UI. Model selection is per-call: the agent resolves the user's PreferredModel (or the config default) into a modelName and passes it to StreamAsync. The provider itself remains stateless with respect to that choice, allowing interchangeable backends without embedding per-call state.

## Remarks
IChatProvider serves as a clean abstraction boundary between the agent orchestration and the concrete chat backends. Implementations can range from mocks to production providers (e.g., Grok/xAI, OpenAI) without requiring callers to know specifics. The interface also enables testability and easier swapping via IChatProviderRegistry and the model/catalog infrastructure: a provider is chosen by name, models are offered by the catalog, and the agent supplies the history and tools for streaming.

## Notes
- The StreamAsync method returns an asynchronous stream; callers should observe the CancellationToken to cancel streaming promptly.
- Providers are expected to be stateless with respect to the chosen model/provider; any per-call state should live in the history/tools passed in.
- The Name property must be a stable, unique identifier that IChatProviderRegistry.Resolve can use to select the provider.
