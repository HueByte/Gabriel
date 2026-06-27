Converts a raw HTML fragment into trimmed plain text by first HTML-decoding entities, then removing HTML tags via TagStripRegex, and finally trimming surrounding whitespace. Use this helper when you need a normalized, human-readable snippet from HTML (for example, search result snippets or lightweight HTML fragments) rather than rendering HTML directly.

## Remarks
This private helper centralizes the common normalization used by the DuckDuckGo web search integration: decode any HTML entities so their textual equivalents remain, then strip tags to leave only literal text, and trim excess whitespace. Doing the decode step before tag-stripping ensures encoded angle brackets (e.g. &lt;&gt;) become their character forms before removal.

## Example
```csharp
string raw = "Hello &amp; <strong>world</strong>   ";
var cleaned = CleanText(raw); // "Hello & world"
```

## Notes
- The method does not perform a null check: passing null will result in an exception when TagStripRegex.Replace is called. Ensure callers pass a non-null string (e.g. raw ?? string.Empty) if nulls are possible.
- The effectiveness of tag removal depends on TagStripRegex; it may not handle malformed HTML or complex constructs (scripts/styles) the same way a full HTML parser would.
- The HtmlDecode step can produce characters that look like markup; the decode-then-strip order is intentional to ensure entities are converted before tags are removed.