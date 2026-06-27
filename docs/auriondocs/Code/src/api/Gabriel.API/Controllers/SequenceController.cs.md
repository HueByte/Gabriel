# SequenceController

> **File:** `src/api/Gabriel.API/Controllers/SequenceController.cs`  
> **Kind:** class

Returns a small set of read-only, top-level Gabriel sequence endpoints that are not tied to a specific conversation or project. Primarily used to retrieve the static catalog of available pattern and palette identifiers a client can apply as a "skin" override; the catalog is cheap to fetch and intended to be requested once per client session.

## Remarks
This controller centralizes sequence-related endpoints that are global to the application rather than scoped to a conversation or project. Per-conversation and per-project sequence endpoints remain on their respective controllers (ConversationsController, ProjectsController) so those behaviors stay colocated with their domain logic. SequenceController is an ApiController that requires authentication (Authorize) and exposes small, read-only endpoints intended for low-cost client consumption.

## Example
```csharp
// Example: fetching the catalog with HttpClient and System.Text.Json
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");

var res = await client.GetAsync("sequence/catalog");
res.EnsureSuccessStatusCode();

var json = await res.Content.ReadAsStringAsync();
var catalog = System.Text.Json.JsonSerializer.Deserialize<SequenceCatalogResponse>(json);
// use catalog.Patterns and catalog.Palettes
```

## Notes
- The controller is decorated with [Authorize]; callers must be authenticated.
- The catalog lists are static and inexpensive to produce — clients are expected to fetch them once per session and may cache them.
- The endpoint is read-only (GET) and returns a SequenceCatalogResponse wrapped in a 200 OK.
