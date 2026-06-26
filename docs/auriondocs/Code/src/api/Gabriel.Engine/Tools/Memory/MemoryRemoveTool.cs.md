# MemoryRemoveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`  
> **Kind:** class

Deletes a previously-saved memory entry by its kebab-case name. Use this tool when an agent (or developer) needs to remove a memory that was stored with memory_save; choose scope="user" to remove a global user-scoped memory or scope="project" to remove a memory associated with the current project. The tool returns a human-readable string indicating success, failure, or any validation/error condition.

## Remarks
MemoryRemoveTool is a thin wrapper around IMemoryService for the specific delete-by-name scenario exposed to the agent tooling system. It performs argument JSON deserialization and validation, enforces the required scope semantics (project scope requires the execution context to be attached to a project), and converts the boolean removal result into a user-friendly message. It intentionally returns error/result messages as plain strings (rather than throwing) so the calling agent can surface those messages directly.

## Example
```csharp
// Call ExecuteAsync with JSON matching the ParametersJsonSchema.
var tool = new MemoryRemoveTool(memoriesService, executionContext);
string argsJson = "{ \"scope\": \"user\", \"name\": \"favorite-editor\" }";
string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
// result will be one of:
// "Removed user-scope memory 'favorite-editor'."
// "No user-scope memory found named 'favorite-editor'."
// or an error string for invalid input or missing project context.
```

## Notes
- The "name" must be provided (non-empty); the tool will return "Error: name is required." for blank names.
- Using scope="project" requires an attached project (ItoolExecutionContext.ProjectId). If absent, the tool returns an explanatory error string rather than attempting deletion.
- Scope comparison is case-insensitive (e.g. "Project" or "project" are accepted).
- The tool deserializes arguments using System.Text.Json with Web defaults; malformed JSON results in an error message containing the serializer exception text.
- The method returns readable status messages rather than structured results; callers that need machine-readable responses should parse or wrap these messages appropriately.