A SemaphoreSlim instance used to serialize the one-time "pre-warm" of session state (the handler's CookieContainer and the chosen User-Agent) so that multiple concurrent callers do not perform the pre-warm work in parallel. It starts unlocked (initial count 1) and is held only for the duration of the first-use work.

## Remarks
This lock prevents duplicate work and races during the initial session setup for the underlying HttpClientHandler. The field is private and readonly because the lock instance is created once for the lifetime of the enclosing object and reused for all pre-warm attempts; callers should acquire it asynchronously to avoid blocking thread-pool threads.

## Example
```csharp
await _sessionLock.WaitAsync();
try
{
    // perform first-use pre-warm (populate CookieContainer, record User-Agent, etc.)
}
finally
{
    _sessionLock.Release();
}
```

## Notes
- SemaphoreSlim implements IDisposable; if the enclosing type has a shorter-than-process lifetime, ensure the instance is disposed when no longer needed.
- Prefer WaitAsync() (as shown) rather than synchronous Wait()/WaitOne() to avoid thread-pool starvation or deadlocks in async contexts; always release in a finally block.