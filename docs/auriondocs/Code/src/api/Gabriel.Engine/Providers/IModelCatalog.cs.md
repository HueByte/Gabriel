# IModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/IModelCatalog.cs`  
> **Kind:** interface

```csharp
public interface IModelCatalog
```


The IModelCatalog interface provides a read-only catalog of every model exposed by all registered IChatProvider implementations and exposes the resolver the agent uses to map a user’s preferences into a concrete ModelSelection. It remains an interface to enable test doubles and future discovery strategies (such as dynamic model discovery) to be swapped in without changing callers.

## Remarks
This abstraction centralizes model discovery and user-preference resolution, decoupling the agent's selection logic from concrete provider implementations. It exposes an immutable list of available models and a single Resolve entry point that enforces the precedence rules: an exact match on the provided preferences wins; if there is no match, the default is the config-declared IsActive model; if that also does not exist, the first registered model is used as a last resort. This design makes it straightforward to mock or substitute the catalog in tests and to evolve discovery strategies over time without impacting callers.

## Notes
- Fallback semantics depend on the IsActive flag on AvailableModel; if none exist, the first registered model is used as a last resort.
- Null or unmatched preferences are treated as non-matches and trigger the defaulting behavior.
