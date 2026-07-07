# SequenceController

> **File:** `src/api/Gabriel.API/Controllers/SequenceController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("sequence")]
public class SequenceController : ControllerBase
```


SequenceController exposes sequence-scoped endpoints that are not tied to a specific conversation or project. Today it hosts the skin-picker catalog, accessible via GET /sequence/catalog. The endpoint returns a catalog of pattern and palette identifiers that clients can pin as a skin override on a project or conversation. The data is static and inexpensive to fetch, so clients should retrieve it once per session and cache it for UI rendering.

## Remarks
This controller provides a lean, static surface dedicated to the skin system, decoupled from per-entity controllers. By isolating the catalog from ConversationsController and ProjectsController, the API surfaces a stable resource that can be cached and reused across sessions without depending on any particular conversation or project state. The endpoint is protected by authorization, reflecting that skin configuration is part of the authenticated user experience rather than public data.

## Example
```csharp
// Fetch the catalog (requires authentication)
using var http = new HttpClient { BaseAddress = new Uri("https://api.example.com/sequence/") };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<token>");
var resp = await http.GetAsync("catalog");
resp.EnsureSuccessStatusCode();
var catalog = await resp.Content.ReadFromJsonAsync<SequenceCatalogResponse>();
// Use catalog.Patterns and catalog.Palettes to populate UI
```

## Notes
- The catalog data is static and does not change per request; changes require redeploys or explicit cache invalidation.
- This endpoint is read-heavy and side-effect free; avoid issuing excessive requests beyond session-wide caching.
- Access requires authentication due to the [Authorize] attribute on the controller.