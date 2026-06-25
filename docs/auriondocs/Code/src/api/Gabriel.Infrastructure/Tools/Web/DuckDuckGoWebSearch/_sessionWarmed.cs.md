# _sessionWarmed

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** field

```csharp
private bool _sessionWarmed
```


A private boolean flag that indicates whether the DuckDuckGo web search session has been warmed up. It is used to guard one-time initialization logic so that the expensive warm-up is performed only once per instance, allowing subsequent operations to proceed under the assumption that the session is ready.

## Remarks
This flag encapsulates the component's lifecycle state, isolating initialization concerns from normal operation. It helps avoid repeated initialization costs and keeps warm-up concerns localized to the class. If the class instance is long-lived and accessed concurrently, consider synchronization to avoid race conditions around first-time warm-up.

## Example
```csharp
if (!_sessionWarmed)
{
    // perform one-time session warm-up
    WarmUpSession();
    _sessionWarmed = true;
}
```

## Notes
- If multiple threads can reach the warm-up check concurrently, you may need synchronization to ensure the warm-up runs exactly once.
- Do not rely on this internal flag from outside the class; it represents internal state only and is not part of the public surface area.