Resets the internal session indicators so the next search operation rebuilds a fresh session.

Call this when the DuckDuckGo endpoint returns an anomaly page or any response that indicates the current session/identity should be discarded. By clearing the "warmed" flag and the chosen user-agent, the next SearchAsync path will select a new UA and perform the homepage round-trip that refreshes cookies and adjusts the request fingerprint.

## Remarks
This method intentionally performs a minimal reset: it flips the _sessionWarmed flag to false and nulls out _sessionUserAgent so session re-creation logic runs naturally on the next search attempt. It does not directly clear cookie storage or fully reinitialize every piece of session state; instead it relies on the existing session-building code to perform the appropriate setup (new UA selection and homepage visit) when invoked.

## Example
```csharp
// inside the class that detects an anomaly response from DDG
if (response.IsAnomalyPage)
{
    // Force the next SearchAsync to pick a new UA and refresh cookies
    ResetSession();
}
```

## Notes
- ResetSession is private and intended for internal use; callers should invoke it only when they want a full session rebuild on the next search.
- The method is not synchronized; if the surrounding class is used concurrently, callers must ensure appropriate synchronization to avoid race conditions when reading/updating session-related fields.
- This method does not throw and performs no network I/O — the actual session re-creation happens later when SearchAsync runs.