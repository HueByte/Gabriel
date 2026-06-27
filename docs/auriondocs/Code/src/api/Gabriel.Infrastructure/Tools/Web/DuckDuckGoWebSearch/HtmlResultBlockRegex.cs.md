Matches a single DuckDuckGo HTML search-result block: a <div> whose class attribute contains the token "result" and everything inside it up to the next result div, the <div id="bottom_spacing"> sentinel, or the end of the document. Use this when extracting per-result inner HTML from the standard DuckDuckGo HTML response (the non-lite endpoint).

## Remarks
This compiled, single-line regex is intended as a stable extractor because the server-side markup for results is consistent; it captures the inner HTML of each result in a named "body" group. The code uses this as a pragmatic HTML-scraping fallback for the regular DuckDuckGo response and will drop back to zero results (so the caller can try the lite/ endpoint) if the page shape changes.

## Notes
- This is not a replacement for an HTML parser: the capture contains raw inner HTML (the "body" group), so callers should parse or sanitize it before extracting text or attributes.
- RegexOptions.Singleline is used so '.' matches newlines; the pattern relies on a lookahead to stop at the next result div, <div id="bottom_spacing">, or the document end.
- The Regex is compiled for performance at startup, but applying it repeatedly to very large responses still has cost — prefer enumerating Matches(html) once and reusing the results.