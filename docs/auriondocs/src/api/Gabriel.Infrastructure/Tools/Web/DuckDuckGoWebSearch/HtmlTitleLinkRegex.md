A compiled regular expression used to locate the anchor element that represents a search-result title in DuckDuckGo HTML. The pattern looks for an <a> tag whose class attribute contains the token "result__a" and captures the link URL in the named group "href" and the anchor's inner HTML/text in the named group "text". Use this when parsing raw DuckDuckGo search result pages to extract the result link and displayed title.

## Remarks
This field centralizes the HTML-matching logic for DuckDuckGo result links so parsing code can reuse a single, optimized pattern. The regex is precompiled (RegexOptions.Compiled) for better runtime performance when used repeatedly, and Singleline is enabled so the inner-text capture can span newlines. The pattern assumes double-quoted attributes and uses a word boundary (\b) to avoid matching class names that merely contain the substring "result__a" as part of a longer token.

## Example
```csharp
// html contains a snippet of DuckDuckGo search results
var match = HtmlTitleLinkRegex.Match(html);
if (match.Success)
{
    string href = match.Groups["href"].Value;          // captured URL
    string rawText = match.Groups["text"].Value;       // captured inner HTML/text

    // decode HTML entities and strip tags if needed
    string title = System.Net.WebUtility.HtmlDecode(rawText);
    // title may still contain inner tags; consider HtmlAgilityPack or a sanitizer
}
```

## Notes
- The pattern only matches attributes quoted with double quotes; single-quoted attributes will not be recognized.
- Using a regex to parse HTML is brittle: changes in DuckDuckGo's markup, different attribute ordering, or additional nested tags inside the <a> can break matches.
- The captured "text" group may include inner HTML or entities; post-processing (HTML decode or stripping tags) is often required to obtain a plain title string.
- RegexOptions.Compiled improves performance at the cost of a slightly higher startup/time-to-JIT and additional memory for the generated assembly.
- RegexOptions.Singleline makes the dot (.) match newlines, which is important if the anchor's inner HTML contains line breaks.
