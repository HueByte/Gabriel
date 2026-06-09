# MemoryRemoveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`  
> **Kind:** class

Deletes a previously saved memory entry identified by a scope and a kebab-case name. Reach for this tool when the agent or user asks the system to forget something (the inverse of memory_save) or to remove a stale/incorrect saved entry; it performs lookup by (scope, name) and returns a human-readable success/failure message.

## Remarks
This tool parses a small JSON arguments object (scope and name), determines whether the removal should act on user-scoped or project-scoped memories, and delegates the actual deletion to an IMemoryService implementation via RemoveByNameAsync. For project-scoped removals it requires the current conversation to be attached to a project (taken from IToolExecutionContext.ProjectId); if no project is present it returns an error instead of attempting removal. The tool returns plain text status messages rather than structured results.

## Example
```csharp
// Remove a user-scoped memory named "favorite-editor"
var argsJson = "{ \"scope\": \"user\", \"name\": \"favorite-editor\" }";
var result = await memoryRemoveTool.ExecuteAsync(argsJson, CancellationToken.None);
// result -> "Removed user-scope memory 'favorite-editor'." or
//           "No user-scope memory found named 'favorite-editor'."

// Remove a project-scoped memory (requires context.ProjectId to be set)
var projectArgs = "{ \"scope\": \"project\", \"name\": \"build-config\" }";
var projectResult = await memoryRemoveTool.ExecuteAsync(projectArgs, CancellationToken.None);
```

## Notes
- The JSON schema's description asks for a kebab-case slug, but the tool only enforces that the name is non-empty; it does not validate kebab-case format.
- Scope comparison is case-insensitive ("project" vs "Project"), but specifying project scope without an attached ProjectId yields an error.
- The tool returns human-readable strings (including error messages on invalid JSON), not a structured success/failure object.