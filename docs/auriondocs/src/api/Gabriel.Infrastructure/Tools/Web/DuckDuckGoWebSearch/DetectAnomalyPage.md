Detects whether an HTML response looks like a bot-block / CAPTCHA / interstitial page (covers several DuckDuckGo and Cloudflare variants). Use this when you fetch search HTML and need to distinguish a blocked/interstitial response from a legitimate empty or error result so the caller can retry, switch endpoints, or surface a clear diagnostic.

## Remarks
This is a lightweight heuristic that checks for a small set of known marker strings inserted by DuckDuckGo and Cloudflare (e.g. anomaly markers, Cloudflare challenge tokens and interstitial text). The method exists so higher-level code can treat these responses differently than normal search results rather than silently returning no matches.

## Example
```csharp
var html = await httpClient.GetStringAsync(url);
if (DetectAnomalyPage(html))
{
    // handle bot-block: retry, use alternate endpoint, or surface a diagnostic
    throw new InvalidOperationException("Request blocked by interstitial / CAPTCHA page");
}
// proceed to parse search results
```

## Notes
- The checks use StringComparison.Ordinal (case-sensitive). If providers change casing or markup this may miss matches.
- This is a heuristic (string containment) not an HTML parse; it can produce false positives or miss novel interstitials.
- Keep the marker list updated if providers change their interstitial content or new mitigations appear.