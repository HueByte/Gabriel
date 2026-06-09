# ContractMappings

> **File:** `src/api/Gabriel.API/Mapping/ContractMappings.cs`  
> **Kind:** class

Converts domain model objects (Project, ProjectFile, Conversation, ContextMetrics, etc.) into API contract/response DTOs used by the HTTP API. These are extension methods the application code should call when preparing data for responses so that message filtering, tool-call inclusion, sibling ordering, and optional metadata (like project-derived avatar seed and files) are applied consistently in one place.

## Remarks
This class centralizes the mapping rules and a few non-trivial filters that ensure the UI sees the same conversation history the agent uses. Notable behaviors include: optional inclusion of nested collections (files, messages), filtering tool messages to only those tool_call entries referenced by an active assistant variant, and precomputing ordered sibling lists per variant group so the variant picker can render stable indices without additional API calls. The optional project parameter on conversation mapping allows callers to include project-level metadata (used to compute the effective avatar seed and whether the project is default) when that context is available.

## Example
```csharp
// Map a project including its files for a list endpoint
var projectResponse = project.ToResponse(includeFiles: true);

// Map a conversation including messages and provide project context so the client
// can render the correct avatar sequence
var conversationResponse = conversation.ToResponse(includeMessages: true, project: project);

// Map a single file
var fileResponse = projectFile.ToResponse();
```

## Notes
- If includeMessages is false the ConversationResponse will have messages set to null (not an empty list); callers should handle that distinction.
- Tool-call JSON is parsed (JsonDocument.Parse and ParseToolCalls); malformed JSON in ToolCallsJson can throw during mapping — validate or guard at the source if necessary.
- These methods assume the source collections (Messages, Files) are stable for the duration of the mapping; if callers mutate those collections concurrently, the behavior is unspecified.