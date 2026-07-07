# IGabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceGenerator
```


Deterministically generates a full 64-frame Gabriel Sequence from a seed, with optional live-state and style overrides. The seed encodes the personality's stable identity (Project.AvatarSeed for project-shared sequences; Conversation.AvatarSeed for standalone chats). When supplied, ConversationState drives the Live State layer (frames 48..63) and the first three layers remain seed-derived. PatternOverride and PaletteOverride let you pin the avatar's skin; these overrides take precedence over the seed-derived selection. If an override identifier is unrecognized, the behavior gracefully falls back to the seed-derived logic (see SequenceCatalog). The interface is stateless and safe to register as a singleton, enabling simple, repeatable composition and easy testing across callers.

## Remarks
This interface establishes a clean separation between the immutable seed that defines an avatar's core character and the runtime context that can modulate its expression. By abstracting sequence generation behind IGabrielSequenceGenerator, callers can swap implementations, mock behavior in tests, or vary generation strategies without changing dependent code. The stateless contract ensures thread-safe, singleton-friendly usage and predictable outputs, since every call to Generate yields a new GabrielSequence from the provided inputs, with no hidden internal state.

## Notes
- Overrides are resolved via a catalog mechanism; unrecognized pattern/palette identifiers do not throw but instead fall back to seed-derived behavior.
- Implementations should remain stateless to preserve singleton safety; avoid retaining per-call state inside the generator instance.
