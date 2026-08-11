# ITokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/ITokenEstimator.cs`  
> **Kind:** interface

```csharp
public interface ITokenEstimator
```


ITokenEstimator is a pluggable contract for estimating token usage, enabling callers to budget prompts without tying themselves to a specific tokenizer. It exposes EstimateText for plain text and EstimateMessages for a sequence of Message objects, allowing the underlying tokenizer to be swapped later (e.g., from a naive char-based estimator to a real BPE tokenizer) without touching callers.

## Remarks
ITokenEstimator decouples token counting from higher-level logic, so orchestration code can reason about costs and token limits independently from how tokens are computed. By supporting both raw text and collections of Message objects, it accommodates estimations for standalone prompts as well as chat histories. The interface is designed to be easy to mock in tests and to enable swapping tokenizer implementations across the Gabriel engine without changing consumer code.

## Notes
- Null handling: the text parameter is nullable; implementations should handle null gracefully (e.g., treat as zero tokens or skip).
- EstimateMessages semantics: ensure the enumeration is stable and side-effect free; avoid altering input sequences.
- Performance considerations: large collections of messages may be expensive to estimate; implementations should avoid multiple passes or offer streaming-friendly approaches where possible.