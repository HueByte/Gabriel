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


ConversationState is an immutable, per-conversation snapshot of behavioral state used by the chat engine to influence how prompts are built and how responses are post-processed. It is maintained by IConversationStateUpdater and read by ISystemPromptBuilder and IResponsePostProcessor, and persisted as JSON on Conversation.StateJson to avoid a separate EF table. This record underpins the planned emotion engine (Phase 10) by exposing Mood and user-style flags that will feed avatar reactions as the system evolves.

## Remarks
ConversationState serves as a centralized, serializable context that links turn-by-turn interaction concerns (token usage, topics, and user signals) with the system's prompting and post-processing decisions. Its immutable design and a dedicated updater keep behavioral changes explicit and thread-safe, while JSON persistence keeps the storage surface minimal and forward-compatible with evolving persona controls (Mood and flags).

## Example
```csharp
var s = ConversationState.Initial();
var next = s with
{
    TurnCount = s.TurnCount + 1,
    UserAskedForDetail = true,
    RecentTopics = new[] { "greetings", "getting-started" }
};
```

## Notes
- The type is immutable; updates must produce a new instance (e.g., via the with-expression) or be applied through a dedicated updater.
- StateJson persistence means changes to the conversation state need to be serialized back into Conversation.StateJson to remain in sync with the rest of the system.
- Default values (Mood.Neutral, Empty RecentTopics) apply when a new state is created; deserialization may restore previously saved values, so be mindful of potential nulls or missing fields if JSON evolves.