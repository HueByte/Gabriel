# Adding a new controller

> *Workflow template auto-derived from 8 existing exemplar(s).*

Adding a new controller is the pattern to use when you need to expose a new set of HTTP endpoints in the Gabriel API surface. New controllers in this area are placed alongside existing controllers under src/api/Gabriel.API/Controllers and follow the established file and type naming seen in the examples; model your controller on the Reference implementation below.

## Reference implementation

The following is real code from src/api/Gabriel.API/Controllers/SequenceController.cs that a new controller instance can be modeled on:

```csharp
// Gabriel-Sequence-scoped endpoints that aren't anchored to a specific
// conversation or project. Today: the skin-picker catalog. The per-conversation
// / per-project sequence endpoints stay on their respective controllers
// (ConversationsController, ProjectsController) so they sit alongside the rest
// of those entities' surface.
[ApiController]
[Authorize]
[Route("sequence")]
public class SequenceController : ControllerBase
{
    // Returns the catalog of pattern + palette identifiers a client can pin
    // on a project / conversation as a "skin" override. The lists are static
    // and cheap - clients can fetch once per session.
    [HttpGet("catalog")]
    public ActionResult<SequenceCatalogResponse> GetCatalog()
        => Ok(new SequenceCatalogResponse(SequenceCatalog.Patterns, SequenceCatalog.Palettes));
}
```

## Where it lives

Place new controllers in the src/api/Gabriel.API/Controllers folder. Name the file with a trailing Controller suffix (for example, <Name>Controller.cs) and declare a corresponding public class named <Name>Controller. The provided exemplars (for example AuthController, ConversationsController, ProjectsController, etc.) follow this same placement and naming.

## Wiring

A registration or composition site for controllers was not detected in the provided wiring sites. To understand how controllers are organized and to copy consistent patterns, inspect the existing controllers in src/api/Gabriel.API/Controllers listed below.

## Existing examples

- [`AuthController`](../../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md)
- [`ConversationsController`](../../Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md)
- [`DiagnosticsController`](../../Code/src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md)
- [`MemoriesController`](../../Code/src/api/Gabriel.API/Controllers/MemoriesController.cs.md)
- [`ModelsController`](../../Code/src/api/Gabriel.API/Controllers/ModelsController.cs.md)
- [`ProjectFilesController`](../../Code/src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md)
- [`ProjectsController`](../../Code/src/api/Gabriel.API/Controllers/ProjectsController.cs.md)
- [`SequenceController`](../../Code/src/api/Gabriel.API/Controllers/SequenceController.cs.md)

---
*Synthesised by Aurion on 2026-07-08 05:47:04 UTC*
