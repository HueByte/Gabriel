Private static helper that normalizes raw HTML text into plain text.

It decodes HTML entities using WebUtility.HtmlDecode, strips HTML tags via TagStripRegex, and then trims whitespace. This sequence yields display-ready text from HTML-encoded content, suitable for UI rendering or indexing where markup should not be exposed.

## Remarks
This centralizes text cleaning for content sourced from the web, ensuring consistent removal of markup before the text is consumed by the UI or downstream processing. It relies on TagStripRegex to strip tags and on WebUtility.HtmlDecode to unescape entities, so changes to those dependencies affect all call sites.

## Example
```csharp
string raw = "<p>Hello &amp; world!</p>";
string cleaned = CleanText(raw);
// cleaned == "Hello & world!"
```

## Notes
- Null input is not guarded; passing null may throw when HtmlDecode is invoked.
- For very large strings, the method creates intermediate strings through decoding and regex replacement, which could impact performance in tight loops.