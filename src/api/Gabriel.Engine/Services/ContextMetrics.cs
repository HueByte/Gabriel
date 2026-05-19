namespace Gabriel.Engine.Services;

// Snapshot of what AgentService would see if it were about to send the next
// turn — same numbers MaybeCompactAsync compares against the threshold.
// Surfaced via IAgentService.GetContextMetricsAsync so the UI can show a
// "context used / until compact" indicator that matches the backend's
// actual decision.
public record ContextMetrics(
    int CurrentTokens,
    int ContextWindowTokens,
    int CompactThresholdTokens,
    double CompactThresholdRatio,
    int MessagesAfterCut,
    bool IsSummarized,
    int SummaryTokens);
