Clears the in-memory session state so the next search rebuilds the session with a freshly chosen user-agent and performs the homepage round-trip. Call this when you need to recover from a DuckDuckGo anomaly page, force a UA rotation, or ensure the subsequent SearchAsync refreshes cookies and shifts the fingerprint.

## Remarks
This method only flips internal flags — it does not perform network activity or reconstruct the session immediately. The actual reinitialization (homepage round-trip, cookie refresh, and UA selection) happens inside SearchAsync when it observes the session is not warmed. ResetSession centralizes the intent to drop the warmed state so callers don't need to duplicate session-rebuild logic.

## Example
```csharp
// inside the DuckDuckGoWebSearch class after detecting an anomaly page
if (detectedAnomalyFromResponse)
{
    ResetSession(); // next SearchAsync will pick a new UA and refresh cookies
    // optionally retry the operation
    // await SearchAsync(query);
}
```

## Notes
- ResetSession does not clear the cookie jar or other persisted session resources; cookie refresh only occurs when SearchAsync performs the homepage round-trip.
- The method performs no network I/O and returns immediately — its effect is observed on the next search attempt.
- If the enclosing class is used concurrently, callers should synchronize access: clearing the warmed flag and user-agent is not protected by explicit locking in this method.