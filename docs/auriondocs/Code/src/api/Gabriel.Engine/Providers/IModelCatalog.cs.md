# IModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/IModelCatalog.cs`  
> **Kind:** interface

Read-only registry and resolver for models exposed by registered IChatProvider implementations. Use this interface when you need to enumerate available models or translate a user's preferred provider/model pair into a concrete ModelSelection without performing provider discovery or mutating the catalog.

## Remarks
This abstraction separates model discovery from model selection logic and keeps the contract read-only so test doubles and alternative implementations (for example, dynamic discovery) can be substituted. Resolution follows a clear precedence: a matching (provider + model) preference wins; otherwise the configuration-declared default (the model marked IsActive=true) is chosen; if no IsActive model exists the first registered model is used as a last resort.

## Example
```csharp
// List available models
foreach (var model in modelCatalog.AvailableModels)
{
    Console.WriteLine($"{model.Provider} / {model.Name}");
}

// Resolve a user's preference into a concrete selection
var selection = modelCatalog.Resolve(preferredProvider: "openai", preferredModel: "gpt-4");
Console.WriteLine($"Resolved provider: {selection.Provider}, model: {selection.Model}");
```

## Notes
- AvailableModels is exposed as an IReadOnlyList; callers should not attempt to modify it.
- Resolve requires both preferredProvider and preferredModel to match a registered model to take precedence; partial matches (only provider or only model) fall back to the default selection behavior.
- The interface documentation describes a fallback to the "first registered model" if no IsActive entry exists; the behavior when no models are registered at all is not specified in the source comments and should be handled or clarified by consumers/implementations.
