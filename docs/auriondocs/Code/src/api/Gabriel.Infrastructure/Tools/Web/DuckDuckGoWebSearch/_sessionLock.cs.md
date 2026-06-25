This private readonly SemaphoreSlim coordinates the per-handler, one-time homepage pre-warm by serializing access to the initialization that populates the HttpClientHandler's CookieContainer and records the User-Agent for the current session. It ensures only a single thread performs the pre-warm for the current handler generation, avoiding duplicate work when multiple callers trigger startup simultaneously.

## Remarks
Because the CookieContainer is stored on the HttpClientHandler, this semaphore acts as a per-handler synchronization primitive for the startup sequence that depends on that state. It isolates the initialization concerns from other components and prevents race conditions during the first-use path by deduplicating concurrent first-time callers.

## Example
```csharp
// Example usage: ensure only one thread performs the homepage pre-warm per handler
await _sessionLock.WaitAsync();
try
{
    if (!homepagePrewarmed)
    {
        // perform pre-warm: populate CookieContainer and register User-Agent
        homepagePrewarmed = true;
    }
}
finally
{
    _sessionLock.Release();
}
```

## Notes
- Always Release the semaphore in a finally block to avoid potential deadlocks if an exception occurs.
- Avoid holding the lock for long-running or IO-bound work to minimize contention with other threads.
- This field is per HttpClientHandler; it does not synchronize across different handler instances or generations.