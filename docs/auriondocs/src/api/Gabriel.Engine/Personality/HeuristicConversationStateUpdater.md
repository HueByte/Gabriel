# HeuristicConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs`  
> **Kind:** class

Updates a ConversationState from a user's message using deterministic, microsecond-cost heuristics (regular expressions and a token estimator). Reach for this implementation when you need predictable, zero-cost per-turn state updates — for example in high-throughput or offline scenarios — or as a cheap fallback in place of an LLM-backed updater.

## Remarks
This class provides a lightweight alternative to an LLM-based conversation-state updater: it estimates token counts, detects task/detail cues, playful or serious tone, basic venting signals, and emoji presence using compiled regular expressions and simple heuristics. It smooths per-turn token counts with an exponential moving average and returns an updated ConversationState value (incremented turn count, updated token stats, mood, recent topics, etc.). The implementation favors predictability and performance over perfect classification and is intentionally conservative in its lexical cues so it minimizes false positives.

## Example
```csharp
// Given an ITokenEstimator implementation:
var tokenEstimator = new MyTokenEstimator();
var updater = new HeuristicConversationStateUpdater(tokenEstimator);

ConversationState current = ConversationState.Initial();
string userMessage = "Can you explain how bubble sort works?";

ConversationState next = updater.Update(current, userMessage);
// next has TurnCount incremented, updated AvgUserTokenCount, Mood inferred, etc.
```

## Notes
- The heuristics are English-centric and rely on regex-based cue detection; non‑English input or unusual phrasing may be misclassified.
- Emoji detection is intentionally lightweight (high-surrogate check + a Unicode range) and can miss or falsely match some symbols.
- Token counts are smoothed via an exponential moving average (the implementation uses a 0.7/0.3 weighting for historical vs. current tokens), which affects how quickly AvgUserTokenCount responds to short or long messages.
- The updater makes no external LLM calls and therefore trades accuracy for latency and cost; consider swapping in an LLM-backed updater when richer semantic understanding is required.
- Thread-safety depends on the provided ITokenEstimator; if that dependency is not safe for concurrent use, protect calls to Update externally.