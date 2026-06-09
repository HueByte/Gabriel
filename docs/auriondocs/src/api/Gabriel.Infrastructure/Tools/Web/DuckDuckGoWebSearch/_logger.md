A private, readonly logger instance typed to DuckDuckGoWebSearch that the class uses to emit diagnostic, informational, warning, and error messages. Reach for this field when adding any runtime logging inside DuckDuckGoWebSearch methods so entries are categorized under the class name and routed through the application's configured logging providers.

## Remarks
Using ILogger<DuckDuckGoWebSearch> gives log entries a category name matching the containing class and integrates with dependency injection and the application's logging configuration. The readonly modifier enforces that the logger is assigned once (typically in the constructor) and not replaced at runtime, keeping logging usage predictable and safe across the instance's lifetime.

## Example
```csharp
// Typical constructor injection and usage
public DuckDuckGoWebSearch(ILogger<DuckDuckGoWebSearch> logger /*, other deps */)
{
    _logger = logger;
}

public void SomeOperation()
{
    _logger.LogInformation("Starting web search for query: {Query}", query);
    // ...
}
```

## Notes
- The field is private: logging is intended for internal diagnostics only; do not expose this field publicly.
- The field relies on DI to provide a non-null ILogger; if the class is instantiated manually, ensure a logger is supplied or guard against null.
- Avoid extremely verbose logging in tight loops to prevent performance and noise issues in production environments.