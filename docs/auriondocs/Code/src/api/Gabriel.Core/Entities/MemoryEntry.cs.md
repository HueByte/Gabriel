# MemoryEntry

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntry.cs`  
> **Kind:** class

```csharp
public class MemoryEntry
```


Represents a single remembered item (a short named memory) owned by a user and optionally scoped to a project. Use MemoryEntry.Create to construct a valid instance and MemoryEntry.Update to perform idempotent edits; the type, name, description and body are required and normalized (trimmed) by the class.

## Remarks
MemoryEntry is an immutable-from-the-outside data entity with private setters and a private constructor so callers must use the Create factory. It encodes the two-level scope model (global-to-user when ProjectId is null, or project-specific when ProjectId is set) and keeps CreatedAt and UpdatedAt timestamps so the UI or callers can show genesis and modification times. Validation for required fields (non-empty name/description/body and non-empty UserId) is enforced here; uniqueness of the Name within a (UserId, ProjectId) scope and kebab-case slug conventions are documented intentions but are not enforced by this type itself.

## Example
```csharp
// Constructing a new memory entry (choose an appropriate MemoryEntryType in real code)
var userId = Guid.NewGuid();
Guid? projectId = null; // or a specific project GUID
MemoryEntryType type = default; // replace with a real enum value

var entry = MemoryEntry.Create(
    userId: userId,
    projectId: projectId,
    type: type,
    name: "save-password-guideline",
    description: "Prefer passphrases over short passwords",
    body: "Use long, memorable passphrases. **Why:** more entropy. **How to apply:** require at least 4 words.");

// Update an existing entry (idempotent update of the body/description/type)
entry.Update(type, "Prefer passphrases over short passwords (updated)", "Updated guidance body...");
```

## Notes
- Create will throw ArgumentException if userId is Guid.Empty or any of name/description/body are null/whitespace.
- Name/Description/Body are trimmed on assignment; the class does not validate kebab-case formatting or enforce uniqueness — that responsibility belongs to callers or the persistence layer.
- Update preserves CreatedAt and sets UpdatedAt to the current UTC time.