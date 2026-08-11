# MemoryRemoveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryRemoveTool : ITool
```


MemoryRemoveTool deletes a saved memory entry by its kebab-case name, scoped to either user-wide memories or the current project. Lookups are performed by a (scope, name) pair, where the kebab-case slug is what the model echoes from memory_list results, and this tool is invoked when a user asks to forget or when a saved memory is stale or incorrect.

## Remarks
MemoryRemoveTool encapsulates the removal as a dedicated action behind the ITool contract, keeping memory management separate from higher-level dialogue logic. It enforces scope rules by consulting the execution context for a project when scope is 'project', returning a precise error if no project is attached. The returned message clearly indicates whether a memory was removed or none was found, informing subsequent user interaction.

## Notes
- Project scope requires a project context; without one, the command returns an error.
- The 'name' argument must be non-empty.
- Scope matching is case-insensitive (e.g., 'project' and 'PROJECT' are treated the same).