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


ConversationState is a per-conversation, immutable record that captures the evolving behavioral state used by the system to tailor prompts and post-processing. It is persisted as JSON on Conversation.StateJson to avoid a dedicated EF table and to provide a stable, transportable snapshot of context across the life of a chat. The state is maintained by IConversationStateUpdater and read by ISystemPromptBuilder and IResponsePostProcessor, and it underpins the future mood engine described in the comments (Mood and user-style flags feeding avatar reactions).

It tracks TurnCount, Mood (defaulting to Neutral), token-usage metrics (AvgUserTokenCount, LastUserTokenCount), recent topics, message timing (LastMessageAt), and user flavor flags (UserUsesEmoji, UserUsesLowercase) plus a hint whether the user asked for detail (UserAskedForDetail).

Initial() creates a starting state with LastMessageAt set to DateTimeOffset.UtcNow.