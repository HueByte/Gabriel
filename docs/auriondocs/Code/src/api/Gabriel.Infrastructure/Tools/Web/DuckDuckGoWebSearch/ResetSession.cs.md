Clears internal session state to trigger a fresh session initialisation. When an anomaly page is detected from DuckDuckGo, ResetSession drops the warmed flag and the selected User-Agent so the next SearchAsync rebuilds the session with a freshly-picked UA and executes another homepage round-trip. This flow refreshes cookies and subtly shifts the fingerprint for the next requests.

## Remarks
This internal helper centralizes session hygiene, ensuring the UA and warmed flag are reset in one place. It decouples session reinitialization from the higher-level request flow, making the behavior predictable when anomalies occur and the client must re-establish its identity.

## Notes
- Triggers an additional homepage round-trip during anomaly recovery, which can impact latency.
- Since it mutates private fields, ensure callers invoke it in a context where no concurrent session initializations are in progress.