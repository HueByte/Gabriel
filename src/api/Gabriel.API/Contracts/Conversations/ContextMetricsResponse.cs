namespace Gabriel.API.Contracts.Conversations;

// Mirrors ContextMetrics (Gabriel.Engine) for over-the-wire transport. Kept as
// a separate record so the API surface doesn't leak engine-layer types and
// stays stable across internal refactors.
public record ContextMetricsResponse(
    // Sum of every category below - matches the threshold MaybeCompactAsync
    // compares against, and what the provider will actually receive.
    int CurrentTokens,
    // Provider's total context window (e.g. 256_000 for Grok 4).
    int ContextWindowTokens,
    // Token count at which the next user turn would trigger MaybeCompactAsync.
    int CompactThresholdTokens,
    // Fraction-of-window form of the threshold (e.g. 0.8 = 80%). Handy for
    // showing a percentage tick on a progress bar without re-deriving it.
    double CompactThresholdRatio,
    // Messages currently above the cut - i.e. verbatim history still being sent.
    int MessagesAfterCut,
    // True once at least one compact has rolled. Lets the UI show a small
    // "summarized" hint.
    bool IsSummarized,
    // --- Per-category breakdown ---------------------------------------------
    // Each field is the estimated token cost of one piece of what
    // AgentContext.ToProviderHistory assembles, in the same order the UI grid
    // legend reads top-to-bottom. The fields sum to CurrentTokens.
    //
    // Persona system prompt built per-turn from ConversationState.
    int SystemPromptTokens,
    // Per-project SystemPrompt override (0 when project has none).
    int ProjectPromptTokens,
    // Saved memories block (user + project scope, 0 when empty).
    int MemoryTokens,
    // Rolling summary system message (0 when not summarized).
    int SummaryTokens,
    // Tool descriptors sent in the "tools" sibling field of the chat call.
    int ToolsTokens,
    // Active post-cut messages (excludes the rolling summary).
    int ConversationTokens);
