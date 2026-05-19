using Gabriel.API.Contracts.Projects;
using Gabriel.API.Mapping;
using Gabriel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

[ApiController]
[Authorize]
[Route("projects/{projectId:guid}/files")]
public class ProjectFilesController : ControllerBase
{
    private readonly IProjectFileService _files;

    public ProjectFilesController(IProjectFileService files)
    {
        _files = files;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectFileResponse>>> List(Guid projectId, CancellationToken ct)
    {
        var files = await _files.ListAsync(projectId, ct);
        return Ok(files.Select(f => f.ToResponse()).ToList());
    }

    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> Download(Guid projectId, Guid fileId, CancellationToken ct)
    {
        var (file, content) = await _files.OpenAsync(projectId, fileId, ct);
        // FileStreamResult disposes the underlying stream after the response
        // is written, so we don't `using`/`await using` it here.
        return new FileStreamResult(content, file.ContentType)
        {
            FileDownloadName = file.Name,
        };
    }

    // Multipart upload. Form key: `file`. Single-file upload to keep the
    // request shape simple - batch upload can come later if needed.
    //
    // No `[FromForm]` on the IFormFile parameter: Swashbuckle blows up on the
    // combo when generating OpenAPI ("Error reading parameter(s) … as
    // [FromForm] attribute used with IFormFile"). [ApiController] infers form
    // binding for IFormFile parameters automatically, so dropping the explicit
    // attribute keeps both the runtime AND OpenAPI generation happy.
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]   // belt-and-suspenders cap on the request body
    public async Task<ActionResult<ProjectFileResponse>> Upload(
        Guid projectId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { detail = "File is required and cannot be empty." });
        }

        await using var stream = file.OpenReadStream();
        var entity = await _files.UploadAsync(projectId, file.FileName, file.ContentType, stream, ct);
        return CreatedAtAction(nameof(Download), new { projectId, fileId = entity.Id }, entity.ToResponse());
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid fileId, CancellationToken ct)
    {
        await _files.DeleteAsync(projectId, fileId, ct);
        return NoContent();
    }
}
