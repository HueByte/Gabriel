# IModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/IModelCatalog.cs`  
> **Kind:** interface

```csharp
public interface IModelCatalog
```


IModelCatalog exposes a read-only view of all models provided by registered IChatProvider implementations and a resolver that maps a user’s preferences to a concrete ModelSelection. The interface remains extensible so tests and future implementations (for example, dynamic model discovery) can swap in alternate realizations without changing call sites. Use this whenever you need to present the available models to a user and to determine which model should handle a request, relying on Resolve to apply a deterministic policy that translates preferences into a ModelSelection.

## Remarks
The interface centralizes model discovery and selection logic, decoupling consumers from the details of how models are gathered and chosen. Resolve encodes the precedence: an exact provider/model match wins; otherwise the configured default (the IsActive=true model) is used; if none exists, the first registered model is used as a last resort. This encapsulation makes testing and future extensions straightforward—new discovery strategies or selection policies can be plugged in behind the same interface.

## Example
```csharp
// Typical usage: determine which model to use for a user’s session
IModelCatalog catalog = serviceProvider.GetRequiredService<IModelCatalog>();
ModelSelection selection = catalog.Resolve(user.PreferredProvider, user.PreferredModel);
```

## Notes
- Resolve follows a deterministic fallback: exact match by provider/model first; if no match, fall back to the default IsActive model; if there is no IsActive entry, use the first registered model. Ensure at least one model is registered to avoid an undefined outcome.
- AvailableModels is a read-only list; callers should not attempt to mutate the collection. Implementations may still update the underlying view, but consumer code should rely on the IReadOnlyList contract.
- If the underlying set of models changes, callers relying on a single Resolve call should be prepared for a different result on subsequent calls; treat the catalog as a dynamic source of truth rather than a static snapshot.