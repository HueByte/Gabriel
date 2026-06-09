# NaiveTokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs`  
> **Kind:** class

A very fast, dependency-free token-count estimator that approximates token counts using a simple "one token per four characters" heuristic and a fixed per-message overhead. Reach for this implementation when you need quick context-window or UI-facing estimates in development or prototypes and do not want to ship a full BPE tokenizer/vocabulary.

## Remarks
This class implements ITokenEstimator with a deliberately coarse approximation to avoid the complexity and size of a real tokenizer. It adds a small constant per-message overhead (to represent role markers and separators) and estimates content token counts by dividing character length by four. The implementation is intentionally simple so it can be swapped out later for a more accurate BPE-based estimator when exact token accounting matters.

## Example
```csharp
var estimator = new NaiveTokenEstimator();

// Estimate a single piece of text
int approxTokens = estimator.EstimateText("Hello, this is a short message.");

// Estimate a sequence of Message objects
var messages = new List<Message>
{
    new Message { Content = "Hi there", ToolCallsJson = null, ToolCallId = null },
    new Message { Content = "This is a longer message to estimate.", ToolCallsJson = "{}", ToolCallId = "call-123" }
};

int totalEstimate = estimator.EstimateMessages(messages);
```

## Notes
- This is a coarse heuristic (roughly 1 token per 4 characters); real BPE tokenization typically differs by ~30–50% and is more accurate.
- A fixed MessageOverhead of 8 tokens is added per message to account for role markers/separators; EstimateText returns 0 for null or empty strings.
- Not suitable for billing, strict quota enforcement, or production systems that require exact token counts—replace with a real tokenizer/vocab in those cases.