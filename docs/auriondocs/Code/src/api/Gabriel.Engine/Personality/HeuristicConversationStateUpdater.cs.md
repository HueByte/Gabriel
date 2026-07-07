# HeuristicConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/HeuristicConversationStateUpdater.cs`  
> **Kind:** class

```csharp
public sealed class HeuristicConversationStateUpdater : IConversationStateUpdater
```


A lightweight, deterministic updater that derives conversational metrics from a single user message using pure heuristics (no LLM calls). Use this when you need a very fast, zero-cost per-turn update to ConversationState — for example to maintain turn counts, an exponential moving average of user token length, and a coarse mood/topic signal — and you can tolerate occasional mis‑classifications.

## Remarks
This class implements a minimal, predictable alternative to an LLM-backed state updater. It applies a set of small, English-oriented regular expressions and simple token-based rules (short-message threshold, task/detail cues, please-suffix detection, playful/serious cues, a small negative-affect lexicon, and emoji presence) to classify user intent/mood and update conversation-level metrics. Because it avoids external calls it is extremely cheap (microsecond-scale per turn) and deterministic, making it suitable for high-throughput or privacy-sensitive scenarios where cost and latency matter more than perfect accuracy. The implementation is intentionally conservative: it prefers simplicity and predictability over coverage, and is designed so an LLM-based updater can be swapped in later while keeping the same interface.

## Example
```csharp
// Create with any implementation of ITokenEstimator, then call Update each turn.
ITokenEstimator tokens = new MyTokenEstimator();
var updater = new HeuristicConversationStateUpdater(tokens);
ConversationState state = ConversationState.Initial();
state = updater.Update(state, "Can you explain how bubble sort works?");
// state now has updated TurnCount, AvgUserTokenCount, LastUserTokenCount and a heuristic Mood
```

## Notes
- The heuristics are English-centric and rely on fixed regex patterns; they may miss or mis-classify non-English input or idiomatic phrasing.
- Emoji detection is implemented via surrogate checks and a Unicode range; some emoji or symbol edge-cases may not be recognized.
- The short-message token threshold and regexes were tuned for typical tokenization assumptions (roughly 4 chars/token); different tokenizers or languages may yield different behavior.
- This component is deterministic and thread-safe from its visible surface (no shared mutable state other than readonly dependencies), but callers should manage shared ConversationState instances according to their concurrency model.
