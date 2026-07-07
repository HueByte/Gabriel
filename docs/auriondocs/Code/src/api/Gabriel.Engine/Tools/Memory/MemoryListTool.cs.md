# MemoryListTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryListTool : ITool
```


MemoryListTool enumerates every memory visible to the current conversation by querying the memory service for the relevant scope (user and project memories). It returns a compact, human-friendly list containing the memory type, scope (user or project), name, and a one-line description. This tool is useful when assessing whether prior memories might inform the current response; it allows you to quickly gauge relevance before deciding to fetch or reference specific memories. It does not load full memory content; it only lists metadata to aid decision-making.

## Remarks
It acts as a lightweight discovery aid that decouples memory storage from the response generation, giving the agent a concise view of what memories exist and are accessible in the present conversation. By honoring the current project scope, it ensures that both user-owned and project-scoped memories are surfaced consistently, enabling better context-aware decisions.

## Example
```csharp
Memories visible to this conversation (2):
- [text, user] Meeting notes — Summary of yesterday's standup
- [document, project] ProjectPlan — Latest project milestones
```

## Notes
- The list is metadata-only: to fetch full bodies, use a separate memory retrieval tool.
- If memory retrieval fails, the tool returns a plain error string rather than throwing.
- The exact ordering of entries is not guaranteed.