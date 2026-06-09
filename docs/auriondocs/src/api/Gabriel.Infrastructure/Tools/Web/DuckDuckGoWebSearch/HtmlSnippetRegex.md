Matches an anchor (<a>...</a>) whose class attribute contains the token "result__snippet" and captures the anchor's inner content into a named group called "text". Use this field when extracting DuckDuckGo result snippets from raw HTML returned by a search page.

## Remarks
This compiled, single-instance Regex is optimized for repeated extraction of snippet anchors from DuckDuckGo search-result HTML. It uses RegexOptions.Singleline so the dot (.) spans newlines (allowing multi-line snippets) and RegexOptions.Compiled for runtime performance. The regex expects the class attribute to use double quotes and looks specifically for the CSS token "result__snippet" (word-boundary aware) inside the class value.

## Example
```csharp
// html contains a fragment of DuckDuckGo search results
var m = HtmlSnippetRegex.Match(html);
if (m.Success)
{
    // captured inner HTML (may include tags); trim or decode as needed
    string snippetHtml = m.Groups["text"].Value;
}
```

## Notes
- The capture contains the anchor's inner HTML, not plain text; strip tags or HTML-decode when you need readable text.
- The pattern requires the class attribute to be double-quoted; anchors using single quotes won't match.
- Because it looks for the first closing </a>, malformed or nested anchors can cause unexpected captures.
- The regex is specific to DuckDuckGo's current markup (the "result__snippet" class); changes to page structure will break extraction.