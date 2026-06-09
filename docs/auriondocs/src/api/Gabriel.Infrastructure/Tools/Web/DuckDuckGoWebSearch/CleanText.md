Decodes HTML entities, strips HTML tags using TagStripRegex, and trims leading/trailing whitespace. Reach for this helper when converting small HTML fragments or search-result snippets into plain text for display or indexing.

## Remarks
This private utility provides a lightweight sanitization step for HTML-derived strings: first it Html-decodes any entities, then removes tags via TagStripRegex, and finally trims surrounding whitespace. It is intended for short, well-formed HTML snippets where the overhead of a full HTML parser would be unnecessary.

## Example
```csharp
var html = "<p>Hello &amp; <b>world</b></p>";
var plain = CleanText(html); // "Hello & world"
```

## Notes
- The method expects a non-null input; callers should ensure `raw` is not null before calling (the regex replace step will throw if given null).
- Stripping HTML with a regular expression is fast but not fully robust: it may mishandle malformed HTML, nested/script/style content, or comments. Use an HTML parser for complex or untrusted HTML.
- Trim only removes leading/trailing whitespace; internal spacing is preserved.