Private static readonly string[] UserAgents is a small, hard-coded pool of real-browser User-Agent strings used by the DuckDuckGo web search integration. Rather than rotating the User-Agent on every request, a single UA is selected during session warmup and kept for the entire session to mirror real browser behavior and to avoid signaling bots with rapid UA changes. The strings cover representative Chrome on Windows, Firefox on Windows, Edge on Windows, and Chrome on macOS, enabling a realistic fingerprint across common platforms. The pool's purpose is to spread fingerprints across deployments or restarts rather than per-request rotation, helping balance realism with stability.

## Remarks

This field participates in the session-scoped fingerprinting strategy for the DuckDuckGo web search component. By selecting a single, realistic User-Agent at session warmup and reusing it for the duration of the session, it reduces the risk of bot-detection signals that can arise from frequent UA changes, while still providing platform diversity across deployments. It complements the HTTP header construction logic that emits the User-Agent header for outgoing requests.

## Notes

- Do not mutate the contents of the array at runtime
```
UserAgents[0] = "Some other UA string";
```
- The UA strings may become outdated; update them cautiously to reflect current real-browser identifiers
```
// Update only with well-tested, current UA strings
```
- If per-request UA rotation is required, do not rely on this field alone; implement a separate mechanism for per-request variability
```
// Example: implement a per-request UA provider instead of rotating this static pool
```