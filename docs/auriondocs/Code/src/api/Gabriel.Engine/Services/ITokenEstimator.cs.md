# ITokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/ITokenEstimator.cs`  
> **Kind:** interface

```csharp
public interface ITokenEstimator
```


Estimates the number of tokens that text or a sequence of messages would consume when prepared for a language model. Use this interface to approximate prompt size or quota usage without committing to a specific tokenizer, so callers can swap in a real BPE-based implementation later without changing call sites.

## Remarks
By abstracting token counting behind an interface, callers remain decoupled from tokenization details and can swap in test doubles or lightweight estimators during development. The two members, EstimateText and EstimateMessages, cover raw text payloads and chat-history scenarios respectively, and should produce consistent counts for the same input. Implementations should be deterministic and side-effect-free to ensure predictable budgeting.

## Notes
- Null input: EstimateText(text: null) should yield a safe, defined result (often 0 tokens); respect the contract of your implementation.
- Thread-safety: Not guaranteed; if the estimator is shared across threads, ensure thread-safety in the implementation or enforce single-threaded usage.
- Enumeration considerations: EstimateMessages may iterate the input; materialize if you need to reuse the collection to avoid multiple traversals.