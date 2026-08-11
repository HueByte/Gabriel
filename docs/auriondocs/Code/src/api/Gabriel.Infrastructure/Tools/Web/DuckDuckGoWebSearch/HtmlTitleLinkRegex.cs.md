This private static readonly Regex consolidates the HTML pattern used to extract DuckDuckGo search result title links from the HTML returned by the search page. The pattern matches an anchor tag whose class contains result__a and captures two pieces of data: the destination URL from the href attribute (named group href) and the visible link text (named group text). The Regex is constructed with Singleline and Compiled options to perform a fast, shared parsing operation across the class.

## Remarks
Centralizes the knowledge of DuckDuckGo's result link markup in one place, enabling consistent extraction without duplicating the regex across methods. Because the HTML structure of search results can change, this implementation should be accompanied by tests and simple fallback handling if the pattern no longer matches.

## Notes
- This regex assumes the presence of an href attribute and a non-empty link text; if the HTML deviates, matches may fail.
- RegexOptions.Singleline makes the dot match newlines; ensure that HTML with line breaks doesn't break the pattern.
- The static readonly instance is thread-safe for concurrent use after initialization and should not be replaced at runtime.