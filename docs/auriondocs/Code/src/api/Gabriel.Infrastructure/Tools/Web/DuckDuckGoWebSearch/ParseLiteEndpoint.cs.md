Parses the HTML produced by the lite DuckDuckGo endpoint to extract a list of WebSearchResult items up to the specified limit. It decodes HTML-encoded link text, unwraps potential redirects, and pairs each link with its corresponding snippet when available. This helper consolidates the lite-endpoint parsing logic and is intended for internal use by the web search feature to produce a stable, ordered collection of results.

## Remarks
This internal routine centralizes the parsing of the lite endpoint, providing a single, predictable extraction path for titles, URLs, and optional snippets. It preserves the order of results as they appear in the HTML and tolerates missing snippets by supplying an empty snippet string. By isolating this parsing, higher-level search code can rely on a consistent WebSearchResult shape without duplicating regex and decoding logic.

## Notes
- Requires non-negative limit; passing a negative value will throw ArgumentOutOfRangeException when initializing the list.
- Assumes the lite layout's index alignment between links and snippets; if the HTML structure changes, the association may drift.
- No null-checks on html are performed; passing null will result in an exception during regex matching.