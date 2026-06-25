Determin es whether a given HTML response is an anomaly page produced by bot-blocking services (such as Cloudflare interstitials) by searching for a curated set of markers. Callers can use this to surface a diagnostic or fall back to a sibling endpoint instead of treating the page as a normal result.

## Remarks

This predicate centralizes the detection logic for anti-bot / blocking pages, consolidating several brittle string checks into a single, reusable gate. It relies on ordinal (case-sensitive) string comparisons, minimizing culture-related surprises but making the checks sensitive to exact page fragments; updates may be needed if providers change their markers.

## Notes

- Null input will cause an exception; ensure the html argument is non-null before invoking.
- Marker set may become outdated as anti-bot providers evolve—update the list of strings as needed.
- This approach uses simple substring checks and may yield false positives or negatives on obfuscated or minified content; a more robust parser could improve reliability.