using Gabriel.API.Contracts.Projects;
using Gabriel.API.Contracts.Sequence;
using Gabriel.API.Mapping;
using Gabriel.Core.Services;
using Gabriel.Engine.Sequence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

[ApiController]
[Authorize]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly IGabrielSequenceService _sequence;

    public ProjectsController(IProjectService projects, IGabrielSequenceService sequence)
    {
        _projects = projects;
        _sequence = sequence;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> List(CancellationToken ct)
    {
        var projects = await _projects.ListAsync(ct);
        return Ok(projects.Select(p => p.ToResponse(includeFiles: false)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Get(Guid id, CancellationToken ct)
    {
        var project = await _projects.GetWithFilesAsync(id, ct);
        return Ok(project.ToResponse(includeFiles: true));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var project = await _projects.CreateAsync(request.Name, request.Description, request.SystemPrompt, ct);
        return CreatedAtAction(nameof(Get), new { id = project.Id }, project.ToResponse(includeFiles: false));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        // Patch semantics: only update fields that were supplied. The DTO is
        // all-nullable; explicit null on `Description` / `SystemPrompt` clears
        // them, missing keys leave them alone (JSON deserialization treats both
        // as null, so this is a small simplification - see the comment in the
        // PATCH design note for the future explicit-clear behavior).
        var project = await _projects.GetAsync(id, ct);
        if (request.Name is not null) project = await _projects.RenameAsync(id, request.Name, ct);
        if (request.Description is not null) project = await _projects.UpdateDescriptionAsync(id, request.Description, ct);
        if (request.SystemPrompt is not null) project = await _projects.UpdateSystemPromptAsync(id, request.SystemPrompt, ct);

        return Ok(project.ToResponse(includeFiles: false));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _projects.DeleteAsync(id, ct);
        return NoContent();
    }

    // Project-shared Gabriel Sequence. Driven by Project.AvatarSeed plus the
    // Live State from the project's most-recently-active conversation. See
    // IGabrielSequenceService.GetForProjectAsync for the aggregation rule.
    // Clients should hit this for projects where IsDefault == false; the
    // Default-project bucket falls back to per-conversation sequences.
    [HttpGet("{id:guid}/sequence")]
    public async Task<ActionResult<GabrielSequenceResponse>> GetSequence(Guid id, CancellationToken ct)
    {
        var sequence = await _sequence.GetForProjectAsync(id, ct);
        return Ok(sequence.ToResponse());
    }

    // Re-rolls Project.AvatarSeed. The pinned pattern / palette overrides
    // (if any) survive - reroll only changes the seed-derived dimensions of
    // the avatar. Mirrors the per-conversation reroll under
    // ConversationsController.
    [HttpPost("{id:guid}/avatar/reroll")]
    public async Task<ActionResult<ProjectResponse>> RerollAvatar(Guid id, CancellationToken ct)
    {
        var project = await _projects.RerollAvatarAsync(id, ct);
        return Ok(project.ToResponse(includeFiles: false));
    }

    // Pin (or clear) the project's avatar skin - pattern + palette identifiers
    // from the catalog. PUT semantics: both fields are taken as the full
    // intended state, null on either dimension clears it back to seed-derived.
    // Unknown / unrecognized identifiers are rejected as 400 so the client
    // doesn't silently lose the user's pick.
    [HttpPut("{id:guid}/skin")]
    public async Task<ActionResult<ProjectResponse>> SetSkin(
        Guid id,
        [FromBody] SetSkinRequest request,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Pattern) && !SequenceCatalog.IsKnownPattern(request.Pattern))
            return BadRequest(new { detail = $"Unknown pattern '{request.Pattern}'." });
        if (!string.IsNullOrWhiteSpace(request.Palette) && !SequenceCatalog.IsKnownPalette(request.Palette))
            return BadRequest(new { detail = $"Unknown palette '{request.Palette}'." });

        var project = await _projects.SetSkinAsync(
            id,
            SequenceCatalog.NormalizePattern(request.Pattern),
            SequenceCatalog.NormalizePalette(request.Palette),
            ct);
        return Ok(project.ToResponse(includeFiles: false));
    }
}
