# HeuristicConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs`  
> **Kind:** class

```csharp
public sealed class HeuristicConversationStateUpdater : IConversationStateUpdater
```


A lightweight, deterministic conversation-state updater that uses simple heuristics (token counts, regex cues, emoji checks and a small negative-affect lexicon) to maintain per-user metrics such as turn count, average/last message token counts, mood, recent topics and flags like whether the user asked for more detail or uses emoji. Pick this implementation when you need a zero-cost, predictable updater for UI signals or routing decisions and do not want to invoke an LLM for per-turn analysis.

## Remarks
This class encodes a few intentionally conservative heuristics rather than relying on any probabilistic model: a short-message threshold, anchored regexes that detect task/detail requests and politeness suffixes, a compact venting lexicon, and a cheap emoji detector based on surrogate pairs and a Unicode range. The design favors predictability and microsecond-level per-turn overhead; it is suitable as a fast default or fallback in environments where cost or latency forbids LLM calls. Expect trade-offs in accuracy — the implementation accepts some mood or intent mis-classifications in exchange for consistency and speed.

## Example
```csharp
// tokenEstimator is an ITokenEstimator implementation provided by the host
var tokenEstimator = /* get ITokenEstimator */;
var updater = new HeuristicConversationStateUpdater(tokenEstimator);
var state = ConversationState.Initial();
state = updater.Update(state, "Can you explain how bubble sort works?");
// state now has updated TurnCount, AvgUserTokenCount, Mood, RecentTopics, etc.
```

## Notes
- The short-message cutoff is tuned via ShortTokenThreshold (10 tokens) and assumes roughly 4 characters per token; token-estimator accuracy affects behavior. 
- Emoji detection is intentionally lightweight and approximate (checks for high surrogates and a Unicode symbol range); it may miss or over-detect some symbols. 
- This implementation is deterministic and not an LLM: it may mis-classify mood/intent in ambiguous cases and is best used where predictability and cost-free operation are priorities.