# MemoryEntryType

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntryType.cs`  
> **Kind:** enum

Represents the category assigned to a stored memory entry. Use this enum to classify memories so agent logic, retrieval, display and retention policies can treat user facts, corrective feedback, project context, and external references differently.

## Remarks
This enum mirrors Claude Code's auto-memory schema to keep the agent's mental model aligned with common conventions. The category is intended to drive different handling paths — for example, applying Feedback to change behaviour, using User entries to personalize tone, surfacing Project entries in context-aware responses, and showing Reference entries as external pointers.

## Example
```csharp
// Filter behaviour based on memory category
MemoryEntryType type = MemoryEntryType.Feedback;

switch (type)
{
    case MemoryEntryType.User:
        // tailor response tone or personalization
        break;
    case MemoryEntryType.Feedback:
        // incorporate correction into future decision-making
        break;
    case MemoryEntryType.Project:
        // surface deadlines or constraints when relevant
        break;
    case MemoryEntryType.Reference:
        // display links or pointers to external resources
        break;
}
```

## Notes
- The enum values are explicit integers and stable; do not reorder or renumber members once persisted, as stored data or serialized representations depend on these numeric values.
- The default (zero) value is User — a newly default-initialized MemoryEntryType will be MemoryEntryType.User.
- When integrating with external memory schemas, ensure mapping remains consistent to avoid semantic mismatches.