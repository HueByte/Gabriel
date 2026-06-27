# Project

> **File:** `src/api/Gabriel.Core/Entities/Project.cs`  
> **Kind:** class

Represents a user-owned container that groups conversations, files and an optional system-level prompt (a "SystemPrompt"). Reach for this when you need a per-user project boundary that provides a stable avatar identity (AvatarSeed), optional skin overrides, and a place to store project-level metadata such as Name, Description and SystemPrompt. The class encapsulates creation and a few mutation operations (rerolling the avatar, pinning/clearing skins) and exposes the project's files as a read-only list.

## Remarks
Project exists to give conversations a shared identity and configuration: each user-created project has a stable AvatarSeed so all conversations in that project render the same Gabriel Sequence avatar, while the lazily-created "Default" project is treated as a standalone bucket by clients (its seed is present but ignored). SystemPrompt, when set, is prepended into the agent's per-turn history (after the global persona and before any rolling summary). PatternOverride and PaletteOverride are catalog identifiers stored as plain strings here — validation against the sequence catalog is intentionally left to higher layers.

## Example
```csharp
// Create a new project for a user
var project = Project.Create(ownerUserId: someUserId, name: "Research", description: "AI experiments", systemPrompt: "You are an expert tutor.");

// Pin a skin (catalog ids expected) or clear by passing null/empty
project.SetSkin(pattern: "plasma", palette: "heat");
project.SetSkin(pattern: null, palette: null); // clear overrides

// Reroll the project's shared avatar
project.RerollAvatar();

// Read files (collection is read-only from the outside)
var files = project.Files;
```

## Notes
- AvatarSeed is generated in the range 1..2^32-1 so it safely round-trips through JSON numbers.
- SetSkin treats null/empty/whitespace strings as "clear" (null) — callers should pass validated, lower-case catalog identifiers; this entity does not validate them.
- Create enforces that ownerUserId is not Guid.Empty and name is not null/whitespace and will throw ArgumentException otherwise. The Files collection is exposed as IReadOnlyList; mutations must be performed through the entity's internal mechanisms (not via the Files property).