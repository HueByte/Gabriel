# IResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs`  
> **Kind:** interface

```csharp
public interface IResponsePostProcessor
```


IResponsePostProcessor defines a contract for post-processing a model's raw response before it is persisted to the database. It currently strips residual AI-era opener/closer phrases and applies a length cap derived from ConversationState, while preserving Markdown formatting (Discord-style). The Clean method accepts a raw string and an optional ConversationState; when state is null implementers should apply a sensible default and still produce a clean, non-destructive result.

## Remarks
This abstraction centralizes content-cleaning logic at the persistence boundary, allowing different implementations to be swapped without changing controllers. By taking an optional ConversationState, it can tailor the length cap and other heuristics to the current turn context, improving consistency across messages and tests.

## Notes
- Implementations must gracefully handle a null ConversationState (use a safe default).
- The cleaning should not alter Markdown syntax beyond removing unwanted opener/closer phrases and trimming length.
- This post-processing is intended for persisted content; avoid altering content during streaming UI interactions.