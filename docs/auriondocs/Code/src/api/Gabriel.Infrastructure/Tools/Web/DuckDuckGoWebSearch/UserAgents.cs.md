A small, curated pool of recent, real-browser User-Agent header strings used by the DuckDuckGo web search implementation to present a realistic browser identity. The code selects a single entry from this array at session warmup and holds that value for the lifetime of the session (avoiding per-request rotation), which reduces bot-detection signals that come from rapid UA flipping.

## Remarks
This array exists to balance realism with stability: each process or session can pick one credible User-Agent to use for outbound requests, while the limited pool spreads fingerprints across different deployments and restarts. The design intentionally avoids rotating the User-Agent on every request because frequent changes inside a session are themselves a detection signal.

## Example
```csharp
// Pick a User-Agent at session warmup and store it for the session
var ua = UserAgents[Random.Shared.Next(UserAgents.Length)];
// Use `ua` for subsequent requests in this session
```

## Notes
- The field is declared readonly, but the array reference is what's readonly; in C# the array's elements could technically be replaced by code running in the same class. The strings themselves are immutable.
- The list is a small, curated sample — not an exhaustive set of browser UAs. Update it periodically if you need newer or different browser fingerprints.
- Do not rotate the User-Agent per request; follow the session-level selection strategy to avoid producing bot-like behavior.