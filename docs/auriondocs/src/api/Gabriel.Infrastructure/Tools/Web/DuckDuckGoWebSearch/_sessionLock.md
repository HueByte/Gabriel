A private, async-capable synchronization primitive used to serialize the "first-use" homepage pre-warm that populates the HttpClientHandler's CookieContainer for the current handler generation. Reach for this when multiple callers may concurrently trigger the pre-warm and you need to ensure only one pre-warm runs at a time without blocking threads (use WaitAsync/Release rather than a blocking lock).

## Remarks
This field deduplicates concurrent first-use callers so the homepage pre-warm runs at most once in parallel for the handler generation tracked by this instance. It complements the CookieContainer stored on the HttpClientHandler by protecting the one-time population step; using SemaphoreSlim (initial/max 1) provides a mutex-like, awaitable gate suitable for async code paths.

## Example
```csharp
await _sessionLock.WaitAsync();
try
{
    // perform the homepage pre-warm or check-if-already-done logic
}
finally
{
    _sessionLock.Release();
}
```

## Notes
- Always pair WaitAsync with Release in a finally block to avoid deadlocks or permanently held locks.
- Prefer WaitAsync in asynchronous code paths; using blocking Wait() can cause thread-pool starvation or deadlocks.
- SemaphoreSlim is constructed with initial and max count of 1, so it acts as an awaitable mutex. If the containing type implements IDisposable, consider disposing the SemaphoreSlim when appropriate.