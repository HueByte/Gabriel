# IModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/IModelCatalog.cs`  
> **Kind:** interface

Read-only view of every model exposed by registered IChatProvider implementations and a small resolver that maps a user's provider/model preference into a concrete ModelSelection. Use this interface when consumers need to enumerate available models or determine which model the system will choose for a given user preference without depending on mutable provider state.

## Remarks
This interface is intentionally read-only and minimal so different implementations (static lists, test doubles, or dynamic discovery) can be swapped in without changing callers. It centralizes the selection logic: Resolve prefers an exact match for the provided preference pair, then falls back to the configuration-declared default (the model marked IsActive=true), and finally to the first registered model if no active default exists.

## Example
```csharp
// Inspect available models
var models = modelCatalog.AvailableModels;
foreach (var m in models)
{
    Console.WriteLine($"{m.Provider}:{m.Name} (Active={m.IsActive})");
}

// Ask the catalog which model will be used for a user's preference
var selection = modelCatalog.Resolve(preferredProvider: "providerA", preferredModel: "gpt-4");
Console.WriteLine($"Selected provider={selection.Provider}, model={selection.Model}");
```

## Notes
- Resolve accepts nullable preference strings; a null preferredProvider or preferredModel simply means the corresponding preference is unspecified and the resolver will apply its fallback rules.
- The AvailableModels collection is exposed as IReadOnlyList to prevent mutation by callers; the interface does not promise thread-affinity or live updates — concrete implementations control concurrency and refresh semantics.
