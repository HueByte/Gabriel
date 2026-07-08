# LLMProviderOptions

> **File:** `src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs`  
> **Kind:** class

```csharp
public abstract class LLMProviderOptions
```


An abstract base class that provides the provider-wide configuration used by all LLM integrations. It consolidates transport, authentication, resilience, sampling, and the model catalog so concrete provider option classes can remain thin and focus on binding to their configuration section (SectionName).

GetDefaultModel and FindModel(string) offer quick access to the active/default model and to locate a model by name from the Models collection.

## Remarks
This abstraction isolates provider-level concerns from per-model details, enabling easy provider swapping without duplicating common settings. The Models catalog serves both the UI (model picker) and runtime model resolution; GetDefaultModel() supplies a sensible fallback by selecting the first active model, while FindModel(string) enables targeted retrieval by name. Concrete provider options typically derive from this base and simply set SectionName to guide configuration binding.

## Notes
- Models is a mutable list initialized to an empty collection; changes after configuration binding may affect default-model resolution unless performed during initialization-time setup.
- ApiKey and BaseUrl are sensitive configuration values; supply them via secure sources (environment variables, user secrets, or protected vaults) rather than hard-coding defaults. 