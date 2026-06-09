# ITokenEstimator

> **File:** `src/api/Gabriel.Engine/Services/ITokenEstimator.cs`  
> **Kind:** interface

Estimates the number of tokens a piece of text or a sequence of chat-style messages will consume for model context-window budgeting. Use this abstraction when callers need an approximate token count but should remain agnostic to the concrete tokenization strategy (for example, a simple char-based estimator in tests or a real BPE/tokenizer implementation for production).

## Remarks
This interface isolates callers from tokenizer details so implementations can be swapped without changing call sites. Typical implementations provide a fast, approximate count rather than an exact tokenizer output; accuracy and rules (how messages are serialized or how special tokens are counted) are implementation-specific and may vary by target model.

## Example
```csharp
// Given an injected ITokenEstimator
int tokensForText = tokenEstimator.EstimateText(prompt);
int tokensForConversation = tokenEstimator.EstimateMessages(conversationMessages);

// Use the estimate to decide whether to truncate or chunk input before sending to a model
if (tokensForConversation > modelTokenLimit)
{
    // trim messages or summarize
}
```

## Notes
- Estimates are intentionally approximate — do not rely on them for byte-for-byte or token-exact correctness when the model's tokenizer semantics matter.
- The string parameter is nullable in the API; concrete implementations should document how they treat null/empty input (common behavior is to return 0).
- Message token counts depend on how messages are serialized/encoded by an implementation (role prefixes, separators, or special tokens can change totals).