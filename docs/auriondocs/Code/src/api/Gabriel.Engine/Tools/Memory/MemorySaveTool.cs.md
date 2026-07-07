# MemorySaveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemorySaveTool : ITool
```


MemorySaveTool is an agent-callable tool that persists durable memories for future conversations, enabling the assistant to recall guidance, feedback, and project context across interactions. It accepts a scope of user or project, a type from {user, feedback, project, reference}, a unique name within that scope, a short description, and a body containing the content to store. The operation is idempotent: saving a memory with the same scope and name updates the existing entry instead of creating a duplicate. When scope is project, the current conversation must be attached to a project in the execution context; otherwise an error is returned. The tool validates inputs, enforces the allowed values for scope and type, and delegates persistence to IMemoryService via MemoryEntrySpec(projectId, type, name, description, body). On success, it returns a human-friendly confirmation like "Saved {scopeLabel} memory [{type}] '{name}'."