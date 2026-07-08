# Project

> **File:** `src/api/Gabriel.Core/Entities/Project.cs`  
> **Kind:** class

```csharp
public class Project
```


Represents a user-owned container that groups conversations, files, and an optional per-project system prompt (a persona override). Reach for Project when you need a stable project scope — for grouping conversations, sharing files and a shared avatar/visual identity — or to create the special lazily-created Default bucket for a user's standalone conversations.

## Remarks
Project encapsulates the project-level identity used by clients and the system: it holds the stable AvatarSeed that drives the project-wide Gabriel Sequence, optional Pattern/Palette overrides that pin a visual skin, and a SystemPrompt that is prepended to an agent's per-turn history. The Default project is a special, lazily-created bucket (IsDefault) that still carries a seed but is treated by clients as per-conversation/standalone. Project owns a collection of ProjectFile instances (exposed as a read-only list) and is referenced by Conversation.ProjectId when conversations belong to a project.

## Example
```csharp
// Create a new project for a user
var project = Project.Create(ownerUserId: userId, name: "Research Notes", description: "Notes and files for Q3" );

// Pin a specific skin (catalog identifiers should already be validated by the caller)
project.SetSkin(pattern: "plasma", palette: "heat");

// If you want a new random avatar seed for the project
project.RerollAvatar();

// Read files (immutable view)
var files = project.Files;
```

## Notes
- SetSkin does not validate pattern/palette against any catalog; callers must pass known, lower-case identifiers or null. Empty or whitespace strings are treated as null (clears the override).
- The Default project (IsDefault == true) still has an AvatarSeed but clients typically ignore it and render per-conversation sequences instead.
- AvatarSeed values are generated in the range 1..2^32-1 so they round-trip safely through JSON Number types.
- Files is exposed as an IReadOnlyList backed by a private list; modify via the entity's methods (not directly) if such methods exist elsewhere.
- Project.Create validates that ownerUserId is not Guid.Empty and that name is non-empty; it throws ArgumentException for invalid inputs.