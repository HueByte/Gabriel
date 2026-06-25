# LiteSnippetRegex

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private static readonly Regex LiteSnippetRegex = new(
        @"<td[^>]*class='result-snippet'[^>]*>(?<text>.*?)</td>",
        RegexOptions.Singleline | RegexOptions.Compiled)
```


LiteSnippetRegex is a private, static readonly Regex used to extract the textual content of a DuckDuckGo search result snippet from the HTML response. The pattern locates a table cell (<td>) whose class attribute equals 'result-snippet' and captures the inner content into a named group called text. The field is initialized once at type load time and is created with Singleline and Compiled options, enabling efficient, repeated parsing of multiple results without re-compiling the pattern.

## Remarks
This field serves as a focused, internal helper that isolates HTML-snippet extraction from higher-level parsing logic. Centralizing the pattern makes the codebase easier to maintain and test, since changes to the HTML structure or the capture semantics are localized to one place. The static readonly initialization ensures the Regex is compiled and cached, reducing runtime overhead when processing many results.

## Notes
- The pattern matches class='result-snippet' using single quotes; HTML that uses double quotes or additional class tokens may not be matched, making the regex brittle to variations in markup.
- The captured group named "text" contains the inner content of the TD and may include nested HTML; downstream code may need to sanitize or strip tags depending on how the snippet is used.
- As a private member, this Regex is not accessible from outside the containing type; consumer code should rely on the public parsing pathways that consume its results rather than accessing it directly.