namespace Gabriel.Core.Personality;

// Per-conversation behavioral state. Maintained by IConversationStateUpdater
// and read by ISystemPromptBuilder + IResponsePostProcessor. Persisted as JSON
// on Conversation.StateJson so EF doesn't need a separate table.
//
// This is the foundation for the future emotion engine (Phase 10) — `Mood` and
// the user-style flags (emoji / lowercase) will feed avatar reactions later.
public sealed record ConversationState
{
    public int TurnCount { get; init; }
    public Mood Mood { get; init; } = Mood.Neutral;

    // Exponential moving average across user-message token counts. Smooth alternative
    // to keeping the last N counts in a buffer.
    public float AvgUserTokenCount { get; init; }
    public int LastUserTokenCount { get; init; }

    public IReadOnlyList<string> RecentTopics { get; init; } = Array.Empty<string>();
    public DateTimeOffset LastMessageAt { get; init; }
    public int ConsecutiveShortMessages { get; init; }

    // Sticky once true — if the user has ever used emoji, the persona is allowed
    // to mirror sparingly. Reset is intentional manual policy (or fresh chat).
    public bool UserUsesEmoji { get; init; }
    public bool UserUsesLowercase { get; init; }

    // True when the latest user message looks like a request for elaboration
    // ("explain", "tell me about", "how does", "what is"). Read by the post-processor
    // to raise the response length cap.
    public bool UserAskedForDetail { get; init; }

    public static ConversationState Initial() => new()
    {
        LastMessageAt = DateTimeOffset.UtcNow,
    };
}
