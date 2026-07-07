# Project

> **File:** `src/api/Gabriel.Core/Entities/Project.cs`  
> **Kind:** class

```csharp
public class Project
```


A user-owned container that groups conversations, files and an optional per-project personality override (SystemPrompt). Use Project when you need a stable, user-scoped workspace that gives a set of conversations a shared identity — including a consistent avatar sequence and optional skin overrides — rather than working with standalone, per-conversation defaults.

## Remarks
Project centralizes metadata and presentation choices for a user's workspace. The AvatarSeed provides a stable, project-wide identity so conversations rendered under the same project can share the same Gabriel Sequence; the IsDefault flag signals the client to treat the bucket as a "standalone" collection (the client ignores the seed and renders per-conversation sequences, and suppresses project-level UI). PatternOverride and PaletteOverride are catalog identifiers used to pin a specific skin; the entity stores them as-is and does not validate against the catalog (catalog lookup/validation lives outside Core). Files are held in a private list and exposed as a read-only view to prevent external mutation of the internal collection.

## Example
```csharp
// Create a new project for a user
var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var project = Project.Create(ownerId, "My Research", description: "Notes and files for Q3", systemPrompt: "You are a helpful research assistant.");

// Pin a specific skin (catalog identifiers must be validated by caller)
project.SetSkin(pattern: "plasma", palette: "heat");

// Reroll the project-wide avatar seed
project.RerollAvatar();

// Read project data
Console.WriteLine(project.Name);
Console.WriteLine(project.SystemPrompt);
foreach (var file in project.Files)
{
    Console.WriteLine(file.Name);
}
```

## Notes
- Project.Create validates ownerUserId (must not be Guid.Empty) and requires a non-empty name; it throws ArgumentException for invalid inputs.
- SetSkin treats null or whitespace/empty strings as "clear" (sets the override to null). Callers should pass already-validated, lowercase catalog identifiers when pinning skins.
- AvatarSeed is generated in the range 1..2^32-1 so it safely round-trips through JSON numbers; it is generated with Random.Shared (non-cryptographic, non-deterministic).
- The class does not implement synchronization; concurrent mutations (e.g., calling SetSkin/RerollAvatar from multiple threads) are not protected and callers should synchronize if needed.
