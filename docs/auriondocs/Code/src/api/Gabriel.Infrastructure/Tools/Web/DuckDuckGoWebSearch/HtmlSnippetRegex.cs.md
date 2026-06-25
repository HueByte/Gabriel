# HtmlSnippetRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex HtmlSnippetRegex = new(
        @"class=""[^""]*\bresult__snippet\b[^""]*""[^>]*>(?<text>.*?)</a>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


This private, precompiled Regex matches the anchor element in DuckDuckGo search result HTML whose class includes result__snippet and captures its inner text as the group 'text'. It is used when the code needs to extract the visible snippet from a page's HTML, avoiding ad hoc string handling and re-compiling the pattern on every call.

## Remarks
Centralizes HTML-snippet extraction and improves performance by reusing a single compiled Regex. It is brittle to HTML structure changes; if DuckDuckGo alters their markup or class names, the pattern will likely need updating.

## Example
```csharp
// Example usage (within the same class)
var m = HtmlSnippetRegex.Match(htmlFragment);
if (m.Success)
{
    string snippet = m.Groups["text"].Value;
}
```

## Notes
- Matches a single snippet per input; to process all results, iterate over Matches.
- The captured text may require decoding HTML entities.
- The Groups["text"] value may contain leading or trailing whitespace.