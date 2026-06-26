# IGabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`  
> **Kind:** interface

Deterministically produces a complete 64-frame GabrielSequence from a stable numeric seed and optional runtime state. Use this interface when you need the canonical avatar sequence for a personality (e.g. Project.AvatarSeed or Conversation.AvatarSeed) or when you want the same seed to generate identical base frames while optionally applying live conversation signals for the final frames.

## Remarks
This generator cleanly separates stable identity from ephemeral state: the first three conceptual layers of the generated sequence are derived solely from the seed (so they remain consistent across sessions), while the Live State layer (frames 48..63) is influenced by the optional ConversationState to reflect current mood, tempo, or engagement. Pattern and palette overrides, when provided, take precedence over seed-derived selections but unrecognized identifiers are silently ignored and fall back to seed behavior (see SequenceCatalog for identifier resolution). The interface is intentionally stateless and deterministic, making implementations safe to register as singletons.

## Example
```csharp
// Generate sequence for a project-shared avatar seed, no live state
var sequence = generator.Generate(Project.AvatarSeed, state: null);

// Generate sequence for a conversation with live signals and a pinned skin
var sequenceWithOverrides = generator.Generate(
    seed: Conversation.AvatarSeed,
    state: currentConversationState,
    patternOverride: "striped",
    paletteOverride: "warm-sunset");

// Unknown pattern/palette identifiers will not throw; they fall back to seed-derived choices
var sequenceFallback = generator.Generate(seed, state: null, patternOverride: "unknown-pattern");
```

## Notes
- The seed must be stable to guarantee deterministic outputs across calls; changing the seed changes the entire seed-derived portion of the sequence.
- ConversationState affects only frames 48..63 (the Live State layer); other frames remain seed-determined.
- Pattern and palette overrides have higher precedence than seed choices; invalid identifiers are ignored rather than causing errors.
- Implementations should be thread-safe and avoid internal mutable state since the interface is intended for singleton registration.