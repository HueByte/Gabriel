# ModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/ModelCatalog.cs`  
> **Kind:** class

```csharp
public sealed class ModelCatalog : IModelCatalog
```


ModelCatalog is a sealed implementation of IModelCatalog that builds a catalog of available chat models by enumerating each IChatProvider's Models during construction. With singleton providers, the potentially expensive walk happens only once, and subsequent Resolve calls reuse the cached list to select a model quickly.

## Remarks
By collecting AvailableModel entries up front, it centralizes model availability and selection logic, decoupling consumers from provider-specific configuration. It also establishes a robust startup path: the first active model found during construction becomes the bootstrap default; if none are active, the first registered model is used. If no models are registered at all, the constructor throws to fail fast and surface misconfiguration instead of failing later at runtime.

## Notes
- Resolve matches a provider name case-insensitively and a model name exactly (ordinal). If a match is found, a corresponding ModelSelection is returned; otherwise the precomputed default is returned.
- If a stale preference (a provider/model pair no longer present) is supplied, ModelCatalog silently falls back to the default rather than throwing, preserving user experience when configurations change.
- The default selection is derived from the first active model if available, otherwise the first available model; if neither exists, an InvalidOperationException is thrown with guidance to register a Mock provider or configure at least one provider.