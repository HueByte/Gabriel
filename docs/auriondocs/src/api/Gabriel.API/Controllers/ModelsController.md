# ModelsController

> **File:** `src/api/Gabriel.API/Controllers/ModelsController.cs`  
> **Kind:** class

Returns the list of available models and lets an authenticated user persist a preferred (provider, name) model selection. The controller surfaces the catalog's AvailableModels and the user's current selection (resolved via IModelCatalog + IUserPreferences), and exposes a PUT endpoint to update the user's preference. Use this controller when implementing a UI model picker so the frontend can read all options and save or clear the user's choice.

## Remarks
ModelsController coordinates between the model catalog and per-user preferences: it ensures the UI sees every registered model and the exact model that will be used on the next turn (either the stored preference or the catalog default). It validates writes so the preferences store only references catalog-registered models and returns the updated model list after a change, allowing the client to refresh state without a separate follow-up request. The endpoints require authentication ([Authorize]) and accept a CancellationToken.

## Example
```csharp
// GET the list of models
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");
var listResponse = await client.GetAsync("models");
listResponse.EnsureSuccessStatusCode();
var body = await listResponse.Content.ReadAsStringAsync();
Console.WriteLine(body);

// Set active model
var setPayload = new { provider = "openai", name = "gpt-4" };
var putResponse = await client.PutAsJsonAsync("models/active", setPayload);
putResponse.EnsureSuccessStatusCode();
var updated = await putResponse.Content.ReadAsStringAsync();
Console.WriteLine(updated);

// Clear preference (both provider and name must be omitted or null)
var clearPayload = new { provider = (string?)null, name = (string?)null };
var clearResponse = await client.PutAsJsonAsync("models/active", clearPayload);
clearResponse.EnsureSuccessStatusCode();
```

## Notes
- Provider comparison is case-insensitive while model Name comparison is case-sensitive; supply the exact model Name as registered in the catalog.
- The PUT body is required; to clear the preference send both Provider and Name as null/empty together. Supplying only one of the two fields results in a 400 Bad Request.
- The controller validates that the (provider, name) tuple exists in the catalog and returns a 400 with an error message if it does not; after a successful set it returns the same ModelsResponse as GET so clients receive the canonical, up-to-date view.