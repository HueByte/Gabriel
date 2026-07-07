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


MemoryEntryType is an enumeration that categorizes a memory entry into one of four domains: User, Feedback, Project, or Reference. It is used when creating memory entries to tag their type and drive category-specific processing and storage.

## Remarks
MemoryEntryType provides a fixed taxonomy for memory items, allowing the system to categorize content as User, Feedback, Project, or Reference. This taxonomy enables consistent routing, filtering, and UI representation across the memory subsystem, decoupling the payload from its category. The explicit numeric values also support stable serialization and cross-component communication, so avoid reordering or removing existing members.

## Example
```csharp
MemoryEntryType type = MemoryEntryType.User;

switch (type)
{
    case MemoryEntryType.User:
        // user-related memory
        break;
    case MemoryEntryType.Feedback:
        // feedback-related memory
        break;
    case MemoryEntryType.Project:
        // project-related memory
        break;
    case MemoryEntryType.Reference:
        // reference-related memory
        break;
}
```

## Notes
- Changing the underlying numeric values will break serialized data; treat values as stable identifiers.
- When adding new categories, update downstream handlers and persistence mappings to recognize the new type.