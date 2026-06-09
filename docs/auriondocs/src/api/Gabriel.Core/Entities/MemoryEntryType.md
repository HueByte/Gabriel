# MemoryEntryType

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntryType.cs`  
> **Kind:** enum

Classifies the kind of information stored in a memory entry so the system can store, filter and act on memories according to their role (user preferences, feedback, project context, or external references). Reach for this enum whenever you create, persist, query or branch logic based on the semantic category of a memory entry.

## Remarks
This enum mirrors the categories used by Claude Code's auto-memory schema so the agent's internal model of "what kind of thing am I writing down?" matches common user-facing conventions. Categories are intentionally coarse-grained: User adjusts tone and personalization, Feedback influences future behaviour, Project groups work-specific context, and Reference points to external resources. The numeric values are explicit to remain stable when stored or serialized.

## Example
```csharp
// Create a new memory entry and tag it as a project-related memory
var memory = new MemoryEntry
{
    Id = Guid.NewGuid(),
    Type = MemoryEntryType.Project,
    Content = "Release scheduled for 2026-06-15; blockers: database migration",
    CreatedAt = DateTime.UtcNow
};

// Filter memories when loading context
var projectMemories = allMemories.Where(m => m.Type == MemoryEntryType.Project);

// Branch behavior based on type
switch (memory.Type)
{
    case MemoryEntryType.User:
        ApplyPersonalization(memory);
        break;
    case MemoryEntryType.Feedback:
        UpdateAgentPreferences(memory);
        break;
    case MemoryEntryType.Project:
        IncludeInProjectContext(memory);
        break;
    case MemoryEntryType.Reference:
        AddExternalReference(memory);
        break;
}
```

## Notes
- The explicit integer values are part of the persisted representation; do not reorder or renumber existing members if entries are stored or serialized, and append new members instead. 
- This is not a bit-flag enum; treat members as mutually exclusive categories.