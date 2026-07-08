# ModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/ModelCatalog.cs`  
> **Kind:** class

```csharp
public sealed class ModelCatalog : IModelCatalog
```


ModelCatalog collects all AvailableModel entries from registered IChatProvider instances into a single, cached catalog and resolves to a ModelSelection. It builds the catalog once during construction (leveraging singleton providers), so subsequent Resolve calls are cheap. Use Resolve with optional preferredProvider and preferredModel to select a specific model; if no exact match exists, the precomputed default is returned.

## Remarks
ModelCatalog centralizes model discovery across providers and caches the result to avoid repeated walking of models. It establishes a deterministic bootstrap default by selecting the first active model (IsActive) it encounters; if none are active, it falls back to the first registered model. If there are no models at all, construction throws InvalidOperationException to fail fast in misconfigured deployments. The resolver is side-effect-free from the caller's perspective: Resolve returns a fresh ModelSelection that reflects either the explicit match or the default, without mutating internal state.

## Notes
- If no models are registered, the constructor throws InvalidOperationException with a message indicating misconfiguration (e.g., no chat models registered; suggest registering Mock or at least one provider).
- Provider-name matching in Resolve is case-insensitive, but model-name matching is exact (Ordinal), which means case differences in the model name will prevent a match even if the provider name matches.
- The catalog is built once at construction time and remains a read-only snapshot for the lifetime of the ModelCatalog instance; changes to providers after construction will not be reflected unless a new ModelCatalog is created.