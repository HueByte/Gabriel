A precompiled, private static readonly regular expression used to extract the visible snippet text from a DuckDuckGo search result’s HTML block. It targets an anchor element whose class attribute contains the word boundary-encapsulated token result__snippet and captures the element’s inner text into a named group called text. The expression is compiled for performance and uses Singleline so the snippet can span multiple lines, enabling efficient, repeated parsing of search results without recreating the Regex instance.

## Remarks
>This symbol centralizes the HTML-snippet extraction logic for DuckDuckGo search results. By caching a compiled pattern, it avoids re-parsing logic scattered across callers and communicates the intent clearly: pull the human-readable snippet from each result’s anchor text. Because it is private and static, the Regex instance is effectively shared across invocations, providing consistent behavior and reduced allocation on repeated parses.

## Example
```csharp
// Example usage
var m = HtmlSnippetRegex.Match(htmlFragment);
if (m.Success)
{
    string snippet = m.Groups["text"].Value;
    // Use snippet as the result summary
}
```

## Notes
- The regex relies on DuckDuckGo’s HTML structure (class containing result__snippet); any change to markup or class names may break extraction. Update the pattern if the page structure changes.
- If you need all snippets from a page, use Regex.Matches instead of a single Match call, and iterate over the collection.
- Because the Regex is static and compiled, it is safe to reuse across threads for read-only matching, avoiding repeated allocations.