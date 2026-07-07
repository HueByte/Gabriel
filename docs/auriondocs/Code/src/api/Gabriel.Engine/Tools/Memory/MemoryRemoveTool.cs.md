# MemoryRemoveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryRemoveTool : ITool
```


Deletes a previously saved memory entry by its kebab-case name, scoped to either user-wide memories or the current project. It is the inverse of memory_save: use it when a user asks to forget something or when a saved memory turns out to be stale or incorrect. The lookup is by a (scope, name) pair, where the name is the kebab-case slug reported by memory_list results.

## Remarks
MemoryRemoveTool encapsulates the deletion operation behind the ITool contract, relying on IMemoryService to perform the removal and on IToolExecutionContext to determine the applicable project scope. This abstraction centralizes memory lifecycle management behind a single operation that can be invoked from user-facing commands. The human-friendly returned string provides a clear signal to the user about whether the entry was removed or not found.

## Notes
- Returns a user-friendly message indicating removal or absence of a matching memory.
- If scope = "project" but there is no attached project, an error is produced.
- The name must be non-empty; otherwise, an error is returned.