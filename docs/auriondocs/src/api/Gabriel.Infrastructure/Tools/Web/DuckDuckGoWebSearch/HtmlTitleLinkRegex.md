Matches anchor elements used by DuckDuckGo search results and captures the link URL and the link's inner content. Specifically targets <a> elements whose class attribute contains the token result__a and exposes two named groups: "href" for the href attribute value and "text" for the anchor inner HTML/text. Use this when scraping DuckDuckGo result pages and you need the result link and its displayed text.

## Remarks
This compiled, single-line regex is optimized for repeated, concurrent matching across raw HTML returned from DuckDuckGo result pages. It uses named capture groups to make downstream code clearer and avoids reparsing the pattern for each match (RegexOptions.Compiled). Singleline is used so the dot (.) can match newlines inside the anchor's inner content.

## Example
```csharp
// html is the HTML fragment or page returned from DuckDuckGo
var match = HtmlTitleLinkRegex.Match(html);
if (match.Success)
{
    var url = match.Groups["href"].Value;    // captured href attribute
    var text = match.Groups["text"].Value;   // captured inner HTML/text of the <a>
}
```

## Notes
- The pattern requires the class attribute (containing the token `result__a`) to appear before the href attribute in the same <a> start tag; if attributes are ordered differently the anchor will not be matched.
- Only double-quoted attributes are supported; single-quoted attributes will not be captured.
- Parsing HTML with regular expressions is inherently brittle: nested tags inside the anchor, unexpected attribute formatting, or minor markup changes from DuckDuckGo may break matches. Prefer a real HTML parser when robustness is required.
- The compiled Regex is safe for concurrent use and improves performance for repeated matches, but compilation increases startup cost.