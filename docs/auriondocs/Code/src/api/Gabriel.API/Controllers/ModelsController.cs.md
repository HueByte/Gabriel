# ModelsController

> **File:** `src/api/Gabriel.API/Controllers/ModelsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("models")]
public class ModelsController : ControllerBase
```


This API controller provides the model selector logic for the UI, returning all registered models and the one that will be used on the next turn based on the current user's preferences, with fallback to the catalog default. It also persists a user's chosen model via a guarded PUT, validating the pair against the catalog so changes apply on the next interaction without a reload.

## Remarks
This symbol focuses on combining catalog data with per-user preferences to determine the active model without requiring a page reload. It sits between the UI layer and the model catalog, ensuring that user choices are validated against the catalog and resolved on every interaction. The separation allows changing how models are stored or resolved without touching the UI code.

## Notes
- Clearing a preference is accomplished by sending a body with both Provider and Name omitted (empty/whitespace). Sending a completely null body yields a 400 error.
- The input (provider, name) pair is validated against the catalog; if the pair isn't registered, the API returns a 400 with a descriptive error.
- IsSelected uses a case-insensitive comparison for the provider field while the model name must match exactly.