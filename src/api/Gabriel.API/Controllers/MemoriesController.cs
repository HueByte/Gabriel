using Gabriel.API.Contracts.Memories;
using Gabriel.Core.Entities;
using Gabriel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

// CRUD surface over IMemoryService for the settings page. List + create
// (upsert) + delete are the bread and butter; updates go through the same
// POST so the agent and the UI hit a single idempotent endpoint.
[ApiController]
[Authorize]
[Route("memories")]
public class MemoriesController : ControllerBase
{
    private readonly IMemoryService _memories;

    public MemoriesController(IMemoryService memories)
    {
        _memories = memories;
    }

    // GET /api/memories?projectId=... — scope filter. Omit projectId for the
    // user-scope list; pass a real Guid to see one project's memories. Pass
    // the literal string "all" to merge user-scope + the project's entries
    // (the same view the agent sees for that conversation).
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemoryDto>>> List(
        [FromQuery] string? scope,
        [FromQuery] Guid? projectId,
        CancellationToken ct)
    {
        IReadOnlyList<MemoryEntry> entries;
        if (string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase))
        {
            entries = await _memories.ListForConversationAsync(projectId, ct);
        }
        else
        {
            entries = await _memories.ListAsync(projectId, ct);
        }

        return Ok(entries.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemoryDto>> Get(Guid id, CancellationToken ct)
    {
        var entry = await _memories.GetByIdAsync(id, ct);
        if (entry is null) return NotFound();
        return Ok(ToDto(entry));
    }

    // Upsert. (UserId, ProjectId, Name) is the natural key; sending the same
    // request twice just bumps UpdatedAt. The agent's memory_save tool hits
    // the same code path under the hood.
    [HttpPost]
    public async Task<ActionResult<MemoryDto>> Save(
        [FromBody] SaveMemoryRequest request,
        CancellationToken ct)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Body is required." });
        }
        if (!Enum.TryParse<MemoryEntryType>(request.Type, ignoreCase: true, out var type))
        {
            return BadRequest(new { error = $"Type must be one of: user, feedback, project, reference (got '{request.Type}')." });
        }
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { error = "Name, Description, and Body must all be non-empty." });
        }

        var saved = await _memories.SaveAsync(
            new MemoryEntrySpec(request.ProjectId, type, request.Name, request.Description, request.Body),
            ct);

        return Ok(ToDto(saved));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var removed = await _memories.RemoveAsync(id, ct);
        return removed ? NoContent() : NotFound();
    }

    private static MemoryDto ToDto(MemoryEntry m) => new(
        Id: m.Id,
        ProjectId: m.ProjectId,
        Type: m.Type.ToString().ToLowerInvariant(),
        Name: m.Name,
        Description: m.Description,
        Body: m.Body,
        CreatedAt: m.CreatedAt,
        UpdatedAt: m.UpdatedAt);
}
