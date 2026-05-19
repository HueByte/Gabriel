namespace Gabriel.API.Contracts.Conversations;

// Mirrors ContextMetrics (Gabriel.Engine) for over-the-wire transport. Kept as
// a separate record so the API surface doesn't leak engine-layer types and
// stays stable across internal refactors.
public record ContextMetricsResponse(
    // Estimated tokens we'd send for the next turn - sum of the rolling
    // summary (if any) plus every message after the last compact cut.
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
    // Token cost of the rolling summary itself (0 when not summarized).
    int SummaryTokens);
