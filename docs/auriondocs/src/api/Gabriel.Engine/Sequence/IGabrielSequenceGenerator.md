# IGabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`  
> **Kind:** interface

Generates a deterministic, full 64-frame GabrielSequence from a stable numeric seed and optional runtime ConversationState. Use this when you need a reproducible avatar animation/appearance for a project or conversation (Project.AvatarSeed or Conversation.AvatarSeed) and want the later frames to reflect live mood/tempo/engagement signals while keeping the base identity derived from the seed.

## Remarks
This abstraction centralizes the logic that maps a personality seed plus optional live signals and optional pattern/palette overrides into the complete sequence used by the renderer. The first layers of the generated sequence are derived purely from the seed; the Live State layer (frames 48..63) incorporates signals from ConversationState. Pattern and palette overrides, when provided, take precedence over seed-derived choices; unrecognized override identifiers silently fall back to seed-derived behavior (see SequenceCatalog for identifier resolution). The implementation is stateless and safe to register as a singleton.

## Example
```csharp
// Resolve via DI or construct an implementation
IGabrielSequenceGenerator generator = /* get from container */;

long seed = project.AvatarSeed; // or conversation.AvatarSeed
ConversationState? liveState = conversation?.State; // may be null for static sequence

// Generate a sequence, pinning the visual skin while allowing live frames to respond
var sequence = generator.Generate(seed, liveState, patternOverride: "stripe", paletteOverride: null);

// Null state produces the pure-seed variant; providing a ConversationState updates frames 48..63
```

## Notes
- The result is deterministic: identical seed + state + overrides produce the same GabrielSequence.
- ConversationState influences only the Live State layer (frames 48..63); earlier frames remain seed-derived.
- Pattern/palette overrides override seed choices; unknown identifiers do not throw and fall back to seed-derived selection.