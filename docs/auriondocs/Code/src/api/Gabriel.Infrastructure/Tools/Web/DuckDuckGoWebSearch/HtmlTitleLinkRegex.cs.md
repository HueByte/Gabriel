Matches a DuckDuckGo search-result anchor and captures its URL and displayed text. Reach for this when you need a lightweight, fast way to extract result links from raw DuckDuckGo HTML markup (rather than using a full DOM parser).

## Remarks
The regular expression looks specifically for <a> elements that include the CSS class "result__a" and uses two named capture groups: "href" for the link target and "text" for the link body. It is constructed with RegexOptions.Singleline so the dot (.) can match newlines (useful when the link text contains line breaks) and RegexOptions.Compiled for reuse and performance. Because the field is private, static, and readonly it is intended as a shared, thread-safe instance used across parsing routines in this class.

## Example
```csharp
var match = HtmlTitleLinkRegex.Match(htmlFragment);
if (match.Success)
{
    // raw captures
    var rawHref = match.Groups["href"].Value;
    var rawText = match.Groups["text"].Value;

    // common post-processing: decode HTML entities and (optionally) strip inner tags
    var href = System.Net.WebUtility.HtmlDecode(rawHref);
    var text = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(rawText, "<.*?>", string.Empty));

    // use href and text
}
```

## Notes
- The regex is fragile to changes in DuckDuckGo's markup; prefer an HTML parser for robust extraction.
- Captured link text may include inner HTML (emphasis tags, spans) and HTML entities — the regex does not unescape or strip those automatically.
- The pattern expects attributes quoted with double quotes and a class token containing "result__a"; anchors using single quotes or different class ordering may not match.