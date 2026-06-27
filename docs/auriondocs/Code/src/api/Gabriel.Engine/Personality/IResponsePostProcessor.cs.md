# IResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs`  
> **Kind:** interface

Cleans a model-produced response before it is stored: strips residual assistant-style openers/closers and enforces a length cap (when provided via ConversationState) while intentionally preserving Markdown formatting. Use this when finalizing and persisting a message to the database, not when streaming live deltas to a client.

## Remarks
This interface centralizes post-generation sanitization and length-policy enforcement so controllers and persistence layers remain agnostic of AI-specific cleanup rules. It supports a "stream raw, clean on save" workflow where clients may receive raw intermediate text but the canonical saved message is normalized and size-limited.

## Example
```csharp
// In a controller or repository before saving the final message:
string raw = aiModel.Generate(...);
string cleaned = responsePostProcessor.Clean(raw, conversationState);
messageRepository.Save(cleaned);

// If ConversationState is not available, pass null. Behavior (default cap or no cap)
// depends on the implementation.
string cleanedWithoutState = responsePostProcessor.Clean(raw, null);
```

## Notes
- Markdown is preserved intentionally; do not perform additional escaping unless you intend to change presentation.
- Implementations may apply different behavior when ConversationState is null (e.g., no cap vs. a default cap); provide state when deterministic length enforcement is required.
- Truncation to enforce length limits can cut mid-markup and produce malformed Markdown; callers that require well-formed markup should validate or reformat after cleaning.