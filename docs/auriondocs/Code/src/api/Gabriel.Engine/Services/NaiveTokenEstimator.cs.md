# NaiveTokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs`  
> **Kind:** class

```csharp
public class NaiveTokenEstimator : ITokenEstimator
```


NaiveTokenEstimator provides a coarse token count for text and messages using a simple 4-char-per-token heuristic plus a small per-message overhead, intended for rough context-window budgeting in development or prototype settings. It is not a substitute for real BPE tokenization; switch to a proper tokenizer if token-level accuracy matters.

## Remarks
This abstraction exists to enable quick, deterministic budgeting without shipping a full tokenizer. It centralizes a minimal token-counting strategy that complements the integration with a language model by offering predictable, fast estimates. It is especially useful during early development or exploratory UI work where exact counts are unnecessary.

## Notes
- It is intentionally approximate; do not rely on it for billing or quotas.
- Per-message overhead is fixed (8 characters) and may not reflect your exact protocol/serialization.
- Counts are based on string lengths; non-ASCII or multi-byte characters may affect actual token counts.
