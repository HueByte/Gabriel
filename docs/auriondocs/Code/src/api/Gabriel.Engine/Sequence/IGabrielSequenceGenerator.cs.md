# IGabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceGenerator
```


IGabrielSequenceGenerator is a stateless, deterministic factory for producing a 64-frame Gabriel Sequence from a seed. The seed encodes the personality's stable identity (Project.AvatarSeed for project-shared sequences; Conversation.AvatarSeed for standalone chats). The Generate method accepts a long seed, an optional ConversationState to drive the Live State layer (frames 48–63), and optional string overrides for patternOverride and paletteOverride that take precedence over the seed-derived selection; if an override is unrecognized, the implementation gracefully reverts to seed-based behavior. Because the interface is stateless, implementations are safe to register as a singleton and reuse across callers to preserve determinism and minimize allocations.