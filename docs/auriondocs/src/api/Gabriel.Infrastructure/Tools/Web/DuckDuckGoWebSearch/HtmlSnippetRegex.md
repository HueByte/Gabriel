A precompiled regular expression that locates anchor (<a>) elements whose class attribute contains the token "result__snippet" and captures the anchor's inner content in a named group called "text". Use this when you need to quickly extract the snippet HTML/text from DuckDuckGo search result markup without constructing a DOM parser.

## Remarks
The regex is created with RegexOptions.Singleline so the dot (.) also matches new lines, allowing the inner content to span lines, and RegexOptions.Compiled to improve matching performance for repeated use. Declared static readonly so the compiled regex is reused across calls and is safe for concurrent matches.

## Example
```csharp
string html = /* HTML source containing search results */;
var match = HtmlSnippetRegex.Match(html);
if (match.Success)
{
    string snippetHtml = match.Groups["text"].Value;
    // snippetHtml contains the inner HTML/text of the <a class="...result__snippet...">...</a>
}
```

## Notes
- Regular expressions are brittle for parsing HTML: the capture may include nested tags or attributes and will not normalize/strip HTML by itself.
- The captured text is raw HTML content (and may contain entities); decode or sanitize as needed before display or storage.
- RegexOptions.Compiled improves runtime performance for repeated matches but increases startup/compile cost and generated code size.
- The pattern expects the class attribute and the closing </a> in the same logical anchor element; malformed or minified HTML variants may not match as intended.