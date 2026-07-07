# MemoryRemoveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryRemoveTool : ITool
```


MemoryRemoveTool deletes a saved memory entry by its kebab-case name. When a user asks to forget something or when a saved memory is found to be stale, use this tool to remove it. It supports two scopes: user (memories that apply across every project) and project (memories saved for the current project). For project-scoped deletions, the tool reads the current conversation's project context and then delegates the removal to the memory store. The call returns a friendly message indicating whether a memory was removed or none was found.

## Remarks
MemoryRemoveTool encapsulates the deletion of memories behind a simple, scope-aware interface. By decoupling the memory store (IMemoryService) from how the command is invoked (ITool), it enables forgetting to be driven by higher-level user intents while preserving project scoping rules via the execution context. This separation also ensures error conditions (such as attempting project-scoped removal when no project is attached) are surfaced as clear, actionable messages rather than exceptions.

## Example
```csharp
// Remove a user-scoped memory named "my-memory"
var json = "{\"scope\":\"user\",\"name\":\"my-memory\"}";
var result = await memoryRemoveTool.ExecuteAsync(json, CancellationToken.None);

// Remove a project-scoped memory named "weekly-standup" (requires an attached project in context)
var json2 = "{\"scope\":\"project\",\"name\":\"weekly-standup\"}";
var result2 = await memoryRemoveTool.ExecuteAsync(json2, cancellationToken);
```

## Notes
- The command takes two JSON properties: scope ("user" or "project") and name (the kebab-case slug of the entry to remove).
- If scope is "project" but the current conversation isn’t attached to a project, the tool returns an error message instead of performing deletion.
- A non-empty name is required; empty or whitespace-only values yield an error response.