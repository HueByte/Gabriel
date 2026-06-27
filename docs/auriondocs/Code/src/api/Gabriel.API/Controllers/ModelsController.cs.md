# ModelsController

> **File:** `src/api/Gabriel.API/Controllers/ModelsController.cs`  
> **Kind:** class

Returns every model the catalog knows about and lets an authenticated user view or change their preferred model selection. Use the GET endpoint to fetch the catalog plus which model will be used for the user's next turn, and use PUT /models/active to persist or clear the user's preference so the agent layer will pick it up on the next message.

## Remarks
This controller provides a thin HTTP surface that composes two application services: IModelCatalog (the source of truth for available models and resolution rules) and IUserPreferences (persistence for a user's chosen provider/model). The GET action resolves the effective selection (either the stored preference or the catalog-configured default) and returns all catalog entries annotated with IsSelected. The PUT action validates the supplied provider/name tuple against the catalog to avoid storing stale or invalid choices, persists the preference, and then returns the same ModelsResponse (the updated list/selection).

## Example
```csharp
// Example unit-style invocation (using fakes/mocks for IModelCatalog and IUserPreferences):
var controller = new ModelsController(mockCatalog.Object, mockUserPrefs.Object);
// GET the list
var listResult = await controller.List(CancellationToken.None);
// Set a new active model
var setRequest = new SetActiveModelRequest { Provider = "example", Name = "model-v1" };
var putResult = await controller.SetActive(setRequest, CancellationToken.None);
```

## Notes
- Validation: The PUT body must either supply both Provider and Name or supply neither (both null/empty) to clear the preference; supplying exactly one causes a 400 Bad Request.
- Existence check: When both Provider and Name are supplied, the controller checks the catalog and returns 400 if the tuple is not registered (prevents writing invalid preferences).
- Comparison semantics: Provider comparisons are case-insensitive, while model Name comparisons use ordinal (case-sensitive) comparison.
- Behavior after set: SetActive persists the preference then returns the same response as List, so clients receive the updated available list and selected model in one call.
- Routing & auth: Controller is under the "models" route and requires authentication (Authorize attribute).