# ResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs`  
> **Kind:** class

```csharp
// Markdown stripping was intentionally removed - the persona allows discord-style
// inline emphasis (bold, italic, code, quotes) because users expect it. Only
// AI-ism openers/closers remain as a cosmetic safety net; the persona prompt is
// the primary defense.
//
// The previous length cap was removed too: it truncated the persisted message
// to less than the live-streamed text, which made the response look "cut off"
// after reloading the conversation from the DB.
public sealed class ResponsePostProcessor : IResponsePostProcessor
```


Removes common conversational "AI-ism" prefixes and suffixes from a model-generated response string, returning a trimmed, cleaner piece of text suitable for display or persistence. Reach for this when you want to strip predictable lead-ins (e.g. "That's a great question…", "Here’s what I think…") and closers (e.g. "Hope that helps", "Let me know if you have any questions") from assistant output so the stored or shown message focuses on substantive content.

## Remarks
The processor maintains two compiled, case-insensitive Regex lists (openers and closers) and applies them in order to the trimmed input. Openers are anchored to the start of the text and closers to the end, so matching phrases are removed from the respective ends; the loop over the arrays means multiple different opener/closer patterns can be stripped in one pass. Regexes are compiled for performance; the class is lightweight and intended as a post-step after generation but before saving or presenting responses. The optional ConversationState parameter is accepted for future use but is not inspected by the current implementation.

## Example
```csharp
var processor = new ResponsePostProcessor();
string raw = "Absolutely — here\'s what I think: Use a factory for this. Hope that helps!";
string clean = processor.Clean(raw, state: null);
// clean -> "Use a factory for this."
```

## Notes
- If raw is null, empty, or consists only of whitespace the method returns the original raw value unchanged (it does not trim or normalize those inputs).
- The state parameter is currently unused; callers can pass null. It exists for potential future rules that may depend on conversation context.
- The opener/closer patterns are opinionated and limited to the configured phrases; unexpected variants or languages will not be stripped. Regex instances are compiled and safe for concurrent use, though the static arrays themselves could be mutated if modified elsewhere (the implementation does not modify them).