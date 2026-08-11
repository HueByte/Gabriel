# MemorySaveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemorySaveTool : ITool
```


MemorySaveTool is an agent-callable tool that persists a memory entry for future conversations. It is designed to capture durable information about a user, feedback to guide future behavior, project-specific context, or external references, and to reuse that memory across interactions. Invoke it with a scope of either user or project, a type drawn from {user, feedback, project, reference}, a short kebab-case name, a one-line description, and the full body content. If an entry with the same name already exists in the chosen scope, the tool updates it in place rather than creating a duplicate. The tool delegates persistence to IMemoryService via SaveAsync using a MemoryEntrySpec constructed from the provided arguments, and it validates the scope against the current project context and the allowed set of types. On success, it returns a confirmation message indicating the saved scope, memory type, and name. This abstraction enables durable, retrievable memories that improve consistency and personalization across conversations.

## Remarks
MemorySaveTool encapsulates the persistence concern for conversational memories. By separating storage from the agent logic, it provides a uniform mechanism to remember user preferences, feedback, project context, or external references, while enforcing scope and type constraints. It fits alongside memory retrieval capabilities and context management, serving as the controlled entry point for adding durable knowledge that should outlive a single turn. The idempotent design (updating an existing entry with the same name) helps prevent accidental duplication when the same memory is saved multiple times.

## Notes
- scope='project' requires the current conversation to be attached to a project; otherwise an error is returned. 
- name, description, and body are required and must be non-empty. 
- Saving with the same name updates the existing memory rather than creating a duplicate. 
- The resulting confirmation message communicates the scope (user or project), the memory type, and the memory name.