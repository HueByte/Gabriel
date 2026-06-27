# ITokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/ITokenEstimator.cs`  
> **Kind:** interface

```csharp
// Approximate token count for context-window budgeting. Behind an interface so
// the naive char-based impl can be swapped for a real BPE tokenizer later
// without touching callers.
public interface ITokenEstimator
```


Estimates how many tokens a piece of text or a sequence of messages would consume. Use this when you need a cheap, approximate token count for budgeting, truncation, or pre-checks (for example before sending a prompt to a model) without performing a full, model-specific tokenization.

## Remarks
This abstraction decouples callers from the concrete tokenization strategy so the project can start with a simple, character-based estimator and later swap in a proper BPE/tokenizer implementation without changing call sites. It exists to provide a lightweight, pluggable contract for estimating token usage across different input shapes (single text values and collections of Message objects).

## Example
```csharp
// Given an implementation of ITokenEstimator (e.g. a simple char-counting estimator
// or a model-aware BPE estimator), usage is straightforward:
ITokenEstimator estimator = new CharCountingTokenEstimator();

int single = estimator.EstimateText("Hello, world!");
int batch = estimator.EstimateMessages(chatHistory); // chatHistory is IEnumerable<Message>

// The concrete estimator determines how messages are inspected and counted.
```

## Notes
- Estimates are approximate — different implementations (char-based vs. BPE) will produce different counts; do not rely on these values for exact billing or model-internal token limits without validating against the model's tokenizer.
- The interface accepts a nullable string for EstimateText but does not document the expected behavior for null or empty inputs (e.g., return 0 vs. throw). Check the chosen implementation's contract.
- Thread-safety and reentrancy are not specified; if you use a shared estimator instance across threads, confirm the implementation is safe for concurrent use.