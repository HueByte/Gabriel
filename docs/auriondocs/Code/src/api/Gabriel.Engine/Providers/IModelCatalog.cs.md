# IModelCatalog

> **File:** `src/api/Gabriel.Engine/Providers/IModelCatalog.cs`  
> **Kind:** interface

```csharp
public interface IModelCatalog
```


IModelCatalog is a read-only contract that exposes every model offered by all registered IChatProvider implementations, together with the resolver the agent uses to map a user’s preference into a concrete ModelSelection. The interface provides a stable way to enumerate available models via the AvailableModels collection and to resolve a user’s choices into a specific ModelSelection without tying callers to a particular discovery mechanism. Because it is an interface, various implementations (including static registrations or dynamic discovery) can swap in without affecting call sites.

## Remarks
This abstraction centralizes model discovery and selection behind a stable contract, enabling tests and future improvements (such as dynamic model discovery) to swap implementations with minimal impact. It encodes a clear, deterministic policy for model selection: a user’s explicit matches take precedence, followed by the config-declared default (the model with IsActive = true), and finally a deterministic fallback to the first registered model when no default exists. The read-only AvailableModels surface also makes it straightforward to expose the catalog to configuration UIs or validation logic without risking mutation.

## Example
```csharp
// Example usage demonstrating preferred lookup and fallback behavior
void Demonstrate(IModelCatalog catalog)
{
    // Prefer a specific provider/model
    ModelSelection preferred = catalog.Resolve("OpenAIProvider", "gpt-4");

    // Fallback to the default/first-registered model when no preferences are provided
    ModelSelection fallback = catalog.Resolve(null, null);
}
```
