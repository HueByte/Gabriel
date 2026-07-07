# Project

> **File:** `src/api/Gabriel.Core/Entities/Project.cs`  
> **Kind:** class

```csharp
public class Project
```


A user-owned container that groups conversations, files and an optional assistant personality override (SystemPrompt). Use Project to represent a user's workspace or bucket for related conversations; a "Default" project is created lazily for every user and behaves as a standalone bucket.

## Remarks
Project provides a stable identity for a set of conversations: it carries an AvatarSeed that drives a shared Gabriel Sequence (so conversations in the same non-default project render a consistent avatar). The SystemPrompt property is intended to be prepended to the agent's per-turn history (after the global persona and before the rolling summary). Files are stored in an internal list exposed as a read-only `IReadOnlyList<ProjectFile>`. The class uses a private constructor and the Create factory to enforce basic invariants (non-empty owner and name) and to initialize the avatar seed.

## Example
```csharp
// Create a new project for a user
var project = Project.Create(ownerUserId: userId, name: "Research Notes", description: "Notes for Q3 research", systemPrompt: "You are concise.");

// Pin a skin (catalog identifiers must be validated by the caller)
project.SetSkin(pattern: "plasma", palette: "heat");

// Reroll the project's shared avatar
project.RerollAvatar();

// Clear any pinned skin
project.SetSkin(pattern: null, palette: "");
```

## Notes
- Create validates that ownerUserId is not Guid.Empty and that name is not null/whitespace; name and optional text fields are trimmed.
- SetSkin treats null or whitespace/empty strings as "clear" (null) and does not validate that the provided pattern or palette exist in any catalog — callers must pass validated, lower-case catalog identifiers.
- AvatarSeed is generated in the range 1..(2^32-1) so it round-trips safely through JSON numbers.
- The entity updates UpdatedAt when mutating operations occur (e.g., RerollAvatar, SetSkin, Rename), but the class does not implement explicit synchronization — concurrent callers must synchronise if needed.