using Gabriel.API.Contracts.Models;
using Gabriel.Core.Identity;
using Gabriel.Engine.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

// Model selector backend. The UI dropdown reads from GET and PUTs the user's
// pick; the agent layer resolves the pick on every turn via IModelCatalog +
// IUserPreferences so a change takes effect on the next message with no
// reload required.
[ApiController]
[Authorize]
[Route("models")]
public class ModelsController : ControllerBase
{
    private readonly IModelCatalog _catalog;
    private readonly IUserPreferences _userPrefs;

    public ModelsController(IModelCatalog catalog, IUserPreferences userPrefs)
    {
        _catalog = catalog;
        _userPrefs = userPrefs;
    }

    // Returns every model the catalog knows about plus whichever entry will
    // be used for this user on the next turn (config IsActive default if
    // they haven't picked yet, or the stored preference).
    [HttpGet]
    public async Task<ActionResult<ModelsResponse>> List(CancellationToken ct)
    {
        var prefs = await _userPrefs.GetAsync(ct);
        var selection = _catalog.Resolve(prefs.PreferredProvider, prefs.PreferredModel);

        var models = _catalog.AvailableModels
            .Select(m => new ModelDto(
                Provider: m.Provider,
                Name: m.Name,
                ContextWindowTokens: m.ContextWindowTokens,
                InputPricePerMTokens: m.InputPricePerMTokens,
                OutputPricePerMTokens: m.OutputPricePerMTokens,
                CacheReadPricePerMTokens: m.CacheReadPricePerMTokens,
                CacheWritePricePerMTokens: m.CacheWritePricePerMTokens,
                IsDefault: m.IsDefault,
                IsSelected: string.Equals(m.Provider, selection.Provider, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(m.Name, selection.Name, StringComparison.Ordinal)))
            .ToList();

        return Ok(new ModelsResponse(
            AvailableModels: models,
            Selected: new SelectedModelDto(selection.Provider, selection.Name)));
    }

    // Persists the user's pick. Pass null/null in the body to clear the
    // preference and fall back to the config default. Validates that the
    // (provider, name) tuple exists in the catalog so a UI sending a stale
    // value gets a clean 400 instead of silently writing junk that the
    // catalog will then ignore on the next read.
    [HttpPut("active")]
    public async Task<ActionResult<ModelsResponse>> SetActive(
        [FromBody] SetActiveModelRequest request,
        CancellationToken ct)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Body is required." });
        }

        var bothNull = string.IsNullOrWhiteSpace(request.Provider) && string.IsNullOrWhiteSpace(request.Name);
        var bothSet = !string.IsNullOrWhiteSpace(request.Provider) && !string.IsNullOrWhiteSpace(request.Name);

        if (!bothNull && !bothSet)
        {
            return BadRequest(new { error = "Provider and Name must be supplied together, or both omitted to clear." });
        }

        if (bothSet)
        {
            var known = _catalog.AvailableModels.Any(m =>
                string.Equals(m.Provider, request.Provider, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(m.Name, request.Name, StringComparison.Ordinal));
            if (!known)
            {
                return BadRequest(new { error = $"Model '{request.Provider}/{request.Name}' is not registered." });
            }
        }

        await _userPrefs.SetPreferredModelAsync(request.Provider, request.Name, ct);
        return await List(ct);
    }
}
