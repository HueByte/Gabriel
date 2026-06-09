# ModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/ModelCatalog.cs`  
> **Kind:** class

A concrete IModelCatalog that builds an immutable catalog of available chat models once at construction by walking the Models lists of all provided IChatProvider instances. Use this when you need a stable, read-only view of registered models and a deterministic way to resolve a ModelSelection from optional preferred provider/model strings; the catalog prefers the first model marked IsActive as the default, falling back to the first registered model if none are active.

## Remarks
ModelCatalog captures a snapshot of every provider's declared models at construction and retains that snapshot for the lifetime of the instance (the implementation assumes providers are singletons). Its primary roles are (1) to expose the available models for UI/dropdown rendering and (2) to produce a concrete ModelSelection either from a valid preference pair or from a safe built-in default so the agent can boot even when configuration is incomplete.

## Example
```csharp
// Build the catalog from registered providers
var providers = new IChatProvider[] { new MockProvider(), new OpenAiProvider() };
var catalog = new ModelCatalog(providers);

// Enumerate available models for a UI
foreach (var m in catalog.AvailableModels)
{
    Console.WriteLine($"{m.Provider}:{m.Name} (tokens={m.ContextWindowTokens})");
}

// Resolve a model selection from user preferences (falls back to default if not found)
var selection = catalog.Resolve(preferredProvider: "OpenAI", preferredModel: "gpt-4");
Console.WriteLine($"Selected provider: {selection.Provider}, model: {selection.Name}");
```

## Notes
- The catalog is a snapshot taken at construction; subsequent changes to IChatProvider.Models are not observed.
- Provider name matching is case-insensitive; model name matching uses ordinal (case-sensitive) comparison. A preference is considered only when both provider and model strings are non-empty.
- If no models are present across all providers an InvalidOperationException is thrown. If a preference references a model that no longer exists, Resolve silently falls back to the default selection.