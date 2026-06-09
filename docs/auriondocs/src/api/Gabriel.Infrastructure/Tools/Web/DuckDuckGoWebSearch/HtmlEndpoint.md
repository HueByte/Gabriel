Absolute URL for DuckDuckGo's primary rich HTML search endpoint (https://html.duckduckgo.com/html/). Use this constant when constructing requests that must unambiguously target the HTML subdomain regardless of any HttpClient BaseAddress configuration.

## Remarks
DuckDuckGo exposes two distinct search layouts on different hosts (a rich HTML endpoint and a minimal "lite" endpoint). This constant intentionally uses an absolute URL so requests always go to the correct subdomain; relying on a relative path or an HttpClient BaseAddress could silently route traffic to the wrong host and break the lite/ fallback behavior.

## Notes
- The value includes a trailing slash — when concatenating paths be careful to avoid duplicate or missing slashes.
- Because the constant is an absolute URL, HttpClient's BaseAddress (if set) will be ignored for requests that use this value directly.
- Do not change this to a relative path or point both endpoints at the same host; prior code that did so caused the lite fallback to stop working.