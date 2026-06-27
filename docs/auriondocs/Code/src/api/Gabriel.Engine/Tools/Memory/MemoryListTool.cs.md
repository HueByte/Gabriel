# MemoryListTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`  
> **Kind:** class

Lists the memories visible to the current conversation by querying the injected IMemoryService and formatting each entry as a one-line summary (type, scope, name, description). Use this tool when an agent needs a lightweight inventory of available memories to decide which, if any, memory bodies to read before forming a response.

## Remarks
This tool exists to keep the system prompt compact while still letting an agent inspect what memories exist for the current conversation. It queries memories scoped to the current project (using IToolExecutionContext.ProjectId) and user-scope memories, then returns a plain-text, human-readable list instead of fetching full memory bodies.

## Example
```csharp
// Typical use from an agent: parameters are ignored (empty schema) so pass an empty object
var result = await memoryListTool.ExecuteAsync("{}", cancellationToken);
Console.WriteLine(result);

// Sample output (shown here as comments):
// Memories visible to this conversation (2):
// - [note, user] Favorite drink — Likes coffee
// - [fact, project] Deployment schedule — Deploy every Friday
```

## Notes
- The method returns plain text formatted for human/agent reading, not structured JSON; callers that need machine-readable output must parse it themselves or call the memory service directly.
- Exceptions from the memory service are caught and returned as an error string prefixed with "Error: ..." rather than being thrown.
- Scope is determined by whether MemoryEntry.ProjectId is null ("user") or non-null ("project").
- If a memory's Description contains newlines the output's one-line-per-entry assumption may be disrupted.