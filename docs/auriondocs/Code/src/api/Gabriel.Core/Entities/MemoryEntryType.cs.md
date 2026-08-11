# MemoryEntryType

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntryType.cs`  
> **Kind:** enum

```csharp
public enum MemoryEntryType
{
    User = 0,
    Feedback = 1,
    Project = 2,
    Reference = 3,
}
```


MemoryEntryType is a simple enum used to tag memory entries with their semantic category—User, Feedback, Project, or Reference—so you can classify stored facts about people, guidance or corrections, ongoing work context, or external pointers in a strongly-typed way instead of using ad-hoc strings. Use it when recording a memory entry to drive tailored explanations, filtering, or routing in downstream components.

## Remarks
MemoryEntryType centralizes labeling for the memory system, enabling consistent handling across presentation, analytics, and policy decisions. It keeps memory storage decoupled from how it's consumed; components can switch on type to decide tone, visibility, or retention rules. The explicit numeric values ensure stable persistence and interoperability with any existing data; avoid reordering values without migration.

## Example
```csharp
MemoryEntryType t = MemoryEntryType.Project;

switch (t)
{
    case MemoryEntryType.User:
        // tailor explanations for a user persona
        break;
    case MemoryEntryType.Feedback:
        // fold user feedback into behavior updates
        break;
    case MemoryEntryType.Project:
        // associate with a project or work item
        break;
    case MemoryEntryType.Reference:
        // link to external documentation or dashboards
        break;
}
```

## Notes
- Changing the underlying numeric values would break persisted data; prefer adding new values and migrating data rather than reordering existing ones.
- If introducing a new category, ensure downstream filters, UI labels, and documentation are updated accordingly.
