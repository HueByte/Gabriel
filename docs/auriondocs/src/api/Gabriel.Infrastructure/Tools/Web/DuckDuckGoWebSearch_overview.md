Implements a no‑API‑key IWebSearch backed by DuckDuckGo's public HTML endpoints. It prefers the richer html/ endpoint and falls back to the lite/ endpoint when parsing yields no results; sessions are pre‑warmed and a single realistic User‑Agent is selected per session to reduce bot‑like behavior. Use this when you need a free, out‑of‑the‑box web search provider (for prototypes, tests, or low-volume use) rather than a paid search API.

## Remarks
This class encapsulates the practical tradeoffs of scraping DuckDuckGo instead of calling a paid API: it posts the same form payload to two distinct endpoints in order (html/ then lite/), uses a session warmup step to populate the CookieContainer, and holds a single browser-like User‑Agent string for the life of the session. Parsing is intentionally forgiving (regex-driven) and parse failures return no results rather than throwing. The implementation logs diagnostic detail when DDG serves anomaly or rate‑limit pages so callers can distinguish genuine "no results" from blocked requests.

## Notes
- Requires the named HttpClient configuration used by the project (see DependencyInjection.ConfigureDdgHttpClient) so the HttpClientHandler includes a CookieContainer; without that the session pre‑warm is ineffective.
- Parsing is HTML/regex based and can break if DuckDuckGo changes its markup; failures produce zero results instead of exceptions.
- The class chooses one realistic User‑Agent per session and does not rotate it per request — rotating UAs rapidly is a bot signal and intentionally avoided.
- DuckDuckGo may return CAPTCHAs or "anomaly" pages under aggressive traffic; the implementation detects and logs these cases but is not a substitute for a paid, rate‑guaranteed API for production scale.