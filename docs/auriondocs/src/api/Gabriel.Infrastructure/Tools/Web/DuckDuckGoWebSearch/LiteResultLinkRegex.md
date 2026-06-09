Matches anchor elements produced by DuckDuckGo's "lite" HTML endpoint and captures the link target and display text. Use this when parsing result pages fetched from the lite endpoint (which uses a flatter table structure and single-quoted class attributes) to extract the href (group "href") and the anchor inner HTML/text (group "text").

## Remarks
This regular expression is tailored for DuckDuckGo's lite response form: it expects anchors like <a href="..." class='result-link'>...</a>. It uses RegexOptions.Singleline so the dot matches newlines and RegexOptions.Compiled for performance when used repeatedly. This is a focused extractor, not a general-purpose HTML parser; it exists to efficiently find result links in a known, stable HTML shape returned by the lite endpoint.

## Example
```csharp
// html contains a fragment from DuckDuckGo's lite results page
var match = LiteResultLinkRegex.Match(html);
if (match.Success)
{
    var href = match.Groups["href"].Value;   // raw href attribute value
    var textHtml = match.Groups["text"].Value; // inner HTML/text of the anchor

    // decode any HTML entities in the text
    var decodedText = System.Net.WebUtility.HtmlDecode(textHtml);

    // href may still be a redirect URL; callers typically pass it through UnwrapRedirect
    Console.WriteLine($"Link: {href}\nText: {decodedText}");
}
```

## Notes
- The regex requires the class attribute to be single-quoted (class='result-link') and the href to be double-quoted; it will not match variants that differ from that exact pattern.
- Because this operates on HTML with a regex, it can break if DuckDuckGo changes the page structure or attribute quoting; treat it as fragile and narrow in scope.
- Captured groups are named "href" (the link target) and "text" (the anchor contents).
- The expression does not validate or normalize URLs and does not perform redirect-unwrapping itself; callers should handle those steps as needed.