using Gabriel.API.Contracts.Projects;
using Gabriel.API.Mapping;
using Gabriel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

[ApiController]
[Authorize]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
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
        // as null, so this is a small simplification — see the comment in the
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
}
