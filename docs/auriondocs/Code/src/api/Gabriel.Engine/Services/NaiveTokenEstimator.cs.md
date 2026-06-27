# NaiveTokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs`  
> **Kind:** class

Provides a very small, dependency-free token estimator that approximates token counts by treating ~4 characters as one token and adding a fixed per-message overhead. Use this class when you need a fast, lightweight context-window or budget estimate in development or prototype scenarios and you do not want to ship a full tokenizer/vocabulary.

## Remarks
This estimator is intentionally coarse: it favors simplicity and zero external dependencies over accuracy. It exists to give quick, conservative budgeting for message payloads (counts message content plus two tool-related fields and a small per-message fixed overhead) and is designed to be replaced by a proper BPE/tokenizer-based estimator when exact token accounting matters.

## Example
```csharp
var estimator = new NaiveTokenEstimator();

// Single text estimate
int tokensForText = estimator.EstimateText("Hello, world!");

// Messages estimate
var messages = new[]
{
    new Message { Content = "User prompt", ToolCallsJson = null, ToolCallId = null },
    new Message { Content = "Assistant reply", ToolCallsJson = "[]", ToolCallId = "t1" }
};
int totalTokens = estimator.EstimateMessages(messages);
```

## Notes
- The estimator uses a simple ceil(length / 4) rule (implemented as (length + 3) / 4) so 1–4 characters -> 1 token, 5–8 -> 2 tokens, etc.
- Real BPE/tokenization can differ substantially (roughly 30–50% different in typical text); do not rely on this for billing or strict token limits.
- Each message adds a fixed overhead of 8 tokens (for role markers/separators), so many short messages will have higher relative overhead.
- EstimateText returns 0 for null or empty input; EstimateMessages calls EstimateText for Content, ToolCallsJson and ToolCallId, so nulls are handled.
