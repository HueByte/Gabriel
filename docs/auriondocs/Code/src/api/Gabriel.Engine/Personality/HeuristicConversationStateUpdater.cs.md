# HeuristicConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs`  
> **Kind:** class

Updates conversation-level metadata using deterministic, micro‑cost heuristics rather than invoking an LLM. Use this implementation when you need predictable, very low-latency per-turn updates (token counts, a simple mood classification, recent-topic tracking and other lightweight signals) and are willing to accept some classification errors in exchange for zero runtime cost and full determinism.

## Remarks
This class implements IConversationStateUpdater with a purely heuristic approach: it estimates token counts via an injected ITokenEstimator, updates exponential moving averages for user token length, and classifies simple conversational signals (short/long message, task/detail cues, playfulness/seriousness, venting) using small, compiled regular expressions and an emoji presence check. It exists as a fast, predictable alternative to an LLM-backed updater and is intentionally conservative — the heuristics prioritize stability and performance over exhaustive language understanding so the downstream system can swap in a more sophisticated (LLM) updater later without changing the surrounding code.

## Example
```csharp
// Assume `tokens` implements ITokenEstimator and is supplied by your DI container.
var tokens = /* ITokenEstimator instance */ null!;
var updater = new HeuristicConversationStateUpdater(tokens);
ConversationState state = ConversationState.Initial();
string userMessage = "Can you write a quick function that sorts a list, please?";
state = updater.Update(state, userMessage);
// state now has updated TurnCount, LastUserTokenCount, AvgUserTokenCount, Mood, etc.
```

## Notes
- This implementation is intentionally heuristic: mood and intent classifications can be misclassified, especially on ambiguous or sarcastic input.
- Emoji detection is lightweight and conservative (checks high surrogates and a Unicode range); it does not guarantee coverage of every emoji or symbol.
- Several Regexes are compiled for performance; the matching is case-insensitive and word-boundary anchored to reduce false positives but still remains a best‑effort heuristic.
- Thread-safety: the updater itself holds immutable state except for the injected ITokenEstimator. Concurrent calls are safe only if the provided ITokenEstimator.EstimateText implementation is thread-safe; otherwise callers should synchronize access to the estimator.