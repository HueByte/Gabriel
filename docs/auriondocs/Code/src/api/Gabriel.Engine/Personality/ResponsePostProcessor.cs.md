# ResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs`  
> **Kind:** class

```csharp
public sealed class ResponsePostProcessor : IResponsePostProcessor
```


ResponsePostProcessor is a sealed class that post-processes AI responses to strip common introductory and closing boilerplate phrases, producing content-focused text for downstream processing or display. Its Clean method accepts a raw response and an optional ConversationState, removes any configured opener from the beginning and any configured closer from the end, and returns the trimmed substantive content. If the input is null or whitespace, it is returned unchanged.

## Remarks
This abstraction centralizes the normalization of canned AI persona text, enabling downstream components (such as UI rendering or logging) to work with cleaner, content-first messages. By implementing IResponsePostProcessor, it plugs into the response-generation pipeline as a dedicated normalization step, ensuring consistent handling of boilerplate regardless of source. The state parameter is accepted for interface compatibility and potential future stateful conditioning, but is not currently used in the logic.

## Notes
- If a legitimate user message begins with one of the opener phrases, that portion will be stripped, potentially removing content the author intended to keep.
- The opener and closer lists are static; there is no runtime configuration. Adjusting behavior requires code changes to these patterns.
- The methods rely on precompiled regular expressions for performance; extremely long inputs will still be processed by the regex engine, but the impact is minimized by compilation and straightforward replacements.