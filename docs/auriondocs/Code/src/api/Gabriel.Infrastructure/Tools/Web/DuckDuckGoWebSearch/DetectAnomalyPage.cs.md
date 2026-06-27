Detects whether an HTML string appears to be a bot‑block or CAPTCHA‑style interstitial served by DuckDuckGo or Cloudflare by checking for known marker substrings. Use this before treating an HTML response as a normal (possibly empty) search result so the caller can surface a clear diagnostic or attempt an alternative fetch strategy.

## Remarks
This is a lightweight, substring‑based heuristic that centralizes detection of a few well‑known anomaly markers (e.g. Cloudflare's "Just a moment" interstitial and DDG/Cloudflare token names). It intentionally avoids HTML parsing for performance and simplicity; the goal is to distinguish blocked/interstitial pages from genuine search results so calling code can fall back or report a specific error instead of silently returning no results.

## Notes
- The method does not guard against a null input; calling with null will result in a NullReferenceException. Callers should ensure the HTML string is non‑null before invoking.
- Checks are case‑sensitive (StringComparison.Ordinal). This favors performance and determinism but means differently cased markers may not be detected.
- This is a heuristic and not exhaustive—providers can change their interstitial content or token names. Update the marker list if new anomaly pages are observed, and be aware of possible false positives if a legitimate page contains any of the checked substrings.