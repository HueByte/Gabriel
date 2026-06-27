A private, readonly ILogger<T> instance used by DuckDuckGoWebSearch to emit structured diagnostic and operational logs (information, warnings, errors, etc.). The logger is intended for use inside this class to record search activity, errors, and other runtime details and is typically provided via constructor injection by the application's dependency injection container.

## Remarks
Using the generic ILogger<DuckDuckGoWebSearch> categorizes log entries under this class' name so downstream logging providers can filter or enrich messages by source. The field is readonly to ensure the logger reference is immutable after construction; this keeps logging usage thread-safe with respect to the reference itself and avoids accidental reassignment.

## Example
```csharp
// constructor injection
public DuckDuckGoWebSearch(ILogger<DuckDuckGoWebSearch> logger /*, ... */)
{
    _logger = logger;
}

// usage inside methods
_logger.LogInformation("Searching DuckDuckGo for {Query}", query);
_logger.LogError(ex, "Search failed for query {Query}", query);
```

## Notes
- The field is private and only accessible within this class; prefer passing contextual information (IDs, queries) as structured properties rather than composing messages manually.
- Being readonly, the logger must be assigned during construction; ensure your constructor validates or relies on DI to provide a non-null ILogger<DuckDuckGoWebSearch>.
