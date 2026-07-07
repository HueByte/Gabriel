# ContractMappings

> **File:** `src/api/Gabriel.API/Mapping/ContractMappings.cs`  
> **Kind:** class

```csharp
internal static class ContractMappings
```


ContractMappings provides a concise, centralized set of extension methods that translate domain models into API response DTOs. It handles Projects, ProjectFiles, Conversations, and ContextMetrics by applying a consistent shape, loading related data only when requested, and computing derived fields that the client relies on (for example, the conversation mode name, whether a project is default, and the effective avatar seed).

## Remarks

By encapsulating this mapping in one place, the domain models remain decoupled from API payloads, and the API surface remains stable even as the internal domain evolves. The ToResponse methods compute derived values (modeName, projectIsDefault, effectiveSeed) and apply filtering rules to messages to match the UI's expectations (e.g., including only active variants and qualifying tool messages). For conversations, the mapping also precomputes per-variant sibling lists to enable a reliable, index-stable variant picker in the client.

## Example

```csharp
// Minimal example: map a project including its files
var projectDto = project.ToResponse(includeFiles: true);
```

```csharp
// Map a conversation with its messages for a given project
var convDto = conversation.ToResponse(includeMessages: true, project: someProject);
```

## Notes

- ToolMessages: Only tool messages with a non-null ToolCallId that are part of an active tool call are included; ToolCallsJson is parsed to extract IDs. If ToolCallsJson is invalid, this will throw.
- IncludeFiles: When includeFiles is false, the returned ProjectResponse omits the Files collection entirely (null). This reduces payload but requires the caller to re-fetch if needed.
- Performance: For conversations with many messages, the mapping builds per-variant sibling lists and filters the messages; this is intended to keep UI behavior consistent but may be non-trivial for large histories.
- Nulls: Some derived fields (like projectIsDefault, effectiveSeed) are nullable depending on whether a project is supplied.