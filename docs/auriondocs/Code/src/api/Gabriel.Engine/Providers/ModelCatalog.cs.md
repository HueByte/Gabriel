# ModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/ModelCatalog.cs`  
> **Kind:** class

Builds an immutable catalog of available chat models by walking the Models lists of the provided IChatProvider instances at construction time. Use this class when you need a central, read-only view of all registered models and a deterministic way to resolve a runtime ModelSelection from optional user preferences (provider + model). The catalog is constructed once and then used for fast lookups.

## Remarks
ModelCatalog collects AvailableModel entries from every IChatProvider supplied to its constructor and keeps them in a read-only list. It establishes a default ModelSelection during construction: the first model marked IsActive across providers, or if none are marked active, the first registered model. If no models are present at all the constructor throws an InvalidOperationException. After construction the catalog is effectively immutable and safe for concurrent reads.

## Example
```csharp
// Typical usage in a DI scenario: the container supplies all registered IChatProvider implementations
var catalog = new ModelCatalog(providers);

// Read all known models (used to populate UI dropdowns, etc.)
IReadOnlyList<AvailableModel> models = catalog.AvailableModels;

// Resolve a selection from user preferences (falls back to the catalog default when preference is missing or stale)
ModelSelection selection = catalog.Resolve(preferredProvider: "openai", preferredModel: "gpt-4");
```

## Notes
- Provider name comparisons are case-insensitive, but model name comparisons use an ordinal (case-sensitive) comparison; a mismatch in model name casing will be treated as "stale" and cause a fallback to the default.
- If a preferred provider/model pair does not match any registered entry, Resolve silently returns the catalog's default selection rather than throwing — the UI should reflect the actual selection on the next load.
- The constructor expects that at least one provider exposes a non-empty Models list; otherwise an InvalidOperationException is thrown. The implementation expects a "Mock" provider to be available in minimally configured deployments to avoid this error.
- The catalog is populated once at creation; subsequent changes to provider registrations or their Models collections will not be reflected in an existing ModelCatalog instance.