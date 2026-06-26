# ContractMappings

> **File:** `src/api/Gabriel.API/Mapping/ContractMappings.cs`  
> **Kind:** class

```csharp
internal static class ContractMappings
```


Converts domain model objects used by the Gabriel API into their corresponding API contract response DTOs. Use these extension methods when preparing data to send to clients so mapping rules (which include filtering of tool messages, variant selection, and sibling ordering) remain consistent across endpoints.

## Remarks
This static helper centralizes mapping logic for Projects, ProjectFiles, Conversations and related types so all endpoints produce the same JSON shape and apply the same filtering rules. Conversation mapping contains non-trivial logic: it optionally omits messages, computes project-related avatar/seed values when a Project is provided, filters tool messages to only those referenced by active assistant variants, and precomputes per-variant sibling lists ordered by CreatedAt so the client can render variant pickers without additional API calls.

## Example
```csharp
// Include files when serializing a project
var projectResponse = project.ToResponse(includeFiles: true);

// Produce a conversation response including messages and using project metadata
var conversationResponse = conversation.ToResponse(includeMessages: true, project: project);

// Produce a lightweight conversation listing row (no messages)
var listRow = conversation.ToResponse(includeMessages: false);
```

## Notes
- Tool call filtering expects Message.ToolCallsJson to be a JSON array of objects each containing an "id" property; messages for tools are included only if their toolCallId is referenced by an active assistant variant.
- String matching for tool-call ids uses StringComparer.Ordinal (ordinal, case-sensitive).
- Sibling lists are grouped by VariantGroupId and ordered by CreatedAt; indexes are stable only if CreatedAt values are stable and comparable.