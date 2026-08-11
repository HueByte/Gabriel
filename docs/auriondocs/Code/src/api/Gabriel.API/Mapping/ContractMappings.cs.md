# ContractMappings

> **File:** `src/api/Gabriel.API/Mapping/ContractMappings.cs`  
> **Kind:** class

```csharp
internal static class ContractMappings
```


ContractMappings is a small, focused set of extension methods that translate the domain models Project, ProjectFile, and Conversation into their API-facing DTOs (ProjectResponse, ProjectFileResponse, ConversationResponse). It centralizes the transformation logic so API endpoints can return stable, well-formed payloads without duplicating mapping code. The methods handle optional data (e.g., including project files, or including conversation messages), derive derived fields such as the mode name and avatar seed, and apply UI-oriented filtering and grouping (like active tool messages and per-variant siblings) to ensure the client sees a coherent view of each entity's state.

## Remarks
ContractMappings isolates presentation concerns from business logic, providing a single, testable surface for converting domain models into API responses. It precomputes data the client relies on (variant-sibling lists, modeName, effective avatar seeds) and applies consistent inclusion rules (files, messages, and tool-call data) across endpoints. The implementation also mirrors the model's semantics for tool interactions, including only active variants and tool messages linked to active tool calls, so the UI reflects the model's current capability set.

## Notes
- ToolCallsJson must be valid JSON; the code parses it to extract tool-call IDs, and malformed JSON can throw at runtime.
- When includeFiles is false, the response's Files property is null; callers should handle absence of file data.
- modeName and effectiveSeed may be null if mode or project are not provided; UI should handle null display.