namespace Gabriel.Engine.Services;

// Snapshot of what AgentService would see if it were about to send the next
// turn - same numbers MaybeCompactAsync compares against the threshold.
// Surfaced via IAgentService.GetContextMetricsAsync so the UI can show a
// "context used / until compact" indicator that matches the backend's
// actual decision.
//
// The per-category breakdown fields (SystemPromptTokens .. ConversationTokens)
// mirror exactly what ToProviderHistory assembles, so the grid visualization
// adds up to CurrentTokens within a single per-message overhead.
public record ContextMetrics(
    int CurrentTokens,
    int ContextWindowTokens,
    int CompactThresholdTokens,
    double CompactThresholdRatio,
    int MessagesAfterCut,
    bool IsSummarized,
    int SummaryTokens,
    // Persona system prompt assembled per-turn from ConversationState.
    int SystemPromptTokens,
    // Per-project SystemPrompt override (0 when project has none).
    int ProjectPromptTokens,
    // Saved memories block (user + project scope, 0 when empty).
    int MemoryTokens,
    // Tool descriptors (Name + Description + JSON schema) sent alongside
    // the messages. Not part of the messages array but billed against the
    // same context window by every provider we target.
    int ToolsTokens,
    // Active post-cut messages only (excludes the rolling summary).
    int ConversationTokens);
