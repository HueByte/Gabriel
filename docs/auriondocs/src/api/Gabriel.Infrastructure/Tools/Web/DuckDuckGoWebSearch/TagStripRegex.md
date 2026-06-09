Matches simple HTML/XML-style tags (any sequence that starts with '<' and continues until the next '>') and is intended for quick removal of markup from text. Use this precompiled Regex when you need a fast, one-shot way to strip tags from small or well-formed HTML fragments (for example, cleaning search-result snippets) rather than performing full HTML parsing.

## Remarks
This field provides a ready-to-use, compiled regular expression instance to avoid recompiling the same pattern repeatedly. It favors speed for straightforward tag-removal tasks and is located privately so callers in the same class can reuse the same compiled pattern.

## Example
```csharp
string html = "<p>Hello <strong>world</strong>!</p>";
string textOnly = TagStripRegex.Replace(html, string.Empty);
// textOnly == "Hello world!"
```

## Notes
- This regex is a heuristic, not an HTML parser: it will remove anything between '<' and the next '>', which can be incorrect for malformed HTML or for tags that include '>' inside quoted attribute values.
- Be cautious with scripts, comments, CDATA sections, or embedded SVG/JS content; those can contain '>' characters or require more sophisticated handling.
- The RegexOptions.Compiled flag improves repeated-match performance but increases initial JIT/compile cost and memory usage.