The _logger field is a strongly-typed logger for the DuckDuckGoWebSearch class. It enables emitting structured log messages that provide observability into DuckDuckGoWebSearch operations, such as initiating requests or handling responses; the field being private and readonly signals that logging is an intrinsic, unchanging part of the class's behavior, typically wired in via constructor dependency injection.

## Remarks
ILogger<T> usage ties logs to the concrete class, which helps filter and correlate messages in diagnostics tools. By using dependency injection and a private readonly field, the class can emit meaningful diagnostics without introducing tight coupling to a concrete logging implementation. This abstraction supports testability, as a test can substitute a mock or in-memory logger to verify log emissions. It also encourages consistent logging practices across the codebase when used as a pattern for similar components.

## Example
```csharp
_logger.LogInformation("Fetching results from DuckDuckGo for query '{Query}'", query);
```

## Notes
- Do not log sensitive user data or credentials; apply redaction if needed.
- Keep log messages lightweight and avoid expensive computations in message formatting.
- Use appropriate log levels (Information, Warning, Error) to reflect the significance of events.
