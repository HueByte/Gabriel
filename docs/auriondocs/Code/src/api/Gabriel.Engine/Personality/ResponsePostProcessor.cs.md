# ResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs`  
> **Kind:** class

Removes common "AI-ism" opener and closer phrases from a generated response so the returned text starts and ends with the substantive content. Reach for this when you need to present or store model output without polite lead-ins or sign-offs (for example, in chat UIs, logs, or streamed transcripts).

## Remarks
This post-processor is a small pipeline-stage utility used after text generation and before rendering or persisting output. It applies a fixed set of case-insensitive, compiled regular expressions to strip known opening prefixes anchored at the start of the string and closing phrases anchored at the end. The class currently accepts a ConversationState parameter to satisfy the interface but does not use it; the array-driven patterns are intentionally centralized so they can be extended if the persona's polite boilerplate changes.

## Example
```csharp
var p = new ResponsePostProcessor();
string raw = "Certainly — here’s what I think. The server should be restarted. Hope that helps!";
string cleaned = p.Clean(raw, state: null);
// cleaned == "— here’s what I think. The server should be restarted."
```

## Notes
- The ConversationState parameter is accepted but ignored by the current implementation.
- Opener regexes are anchored to the start (^) and closer regexes target the end, so only leading/trailing phrases are removed; internal matches are preserved.
- Regexes are compiled and reused (static readonly) for performance and are safe to call concurrently.