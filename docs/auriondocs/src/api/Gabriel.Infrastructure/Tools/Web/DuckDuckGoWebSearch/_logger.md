Holds the ILogger<DuckDuckGoWebSearch> instance used by the DuckDuckGoWebSearch class to emit structured diagnostic, informational, and error logs. Reach for this field when instrumenting methods in DuckDuckGoWebSearch to record progress, failures, performance timings, or other runtime details; the instance is typically provided via dependency injection and assigned in the class constructor.

## Remarks
Using the generic ILogger<T> ties log entries to the DuckDuckGoWebSearch category so consumers can filter or route logs by this component. Keeping the field readonly ensures the logger reference is stable for the lifetime of the object and simplifies reasoning about where logs originate. The logger is suitable for production telemetry as well as for unit tests (where a test logger or mock can be injected).

## Example
```csharp
// inside a method of DuckDuckGoWebSearch
_logger.LogInformation("Starting DuckDuckGo search for {Query}", query);

try
{
    var results = await PerformSearchAsync(query);
    _logger.LogDebug("Search returned {Count} results for {Query}", results.Count, query);
}
catch (Exception ex)
{
    _logger.LogError(ex, "DuckDuckGo search failed for {Query}", query);
    throw;
}
```

## Notes
- The field must be initialized (usually in the constructor) via dependency injection; it being readonly prevents reassignment.
- For expensive-to-format values, check _logger.IsEnabled(LogLevel.Debug) before computing them to avoid unnecessary work.
- Avoid logging sensitive or personally identifiable information; prefer structured templates over string concatenation for better filtering and analysis.