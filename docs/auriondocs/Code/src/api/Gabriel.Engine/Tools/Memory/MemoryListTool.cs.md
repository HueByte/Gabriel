# MemoryListTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryListTool : ITool
```


MemoryListTool enumerates every memory visible to this conversation by querying the memory service with the current project context and returns a concise, line-per-entry list that includes the memory type, scope (user or project), name, and a one-line description. Use it to quickly assess which prior memories might be relevant before composing a reply, without loading full memory bodies into the system prompt.

## Remarks
MemoryListTool derives the scope by inspecting whether a memory's ProjectId is null: null means user-scope, non-null means project-scope. This explicit formatting helps the agent decide relevance at a glance, without fetching full bodies. The tool is designed for on-demand discovery and is dependency-injected with IMemoryService and IToolExecutionContext to support testability and separation of concerns. Output is human-friendly rather than a raw payload, so it can be consumed directly by the agent's reasoning/chat UI.

## Notes
- Long memory lists can produce verbose output; there is no pagination.
- The exact error string includes the exception message; in typical UI, consider surfacing a user-friendly fallback or logging.