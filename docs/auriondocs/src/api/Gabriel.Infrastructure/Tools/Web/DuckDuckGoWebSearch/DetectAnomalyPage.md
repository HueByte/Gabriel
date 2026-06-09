Returns true when the provided HTML appears to be a bot-block / CAPTCHA / interstitial page emitted by DuckDuckGo or the Cloudflare layer in front of it. Use this to detect and handle service-side challenge pages (for example: retrying via a different endpoint, surfacing a specific error to the caller, or skipping parsing when results are meaningless).

## Remarks
This method is a lightweight heuristic that looks for several known markers (class names, script names, and Cloudflare-specific tokens) to identify anomaly/interstitial pages. It's intentionally implemented as substring checks rather than HTML parsing so callers can quickly determine if the response should be treated as a block instead of a legitimate (but empty) search result.

## Example
```csharp
var html = await httpClient.GetStringAsync(url);
if (DetectAnomalyPage(html))
{
    // Handle the challenge: log, surface a clear error, or try a fallback endpoint
}
else
{
    // Normal parsing of search results
}
```

## Notes
- The method does not guard against null: passing a null html will throw a NullReferenceException. Ensure the caller provides a non-null string.
- Checks are case-sensitive (StringComparison.Ordinal). Variants that differ only by case may not be detected.
- This is a heuristic and may produce false positives if the matched substrings appear in benign content; conversely it may miss future/unknown challenge pages.