HtmlResultBlockRegex is a private, static Regex used to extract individual result blocks from the HTML endpoint response. It matches a div whose class contains the word 'result' and captures its inner content as the named group 'body', stopping before the next result block, the bottom spacing marker, or end-of-content.

## Remarks
Centralizes the HTML parsing pattern for search results behind a single, reusable symbol, reducing duplication and making future HTML structure changes easier to accommodate. The pattern uses Singleline and RegexOptions.Compiled to balance correctness across multi-line HTML and runtime performance. The accompanying comment notes a graceful fallback to zero results if the server surface changes, which guides callers to handle missing matches without crashing.

## Example
```csharp
foreach (Match m in HtmlResultBlockRegex.Matches(html))
{    
    string body = m.Groups["body"].Value;
    // Process blockHtml here
}
```

## Notes
- The regex relies on the presence of a class containing the word 'result' in the HTML; if the markup changes (e.g., different class names or casing), the matches may be lost.
- Because the field is static readonly and compiled, it initializes once per AppDomain and requires recompilation to reflect changes; consider dependencies on this behavior when evaluating hot-reload scenarios.