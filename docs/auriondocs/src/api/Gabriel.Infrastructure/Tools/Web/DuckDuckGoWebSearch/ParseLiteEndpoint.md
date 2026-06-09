Parses HTML returned by DuckDuckGo's "lite" search endpoint and extracts up to the requested number of results as WebSearchResult instances. It locates result links and corresponding snippet nodes using precompiled regexes, decodes HTML entities, cleans the extracted text, and unwraps redirect URLs before returning titles, urls, and snippets.

## Remarks
The lite endpoint emits results in a simple link/snippet document order where each link is typically paired with a snippet at the same index. This method relies on that ordering: link matches drive result creation and snippet matches are read by index when available. The implementation purposely trusts the link-derived title and URL as the authoritative values; when a snippet is missing or indexes drift the worst outcome is an incorrect snippet while the title and URL remain accurate. Redirect wrappers in result hrefs are removed via UnwrapRedirect so callers receive the final target URL.

## Notes
- If a matched link yields an empty or whitespace-only title or URL, that entry is skipped and does not count toward the returned limit.
- Snippet indexing can drift if the lite HTML omits snippets for some links; in that case a snippet may belong to a different result while title and url remain correct.
- The returned list contains at most `limit` items but may be smaller depending on how many valid links are found.