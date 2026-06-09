# MemoryListTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`  
> **Kind:** class

Lists the memories that are visible to the current conversation so an agent can decide whether to read any specific memory before responding. Use this tool when you need a brief, human-readable summary of available memories (type, scope, name, one-line description) rather than fetching full memory bodies.

## Remarks
MemoryListTool is a lightweight, read-only helper that queries the configured IMemoryService for entries scoped to the conversation (both user-scope memories and any project-scoped memories for the current project). It returns a plain text summary suitable for inclusion in an agent's prompt or diagnostic output. Errors during listing are captured and returned as an error message string instead of throwing, and the tool exposes an empty JSON schema because it takes no parameters.

## Example
```csharp
// Typical use inside a tool-execution environment
var tool = new MemoryListTool(memoriesService, toolExecutionContext);
string result = await tool.ExecuteAsync("{}", CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The tool returns a human-readable string (not structured JSON); callers that need structured data should call IMemoryService directly.
- ExecuteAsync catches exceptions from the memory service and returns an error string — it does not propagate exceptions.
- The output shows each entry as: [type, scope] name — one-line description; scope is "user" when ProjectId is null and "project" otherwise.