A precompiled, shared regular expression that matches HTML/XML-style tags (any sequence starting with '<', containing one or more characters that are not '>', and ending with '>'). The field provides a fast, reusable way for the containing class to remove simple markup from strings without recompiling the pattern on every use.

## Remarks
The pattern is "<[^>]+>": it requires at least one character between angle brackets and will match comments, typical start/end tags, and other tag-like sequences. Declared static readonly and created with RegexOptions.Compiled so the compiled machine code is reused across calls for better performance in scenarios where tag-stripping is performed frequently. The field itself is read-only and safe for concurrent use as a reader.

## Example
```csharp
// from inside the same class (field is private)
string input = "<p>Hello <strong>world</strong>!</p>";
string plain = TagStripRegex.Replace(input, string.Empty); // "Hello world!"
```

## Notes
- This regex is intended for simple tag removal, not robust HTML parsing; use an HTML parser (e.g., HtmlAgilityPack) for complex or malformed HTML.
- The pattern does not match empty angle brackets ("<>") because it requires one or more characters between '<' and '>'.
- Attribute values that themselves contain '>' can cause incorrect truncation because the regex stops at the first '>' it encounters.
- The compiled option improves performance but incurs a one-time compilation cost at type initialization.