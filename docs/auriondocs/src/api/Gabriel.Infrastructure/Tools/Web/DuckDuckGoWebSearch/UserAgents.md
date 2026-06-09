A small static pool of realistic, modern browser User-Agent strings intended to be selected once per session (not per request) when the DuckDuckGo web search tooling makes HTTP requests. The pool exists to present a plausible, consistent UA for the lifetime of a session while spreading fingerprinting across different deployments and restarts.

## Remarks
This field exists to avoid per-request UA rotation (which is itself a bot signal) while still preventing every deployment from sharing the exact same UA. A single UA should be chosen at session warmup and held for the session; anomaly-detection or session-reset logic can pick a different UA later. The pool size is deliberately small — it's meant to reduce uniform fingerprints across instances, not to perfectly emulate a large variety of client environments.

## Example
```csharp
// pick one UA for the session and apply it to an HttpClient
var sessionUserAgent = UserAgents[Random.Shared.Next(UserAgents.Length)];
httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", sessionUserAgent);
```

## Notes
- The choice to avoid per-request rotation is intentional: rapid UA changes within a session can increase bot detection risk.
- The field is declared static readonly, so the array reference cannot be replaced; however, arrays are mutable in general — treat this pool as immutable and do not modify its elements at runtime.
- Keep the pool up-to-date: User-Agent strings should be refreshed occasionally so they remain current and realistic for target sites.