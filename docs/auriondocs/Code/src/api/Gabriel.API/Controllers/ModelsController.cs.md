# ModelsController

> **File:** `src/api/Gabriel.API/Controllers/ModelsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("models")]
public class ModelsController : ControllerBase
```


ModelsController exposes the UI-facing API surface for selecting and listing models. It provides two endpoints: GET returns all cataloged models plus the entry that will be used on the next turn (the active model for the user, resolved against defaults and stored preferences), and PUT /models/active persists or clears the user's selection. It consults IModelCatalog and IUserPreferences so the effective selection is resolved on every turn without requiring a page reload.

## Remarks
This class acts as a thin API facade between the UI and the domain services that handle model resolution. It centralizes validation (ensuring only known models are selected and that input is complete) and returns a ready-to-consume set of model descriptors with a clear indication of which model is currently active. By resolving the active model on every request, changes take effect on the next interaction rather than requiring a reload, keeping the UI responsive.

## Notes
- PUT /models/active requires either both Provider and Name to be supplied together or both omitted to clear; otherwise returns 400 with an error describing the misconfiguration.
- If both Provider and Name are provided, the controller validates that the model exists in the catalog; if not, it returns 400 with a not-registered message.
- Clearing the selection uses the empty body values (null/empty) to reset the preference and fall back to the catalog default on the next read.
- The active selection in the returned list is determined by a case-insensitive provider match and an exact name match, ensuring a stable and predictable UI state.

## Dependencies
- ControllerBase
- ModelsResponse
- ModelDto
- SelectedModelDto
- StringComparison
- AvailableModels

## Dependency APIs (verified signatures)
- record `ModelsResponse` (`src/api/Gabriel.API/Contracts/Models/ModelDto.cs`)
- record `ModelDto` (`src/api/Gabriel.API/Contracts/Models/ModelDto.cs`)
- record `SelectedModelDto` (`src/api/Gabriel.API/Contracts/Models/ModelDto.cs`)
- property `AvailableModels` (`src/api/Gabriel.Engine/Providers/IModelCatalog.cs`)

## Symbol To Document
- Name: `ModelsController`
- Kind: class
- File: `src/api/Gabriel.API/Controllers/ModelsController.cs`
- Language: csharp
- ID: 8b938895-5fbc-4f78-9343-d411cc67f8e3