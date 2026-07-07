# GabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceService : IGabrielSequenceService
```


GabrielSequenceService is a concrete implementation of IGabrielSequenceService that builds GabrielSequence objects for a conversation or a project by validating the current user, loading the required aggregate state from repositories, and delegating to the generator with the avatar seed and any overrides.

For conversations it loads the conversation with messages to preserve history, uses its GetState() plus any PatternOverride or PaletteOverride to generate the sequence (and throws NotFoundException if missing); for projects it uses the latest conversation in the project as the live state (or null to fall back to neutral defaults) and feeds that into the generator along with the project's avatar seed and overrides.

## Remarks

GabrielSequenceService centralizes the orchestration between data retrieval and sequence generation, shielding callers from repository details and ensuring consistent semantics across conversations and projects. The implementation is a pragmatic first cut: it favors straightforward state derivation and a deterministic latest-conversation selection for projects, leaving room for caching or more sophisticated approaches in a future iteration.

## Notes

- Requires an authenticated user; otherwise UnauthorizedAccessException is thrown.
- NotFoundException is thrown when the targeted Conversation or Project cannot be found for the current user.
- If a project has no conversations, latest is null and the generator uses neutral defaults.