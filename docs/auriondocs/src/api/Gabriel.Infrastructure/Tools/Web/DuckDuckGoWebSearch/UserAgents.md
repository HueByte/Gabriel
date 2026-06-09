A small static read-only pool of real-browser User-Agent strings used by the DuckDuckGo web search implementation to present realistic, human-like UA values. The pool is intentionally limited and not rotated per request — a single UA should be chosen at session warmup and held for the session to avoid rapid UA flipping, which is itself a bot signal.

## Remarks
This field exists to reduce obvious bot fingerprints while keeping behavior plausible compared to real browsers. Instead of generating or changing the User-Agent on every request (which can trigger anomaly detectors), the code selects one entry from this pool when a session starts and reuses it until the session is reset by anomaly detection or other session lifecycle events. The pool also spreads UA fingerprints across different deployments and restarts so no single instance always presents the same UA.

## Example
```csharp
// Pick a User-Agent once at session start and reuse it for the session
var rnd = new Random();
string sessionUserAgent = UserAgents[rnd.Next(UserAgents.Length)];
// attach sessionUserAgent to outgoing HTTP requests for the life of the session
```

## Notes
- The array is declared static readonly so the reference cannot be reassigned; however, if mutated (not expected here) the strings themselves are immutable.
- Browser strings become stale over time; update the pool periodically to keep UAs current.
- Do not rotate the User-Agent per request — the pool is designed to avoid that anti-pattern because rapid UA changes are a bot signal.
