# ConversationState

> **File:** `src/api/Gabriel.Core/Personality/ConversationState.cs`  
> **Kind:** record

```csharp
public sealed record ConversationState
{
    public int TurnCount { get; init; }
    public Mood Mood { get; init; } = Mood.Neutral;

    public float AvgUserTokenCount { get; init; }
    public int LastUserTokenCount { get; init; }

    public IReadOnlyList<string> RecentTopics { get; init; } = Array.Empty<string>();
    public DateTimeOffset LastMessageAt { get; init; }
    public int ConsecutiveShortMessages { get; init; }

    public bool UserUsesEmoji { get; init; }
    public bool UserUsesLowercase { get; init; }

    public bool UserAskedForDetail { get; init; }

    public static ConversationState Initial() => new()
    {
        LastMessageAt = DateTimeOffset.UtcNow,
    };
}
```


ConversationState is the per-conversation, immutable snapshot of the session's behavioral state. It is maintained by IConversationStateUpdater and read by ISystemPromptBuilder and IResponsePostProcessor to tailor prompts, responses, and behavior to the current dialogue (mood, topics, and user style). The state is persisted as JSON on Conversation.StateJson so Entity Framework does not require a separate table, and it serves as the foundation for the planned emotion engine (Phase 10), where Mood and user-style flags will influence avatar reactions.

## Remarks
This design centralizes transient, session-scoped signals in a single immutable record, decoupling state management from the prompt-building and post-processing logic. It enables future enhancements without changing the public interface or persistence shape, and it provides a stable contract for components that need to read or extend conversation behavior across turns.

## Example
```csharp
var initial = ConversationState.Initial();
var next = initial with {
    TurnCount = initial.TurnCount + 1,
    LastMessageAt = DateTimeOffset.UtcNow
};
```

## Notes
- ConversationState is immutable; updates are performed by creating a new instance (e.g., via the with-expression).
- AvgUserTokenCount is an EMA-based metric; update with a proper smoothing factor to avoid abrupt shifts.
- LastMessageAt is UTC-based and used to reason about recency; ensure consistent timezone handling when persisting.