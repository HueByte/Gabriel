A private readonly SemaphoreSlim used to serialize the first-use initialization of per-session state on the HttpClientHandler (CookieContainer population and the committed User-Agent). It ensures only one thread performs the initial pre-warm for the current handler generation, while others await completion, avoiding duplicate work and race conditions during startup.

## Remarks
This semaphore coordinates per-instance session initialization, preventing race conditions when warming the homepage and writing the User-Agent. It sits alongside the HttpClientHandler-based session state; by serializing the first-use path, it makes the initialization predictable and avoids redundant work across concurrent callers.

## Example
```csharp
// Typical usage pattern
await _sessionLock.WaitAsync();
try
{
    // assume some fields tracking initialization
    if (!CookiesPrewarmedForCurrentSession)
    {
        // perform pre-warm: populate CookieContainer, set User-Agent
        CookiesPrewarmedForCurrentSession = true;
        // set UserAgent etc.
    }
}
finally
{
    _sessionLock.Release();
}
```

## Notes
- Always release the semaphore in a finally block to avoid deadlocks.
- Avoid holding the lock during long-running I/O; keep the critical section as small as possible and perform expensive work outside the guarded region if feasible.
- The field is per-instance; do not rely on it across handler generations or across different HttpClientHandler instances.