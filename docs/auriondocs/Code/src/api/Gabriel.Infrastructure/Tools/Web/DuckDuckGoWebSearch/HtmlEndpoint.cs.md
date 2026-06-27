A single-source constant containing the absolute URL for DuckDuckGo's rich HTML search endpoint (https://html.duckduckgo.com/html/). Use this when the DuckDuckGo web search tool needs to send queries to the primary HTML-rendered search host; the absolute URL ensures requests hit the intended subdomain regardless of any HttpClient.BaseAddress that may be configured.

## Remarks
This project targets two distinct DuckDuckGo search hosts: the rich HTML endpoint (html.duckduckgo.com) and a minimal "lite" endpoint (lite.duckduckgo.com). The constant is an absolute URL on purpose so that outgoing requests are routed to the correct host even if a named HttpClient has a different BaseAddress. Historically, pointing both endpoints at the same host caused the lite fallback to break because html.duckduckgo.com does not provide the lite layout.

## Notes
- The value includes a trailing slash — callers expect to append relative query paths, so removing the slash can lead to malformed URLs.
- This field is private and constant; change the host only if DuckDuckGo's endpoints change, and verify both HTML and lite behaviours when updating.