A precompiled, single-line Regex that matches anchor (<a>) elements whose class attribute contains the token "result__snippet" and captures the anchor's inner content in a named group "text". Use this when you need a fast, simple extraction of snippet HTML from raw DuckDuckGo-like search result markup; for robust HTML manipulation prefer an HTML parser.

## Remarks
This field targets the specific markup pattern used for result snippets (an <a> element with a class containing "result__snippet"). It is created with RegexOptions.Singleline so the dot (.) matches newlines and RegexOptions.Compiled to improve match performance at runtime. Because it is a static readonly Regex, it is immutable and safe for concurrent use across threads.

## Example
```csharp
// html is the raw HTML string containing DuckDuckGo-like search results
var matches = HtmlSnippetRegex.Matches(html);
foreach (Match m in matches)
{
    // The named capture "text" contains the inner HTML of the matched <a> element
    var innerHtml = m.Groups["text"].Value;
    // Optionally decode HTML entities or strip tags if you need plain text
    // var plainText = System.Net.WebUtility.HtmlDecode(StripTags(innerHtml));
}
```

## Notes
- The regex extracts inner HTML, not sanitized plain text — it may include nested tags and HTML entities.
- Using a regular expression to parse HTML is brittle: changes in upstream markup (different attribute order, additional whitespace, or renamed classes) can break matches. Consider an HTML parser for long-term robustness.
- RegexOptions.Compiled improves match speed but increases startup/compile cost and memory usage for the generated assembly code.