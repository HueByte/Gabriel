HtmlTitleLinkRegex is a private static readonly Regex that encapsulates the HTML pattern used to locate the title link elements in DuckDuckGo search results. It matches anchor tags whose class attribute includes the word result__a, and it captures the link href and the displayed text into named groups href and text. The regex is compiled for performance and uses Singleline so the dot in the text portion can span newline characters; being static readonly ensures a single, immutable instance is reused.

## Remarks
HtmlTitleLinkRegex centralizes the HTML-specific parsing pattern inside the DuckDuckGoWebSearch helper, reducing duplication and limiting the impact of HTML changes to a single place. The named groups href and text provide a stable extraction contract for downstream processing. The combination of Compiled and Singleline flags yields fast, one-time initialization and efficient scanning of multiple results.

## Example
```csharp
// Inside the same class that defines HtmlTitleLinkRegex
foreach (Match m in HtmlTitleLinkRegex.Matches(htmlFragment))
{
    string href = m.Groups["href"].Value;
    string title = m.Groups["text"].Value;
    // Process the extracted link and its displayed text
}
```

## Notes
- The pattern depends on DuckDuckGo's HTML structure; a markup change can break it.
- Since the field is private, external callers should use the class's public API rather than attempting to reuse this Regex directly.