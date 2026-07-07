# SequenceController

> **File:** `src/api/Gabriel.API/Controllers/SequenceController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("sequence")]
public class SequenceController : ControllerBase
```


Returns the catalog of pattern and palette identifiers that clients can pin as a skin override on a project or conversation. The lists are static and inexpensive to fetch, so clients typically retrieve them once per session to populate a skin picker UI. The endpoint lives under the Gabriel-Sequence-scoped surface and is not tied to any specific conversation or project; it simply delivers the available skin options. Access is restricted to authenticated users via the Authorize attribute, ensuring that only authorized clients can retrieve the catalog.

## Remarks
By centralizing skin data in SequenceCatalog, this endpoint ensures consistency of options across all clients and surfaces. The controller acts as a thin façade over the catalog, decoupling the UI's skin choices from entity-specific logic. If you need to validate client-supplied names, reference SequenceCatalog's validation helpers (IsKnownPattern/IsKnownPalette) in the API's broader surface.

## Notes
- The catalog is static; updates require changing SequenceCatalog and redeploying the service.
- There are no query parameters or filtering support at this time; to support filtering or pagination, consider extending the API in a future iteration.