# ConversationState

> **File:** `src/api/Gabriel.Core/Personality/ConversationState.cs`  
> **Kind:** record

Represents per-conversation behavioral state used by the dialogue system to adapt prompts and post-process responses. Use this record when reading or persisting conversation-level signals (turn count, mood, token statistics, recent topics and user-style flags); the state is updated by the conversation updater and consumed by the system prompt builder and response post-processor.

## Remarks
This record is the in-memory shape for the conversation's behavioral metadata and is persisted as JSON on Conversation.StateJson so Entity Framework does not require a dedicated table. It is intentionally lightweight and immutable: an IConversationStateUpdater produces new instances (via record "with" expressions) and downstream components (ISystemPromptBuilder, IResponsePostProcessor) read the values to alter prompt construction and response length/format. The type is the foundation for future features (the emotion engine) and carries sticky style flags that survive across messages until intentionally reset.

## Example
```csharp
// Create initial state and update a few fields as responses are generated
var state = ConversationState.Initial();

// after processing a user message
var updated = state with
{
    TurnCount = state.TurnCount + 1,
    LastMessageAt = DateTimeOffset.UtcNow,
    LastUserTokenCount = 42,
    AvgUserTokenCount = 40.5f, // typically computed by the updater using an EMA
    UserUsesEmoji = true,      // once set, remains true until explicitly reset
    UserAskedForDetail = true, // read by post-processor to raise response length cap
};
```

## Notes
- The record is immutable; updates must produce a new instance (use the C# "with" expression).
- Initial() sets LastMessageAt to DateTimeOffset.UtcNow, which can introduce non-determinism in tests if not mocked.
- AvgUserTokenCount is an exponential moving average maintained by the updater — it is a smoothed value, not a history of exact counts.
- UserUsesEmoji and UserUsesLowercase are "sticky" flags: once true they stay true until a policy-driven reset or a fresh conversation.
