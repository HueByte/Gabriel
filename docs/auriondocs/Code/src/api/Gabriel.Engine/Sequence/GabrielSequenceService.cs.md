# GabrielSequenceService

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceService.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceService : IGabrielSequenceService
```


GabrielSequenceService is a sealed service that builds GabrielSequence instances for either a Conversation or a Project. It orchestrates repository access, enforces authentication, and delegates to IGabrielSequenceGenerator to render a sequence from the retrieved avatar seed, current state, and any overrides.

## Remarks
By centralizing data retrieval and sequence generation, this service isolates I/O concerns from the generation logic and provides clear failure signals (e.g., UnauthorizedAccessException for unauthenticated users and NotFoundException for missing aggregates) before a sequence is produced. It serves as the boundary between the application layer’s data access and the domain/presentation concerns that consume GabrielSequence objects, ensuring consistent state-driven rendering across both conversations and projects.

## Notes
- The generator receives a possibly null state when there are no relevant conversations (e.g., no conversations in a project), so the generator must handle neutral defaults in that scenario.
- If the current user is not authenticated, GetForConversationAsync and GetForProjectAsync throw UnauthorizedAccessException.
- When a requested Conversation or Project cannot be found, a NotFoundException is thrown to indicate the precise missing aggregate.
