# ModelsController

> **File:** `src/api/Gabriel.API/Controllers/ModelsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("models")]
public class ModelsController : ControllerBase
```


ModelsController is an ASP.NET Core API controller that exposes endpoints for discovering available model configurations and persisting a per-user selection. It depends on IModelCatalog to enumerate models and on IUserPreferences to resolve and store the user's chosen provider/name. The GET /models returns all known models along with a Selected model indicating what will be used on the next turn—the selection is computed by resolving the user's preferences against the catalog; if the user hasn't chosen, the catalog default is used. The PUT /models/active validates input, ensures the requested (provider, name) exists in the catalog, and writes the choice to the user's preferences, returning the updated list so the UI can refresh immediately.

## Remarks
This controller acts as the thin UI-facing bridge between the catalog and per-user preferences. It centralizes validation and persistence of the active model, ensuring the UI can present a stable, consistently populated dropdown and that invalid selections are rejected explicitly rather than being ignored. By reusing List's result after a change, it guarantees the client sees the current selection reflected immediately without requiring a separate fetch.

## Notes
- To clear the current selection, send a body with both Provider and Name omitted or whitespace; a null body is rejected with a 400 error. This design prevents silent writes and ensures the catalog's state remains valid.
- If you provide only one of Provider or Name, the request is rejected with a 400 and a clear error message: "Provider and Name must be supplied together, or both omitted to clear." 
- If the (Provider, Name) pair is not registered in the catalog, a 400 is returned with an explicit error: "Model '<provider>/<name>' is not registered."
