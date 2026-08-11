# HeuristicConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs`  
> **Kind:** class

```csharp
public sealed class HeuristicConversationStateUpdater : IConversationStateUpdater
```


A deterministic, microsecond-cost implementation of IConversationStateUpdater that derives simple conversation signals from a single user message using compiled regular expressions and a token estimator. Reach for this class when you want predictable, zero-LLM per-turn state updates (turn counts, token-based length statistics, lightweight mood cues, topic hints, and simple stylistic signals such as emoji or lowercase usage) instead of a costlier LLM-backed analysis.

## Remarks
This updater is intentionally heuristic and conservative: it trades some classification accuracy for predictability and near-zero runtime overhead. It uses a token estimator to compute per-message token counts and an exponential-moving-average to update the user's average token length, and it applies a set of compiled, case-insensitive regexes (detail/task cues, polite suffixes, playful/serious markers, and a small negative-affect lexicon) plus a simple emoji check to derive surface signals. The class exists as a lightweight default/updater replacement where deterministic behavior and cost-control are more important than deep intent understanding; an LLM-based updater with the same interface can be swapped in later when higher fidelity is required.

## Example
```csharp
// Typical, minimal usage
ITokenEstimator tokenEstimator = /* your token estimator */;
var updater = new HeuristicConversationStateUpdater(tokenEstimator);
var state = ConversationState.Initial();

state = updater.Update(state, "write a sorting algorithm please");
// state now has TurnCount incremented, LastUserTokenCount and AvgUserTokenCount updated,
// Mood adjusted by classification, and simple stylistic/topic signals set.
```

## Notes
- The ShortTokenThreshold and other tunings are heuristic (comments indicate ~4 chars/token assumptions) and may need adjustment for different tokenizers or languages.
- Detail/task detection relies on a word-boundary-anchored, compiled regex so common substrings won't usually trigger false positives; the regexes are case-insensitive and compiled for performance.
- Emoji detection is a lightweight check (high-surrogate characters plus the U+2600–U+27BF range) and is not a comprehensive Unicode emoji detector.
- This implementation is intentionally conservative and may misclassify mood or miss nuanced intent; use an LLM-backed updater if you require deeper semantic understanding.