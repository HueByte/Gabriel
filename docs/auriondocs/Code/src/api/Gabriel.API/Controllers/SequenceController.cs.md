# SequenceController

> **File:** `src/api/Gabriel.API/Controllers/SequenceController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("sequence")]
public class SequenceController : ControllerBase
```


Exposes a global API endpoint that returns the catalog of available skin identifiers (patterns and palettes) for pinning a skin override on a project or conversation. The data comes from the static SequenceCatalog and is wrapped in a lightweight SequenceCatalogResponse; because the lists are static, clients can fetch them once per session without incurring significant overhead. This endpoint is intentionally sequence-scoped and sits under the sequence route, separating it from per-conversation or per-project endpoints that reside on their respective controllers.

## Remarks
This centralized catalog pattern avoids duplicating the same options across multiple controllers and ensures a consistent set of skin options across the API surface. The API surface is decoupled from internal storage: changes to SequenceCatalog or the response wrapper won't require client changes, other than fetching updated data. Access is guarded by Authorization, reinforcing that skin customization is a user-facing feature tied to a validated session.

## Notes
- No query parameters or per-user filtering; results are global and static.
- If the catalog updates, clients should re-fetch to see new options; the endpoint does not push updates.