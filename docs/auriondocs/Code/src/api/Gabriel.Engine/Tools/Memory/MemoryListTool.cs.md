# MemoryListTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`  
> **Kind:** class

```csharp
public sealed class MemoryListTool : ITool
```


MemoryListTool is a read-only utility that reveals the memories visible to the current conversation. It queries the memory store for memories associated with the active project (via the execution context) and returns a concise, human-friendly list that includes each memory's type, scope (user or project), name, and a one-line description. Use it to quickly determine whether prior memories are relevant before composing a response, without loading full memory payloads or bloating prompts.

## Remarks
By separating memory discovery from response generation, MemoryListTool provides a single, consistent surface for understanding what the agent can see. It encapsulates how scope is determined (user vs project) and how entries are formatted, reducing coupling between callers and the memory storage implementation. This helps keep prompts lean while still giving enough context to decide relevance.

## Example
```csharp
// Most common usage: discover what memories are visible to the current conversation
string result = await memoryListTool.ExecuteAsync("{}", CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The output is a human-readable summary, not a structured payload.
- If the memory store cannot be queried, the method returns an error string (e.g., "Error: could not list memories — ...") instead of throwing.
- Each entry shows memory type, scope (user vs project), name, and description; the header includes the total count of visible memories.
