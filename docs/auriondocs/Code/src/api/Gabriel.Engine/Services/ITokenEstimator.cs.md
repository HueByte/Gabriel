# ITokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/ITokenEstimator.cs`  
> **Kind:** interface

```csharp
public interface ITokenEstimator
```


ITokenEstimator is a lightweight abstraction that exposes two token-estimation strategies: EstimateText for a single string and EstimateMessages for a sequence of Message instances. It enables callers to approximate how many tokens would be consumed when preparing inputs for token-limited models without binding to a specific tokenizer implementation. The interface is intentionally behind a simple contract so callers can remain agnostic to how tokens are counted, allowing a naive character-based approach to be swapped later for a real BPE tokenizer without changing call sites.

## Remarks
This abstraction decouples token accounting from the underlying tokenizer, enabling consistent budgeting for model inputs and preflight validation. It centralizes token estimation logic, making it easier to calibrate, test, and evolve tokenizer strategies across the codebase. By operating on the Message type, it supports reasoning about the token cost of a conversation payload when preparing requests to chat models.

## Notes
- The text parameter is nullable; implementations must handle nulls gracefully (e.g., treat null as producing zero tokens).
- If you rely on EstimateMessages, decide which parts of Message contribute to token count (for example Content versus ToolCallsJson) and document the policy to avoid budget mismatches across components.