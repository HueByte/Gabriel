Matches anchor elements in DuckDuckGo's "lite" HTML results and extracts the link target and visible text. Use this when parsing the lite endpoint's result rows (which use a single-quoted class attribute named 'result-link') to obtain the href and the anchor text via the named groups 'href' and 'text'.

## Remarks
This compiled, single-instance Regex is optimized for repeated parsing of DuckDuckGo's lite-result HTML. It expects an anchor where the href attribute appears before a class attribute exactly equal to 'result-link' (single quotes) and captures the href (from double quotes) and the inner HTML of the anchor. The Singleline option allows the inner text capture to span newlines; the Compiled option improves runtime performance for repeated matches.

## Example
```csharp
var match = LiteResultLinkRegex.Match(htmlSnippet);
if (match.Success)
{
    string href = match.Groups["href"].Value;   // captured from href="..."
    string innerHtml = match.Groups["text"].Value; // captured inner HTML/text of the anchor
}
```

## Notes
- The regex assumes the href attribute appears before the class='result-link' attribute; if the attribute order differs the pattern will not match.
- It specifically expects href to use double quotes and the class attribute to use single quotes; variations in quoting will break the match.
- Parsing arbitrary or malformed HTML with regular expressions is brittle — use an HTML parser if you need robustness against formatting/attribute-order changes.
- The field is static/readonly and the Regex type is safe for concurrent matches, so it can be reused across threads without synchronization.