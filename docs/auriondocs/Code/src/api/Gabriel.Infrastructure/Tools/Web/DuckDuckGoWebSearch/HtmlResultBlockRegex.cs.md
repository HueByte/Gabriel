HtmlResultBlockRegex is a private, static readonly Regex used to locate and capture the inner HTML of each search result block in the HTML returned by the DuckDuckGo web search endpoint. It matches a <div> element whose class attribute contains the word result and exposes the block content via the named capture group body, enabling the parser to enumerate titles, snippets, and URLs without re-parsing the full HTML.

## Remarks
HtmlResultBlockRegex centralizes the HTML parsing logic for DuckDuckGo results. By keeping the pattern in one private field, changes to the server-side markup require updating only this location. The use of a compiled, singleline regex improves startup cost and per-match performance, and the named group body provides a stable handle for downstream extraction; if the server HTML changes such that no results can be matched, the caller will observe zero results and fall back to lite.

## Notes
- Changes to the HTML structure (e.g., the removal or renaming of the result container) may cause this regex to fail to match any results.
- Because HtmlResultBlockRegex is private, it cannot be reused directly from other classes; if reuse is needed, expose a proper API or encapsulate parsing logic accordingly.