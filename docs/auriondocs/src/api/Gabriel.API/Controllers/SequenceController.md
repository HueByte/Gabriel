# SequenceController

> **File:** `src/api/Gabriel.API/Controllers/SequenceController.cs`  
> **Kind:** class

Exposes application-level Sequence endpoints that are not tied to a specific conversation or project. Currently it provides a single read-only endpoint (/sequence/catalog) returning the static skin-picker catalog (pattern and palette identifiers) that clients can use to present or pin a "skin" override for a conversation or project.

## Remarks
This controller separates sequence-scoped API surface that applies across the app from per-conversation or per-project endpoints (which remain on their respective controllers). The catalog returned is static and inexpensive to produce, so clients are expected to fetch it once per session and cache it rather than polling frequently.

## Example
```csharp
// Example: fetch the catalog and read pattern/palette ids
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");
var response = await client.GetAsync("https://api.example.com/sequence/catalog");
response.EnsureSuccessStatusCode();
var json = await response.Content.ReadAsStringAsync();
var catalog = System.Text.Json.JsonSerializer.Deserialize<SequenceCatalogResponse>(json);
// catalog.Patterns and catalog.Palettes contain the identifiers to display to users
```

## Notes
- The endpoint is protected by authorization; include valid credentials (Bearer token) when calling it.
- The returned lists are global/static (not scoped to a project or conversation) and intended to be cached client-side.
- This controller's surface is intentionally minimal — additions here should be for sequence-wide functionality that doesn't belong on Conversation/Project controllers.