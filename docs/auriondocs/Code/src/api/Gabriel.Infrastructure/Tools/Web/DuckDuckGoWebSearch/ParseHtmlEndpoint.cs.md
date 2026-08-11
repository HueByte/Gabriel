Parses the provided HTML to yield a collection of WebSearchResult objects up to the specified limit. It scans the HTML using HtmlResultBlockRegex to locate result blocks, extracts the body portion from each block, and then uses HtmlTitleLinkRegex to find the title link. The href is HTML-decoded, and the URL is obtained by UnwrapRedirect; the title text is cleaned with CleanText. If either the title or URL is empty, that block is ignored. If a snippet is present within the body, it is cleaned and stored; otherwise, the snippet is an empty string. Each valid block is turned into a WebSearchResult(title, url, snippet) and added to the results, which are returned once all blocks are processed or the limit is reached.

## Remarks
By isolating HTML parsing into ParseHtmlEndpoint, this method provides a focused, testable step that translates raw web search page markup into the domain object WebSearchResult. It relies on a small set of regex-based extractors and a redirect unwrapper, enabling consistent handling of links and metadata across different blocks.

## Notes
- The loop stops processing once the requested limit is reached, potentially ignoring remaining blocks.
- Blocks without a valid title or URL are skipped, so not all matched blocks contribute results.
- The snippet is optional; if absent, the resulting WebSearchResult stores an empty snippet.