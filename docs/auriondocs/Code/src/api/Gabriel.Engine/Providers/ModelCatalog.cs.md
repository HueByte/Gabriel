# ModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/ModelCatalog.cs`  
> **Kind:** class

```csharp
public sealed class ModelCatalog : IModelCatalog
```


ModelCatalog is a concrete, sealed implementation of IModelCatalog that builds a catalog of all available chat models by enumerating every registered IChatProvider.Models during construction. Because providers are registered as singletons, the catalog is built once and reused for subsequent resolutions, avoiding repeated traversal of provider lists. It exposes the full set of models via AvailableModels and resolves a ModelSelection through an optional pair of preferred provider and model; if no valid match is found, it falls back to a precomputed default model.

## Remarks

Centralizes model discovery and selection, decoupling clients from provider-specific details and ensuring a sane, deterministic default path if configuration is incomplete. The default is determined at construction time (the first active model, or the first available if none are active) and reused for all resolves, which promotes a stable startup experience even in misconfigured environments. The catalog is built once and kept immutable thereafter, providing fast lookups without re-walking providers on every call.

## Example
```csharp
// Most common usage: explicitly select a provider/model
var catalog = new ModelCatalog(providers);
var selection = catalog.Resolve("OpenAI", "GPT-4");

// Or rely on the precomputed default when preferences are omitted or unavailable
var defaultSelection = catalog.Resolve(null, null);
```

## Notes
- Provider name comparisons are case-insensitive, while model name comparisons are exact (case-sensitive).
- If no models are registered across all providers, the constructor throws InvalidOperationException with guidance to register a Mock or configure at least one provider.
- AvailableModels is a read-only list and does not change after construction; Resolve uses a cached _default when no valid match is found.