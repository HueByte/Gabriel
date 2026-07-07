# NaiveTokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/NaiveTokenEstimator.cs`  
> **Kind:** class

```csharp
public class NaiveTokenEstimator : ITokenEstimator
```


NaiveTokenEstimator provides a rough token count for text and messages using a simple 4 characters per token heuristic. It is intended to support context-window budgeting in development or prototype scenarios before integrating a production-grade tokenizer. It implements ITokenEstimator and exposes two entry points: EstimateText, which estimates tokens for a single string, and EstimateMessages, which sums token estimates for a collection of Message instances, including a small per-message overhead for role markers and separators and separate estimates for content, tool calls, and tool call identifiers. This class is a drop-in substitute during early stages and can be replaced by a real tokenizer without changing its consumers.

## Remarks
This abstraction exists to provide a lightweight, dependency-free way to approximate token usage during development, enabling predictable budgeting of what can fit within a context window. By implementing the ITokenEstimator interface, callers can swap in a production-ready tokenizer later with minimal code changes, keeping token accounting centralized. It also aligns token estimation with the Message model by accounting for message content and associated tool-calls as part of the total estimate.

## Notes
- The 4 characters per token heuristic is intentionally coarse and may misestimate token counts for real models and languages.
- Per-message overhead (8 in this implementation) is a fixed cost added for each Message, independent of its actual content; adjust only if the surrounding protocol or messaging surface changes.
- Null or empty text yields zero tokens; ToolCallsJson and ToolCallId are included in the per-message estimate even if they are null or empty.
