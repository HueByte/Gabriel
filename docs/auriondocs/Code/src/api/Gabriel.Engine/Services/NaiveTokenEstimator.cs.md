# NaiveTokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs`  
> **Kind:** class

```csharp
public class NaiveTokenEstimator : ITokenEstimator
```


NaiveTokenEstimator is a lightweight, coarse-grained implementation of ITokenEstimator. It provides a fast heuristic to estimate token usage for a single text fragment and for a collection of messages by counting characters and adding a fixed per-message overhead, suitable for development and prototyping where shipping a real tokenizer is overkill.

## Remarks
This abstraction isolates a pragmatic budgeting tool from the complexities of real tokenizers. It relies on the Message structure's Content, ToolCallsJson, and ToolCallId to approximate the portion of tokens consumed by each message's content and any tool-use metadata, enabling quick experiments with context window planning. If a real tokenizer is introduced later, this class can be swapped to implement the same interface without changing callers.

## Notes
- This is a rough estimate, not a precise token count, and should not be used for billing or strict budgeting in production.
- The per-message overhead is a fixed 8 tokens, which may not reflect actual protocol/serialization costs in all scenarios.
- Null values for Content, ToolCallsJson, or ToolCallId contribute zero tokens due to the null-safe helper used by EstimateText.