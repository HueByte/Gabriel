# MemorySaveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`  
> **Kind:** class

```csharp
public sealed class MemorySaveTool : ITool
```


MemorySaveTool provides a standardized, agent-callable way to persist durable memories about users, feedback, project context, or external references so future conversations can reference them. It is idempotent: saving a memory with the same scope and name updates the existing entry instead of creating a duplicate. Developers use this when they want to capture user-provided rules, preferences, or contextual notes that should survive beyond a single interaction, and to ensure retrieval logic has a consistent, well-scoped source of truth.

## Remarks
MemorySaveTool encapsulates memory persistence behind an ITool, decoupling validation, storage, and retrieval concerns. The explicit scopes ('user' or 'project') and types ('user', 'feedback', 'project', 'reference') enforce correct categorization and help downstream logic reason about contextual relevance. The idempotent save behavior reduces duplication and simplifies updates when the same memory is saved again.

## Notes
- scope='project' requires that the current conversation be attached to a project; otherwise this operation fails.
- name, description, and body must be non-empty to proceed.
- type must be one of user, feedback, project, or reference; invalid values are rejected.
- Saving twice with the same name within the same scope updates the existing entry instead of creating a duplicate.