# ContractMappings

> **File:** `src/api/Gabriel.API/Mapping/ContractMappings.cs`  
> **Kind:** class

```csharp
internal static class ContractMappings
```


ContractMappings is an internal helper that translates domain models into API response contracts by offering ToResponse extension methods for Project, ProjectFile, and Conversation. It centralizes the mapping logic and exposes endpoint-oriented shapes, allowing callers to opt-in to related data (such as including files or messages) while consistently computing metadata like avatar seeds and default-project flags.

## Remarks
This abstraction decouples the domain layer from the API contracts, ensuring consistent response shapes across endpoints. It encodes endpoint-specific concerns—e.g., whether to embed a project's files, whether to include a conversation's message history, and how avatar seeds are derived from either the project or the conversation metadata. The approach reduces duplication and keeps the mapping logic in one place, simplifying maintenance and future changes.

## Notes
- When includeFiles is false, the ProjectResponse's Files collection is null rather than an empty list; call sites should account for null when serializing.
- In Conversation.ToResponse, omitting messages via includeMessages = false yields a lightweight payload suitable for list endpoints; additional project-derived metadata remains present to enable correct UI rendering.