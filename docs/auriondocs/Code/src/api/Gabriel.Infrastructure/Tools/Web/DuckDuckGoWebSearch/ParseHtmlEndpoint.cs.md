Parses a chunk of HTML produced by a search endpoint and converts it into a list of WebSearchResult objects, up to the specified limit. It walks the HTML in blocks identified by HtmlResultBlockRegex, extracts the body, finds the title/link via HtmlTitleLinkRegex, decodes the href, and resolves any redirects to obtain a stable URL. If a valid title and URL are obtained, it optionally extracts a snippet with HtmlSnippetRegex (defaulting to an empty string) and appends a WebSearchResult(title, url, snippet) to the results. The final list is returned when all blocks are processed or the limit is reached.

## Remarks
Isolates the HTML-to-domain object mapping behind a small, private method so higher-level search orchestration doesn't need to know about the exact HTML structure. It encapsulates common resilience steps (skip malformed blocks, decode HTML, drop empty fields) and centralizes URL normalization via UnwrapRedirect. The approach trades flexibility for stability in face of HTML changes in the endpoint.

## Notes
- Relies on specific named regex groups (e.g., body, href, text); if the HTML structure changes, the regex definitions will need updates. 
- Respects the limit by breaking early once enough results are collected. 
- Snippet is optional; if missing, the produced WebSearchResult uses an empty string for the snippet.