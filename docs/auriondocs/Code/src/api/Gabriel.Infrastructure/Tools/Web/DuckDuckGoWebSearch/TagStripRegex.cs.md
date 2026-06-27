A precompiled, reusable regular expression that matches simple HTML/XML tags (anything starting with '<' and ending with '>'). It is intended for quick removal of markup via Replace when you need a fast, low-overhead way to strip tags from small or well-formed HTML fragments inside the containing class.

## Remarks
This field is declared static and readonly and uses RegexOptions.Compiled to improve performance when the same pattern is applied repeatedly. Being a shared, compiled Regex reduces repeated parsing and allocation and is safe for concurrent use across threads.

## Example
```csharp
// Remove all tags from an HTML fragment
string html = "<p>Hello <strong>world</strong>!</p>";
string plain = TagStripRegex.Replace(html, ""); // "Hello world!"
```

## Notes
- This is a simple tag stripper, not an HTML parser: it can fail on malformed HTML or on attribute values that contain unescaped '>' characters.
- The regex does not decode HTML entities (e.g., &amp; remains &amp;). Perform entity decoding separately if needed.
- Avoid using this for security-sensitive sanitization or for complex HTML manipulation; prefer an HTML parser/library for robust handling.