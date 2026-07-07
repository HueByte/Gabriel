# Adding a new controller

> *Workflow template auto-derived from 8 existing exemplar(s).*

Adding a new controller is the pattern to use when you need to expose a new set of HTTP endpoints in the API surface alongside the existing controllers. Reach for this pattern when the endpoints are a cohesive group (e.g., project-level, conversation-level, or cross-cutting sequence endpoints) and can be modelled after an existing controller. The examples in src/api/Gabriel.API/Controllers show the shape and attributes commonly used for controllers in this codebase.

## Reference implementation

The following is real code from src/api/Gabriel.API/Controllers/SequenceController.cs that a new controller can be modelled on:

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

Controllers in this area of the codebase live under src/api/Gabriel.API/Controllers. The existing files use a "{Name}Controller.cs" filename pattern (for example, SequenceController.cs, ProjectsController.cs) with the primary symbol named accordingly (e.g., SequenceController, ProjectsController).

## Wiring

A specific registration or composition site for controllers was not detected in the provided inputs. If you need to wire a new controller into the running app, consult the existing controllers listed below as examples for how they are organized and referenced; the precise registration location was not identified from the symbol graph provided.

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
*Synthesised by Aurion on 2026-07-07 21:08:34 UTC*
