# ConversationState

> **File:** `src/api/Gabriel.Core/Personality/ConversationState.cs`  
> **Kind:** record

Represents lightweight, per-conversation behavioral and usage metadata that drives system prompts and response post-processing. Use this record when reading or persisting a conversation's ephemeral signals (mood, token statistics, recent topics, usage/style flags) instead of creating separate database tables or scattering these fields across services.

## Remarks
This record is the single-source state for conversation-level features: it is maintained by an updater component and consumed by the prompt builder and response post-processor. It is intentionally stored as JSON on Conversation.StateJson (so Entity Framework does not require a separate table). The shape is designed to be small and immutable so components can take a snapshot and produce deterministic behavior; it also forms the foundation for future emotion/avatar behavior (e.g., Mood, emoji/lowercase mirroring).

## Example
```csharp
// Create initial state and produce an updated snapshot using a record 'with' expression.
var state = ConversationState.Initial();
var updated = state with
{
    TurnCount = state.TurnCount + 1,
    LastMessageAt = DateTimeOffset.UtcNow,
    LastUserTokenCount = 58,
    // AvgUserTokenCount should be updated by the IConversationStateUpdater (e.g. EMA calculation)
    AvgUserTokenCount = state.AvgUserTokenCount // replace with new EMA value
};

// Persist updated by serializing back to Conversation.StateJson (handled by the updater).
```

## Notes
- The record is immutable: update via 'with' expressions and persist the new instance; sharing an instance across threads is safe because of immutability.
- AvgUserTokenCount is an exponential moving average of user token counts (not a simple arithmetic mean); treat it accordingly when updating.
- UserUsesEmoji and UserUsesLowercase are "sticky" flags — once true they remain true until explicitly reset or the conversation is replaced (this is a deliberate policy choice for mirroring behavior).