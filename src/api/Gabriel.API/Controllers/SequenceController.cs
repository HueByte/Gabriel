using Gabriel.API.Contracts.Sequence;
using Gabriel.Engine.Sequence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

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
    // and cheap — clients can fetch once per session.
    [HttpGet("catalog")]
    public ActionResult<SequenceCatalogResponse> GetCatalog()
        => Ok(new SequenceCatalogResponse(SequenceCatalog.Patterns, SequenceCatalog.Palettes));
}
