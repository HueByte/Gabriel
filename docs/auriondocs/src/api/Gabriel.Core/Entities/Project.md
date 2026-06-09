# Project

> **File:** `src/api/Gabriel.Core/Entities/Project.cs`  
> **Kind:** class

Represents a user-owned container that groups conversations, project-scoped files, and an optional system prompt (a personality override). Reach for Project when you need a stable identity and shared configuration for a collection of conversations — for example a persistent workspace that supplies a project-level system prompt, a shared avatar seed, and optional skin overrides.

## Remarks
Project provides the per-user project abstraction used by the client to decide rendering and affordances. Each project carries an AvatarSeed (stable per-project identity) and optional PatternOverride/PaletteOverride to pin visual skins; the special lazy-created "Default" project (IsDefault) still has a seed but the client treats it as per-conversation/standalone. SystemPrompt, if set, is intended to be prepended to agent history at runtime (after global persona and before any rolling summary).

## Example
```csharp
// Create a new project for a user
var project = Project.Create(ownerUserId: userId, name: "Research Workspace", description: "Notes and experiments", systemPrompt: "Be concise.");

// Pin a skin (catalog identifiers should be validated by the caller)
project.SetSkin(pattern: "plasma", palette: "heat");

// Reroll the shared avatar seed
project.RerollAvatar();

// Inspect files (read-only view)
var files = project.Files;
```

## Notes
- Project.Create throws ArgumentException when ownerUserId is Guid.Empty or when name is null/whitespace.
- PatternOverride and PaletteOverride are stored as plain identifiers; this entity does not validate them against any catalog — callers should pass already-validated, lower-case catalog names or null to clear.
- SetSkin treats empty or whitespace strings as null (clearing the override) and updates UpdatedAt.
- AvatarSeed is generated in the range 1..(2^32-1) so it round-trips safely through JSON number encodings.
- Files exposes a read-only view of the entity-owned list; callers cannot mutate the list directly but the entity may expose methods to add/remove files.
- The entity is mutable and does not employ internal synchronization; concurrent access should be coordinated by callers if needed.
