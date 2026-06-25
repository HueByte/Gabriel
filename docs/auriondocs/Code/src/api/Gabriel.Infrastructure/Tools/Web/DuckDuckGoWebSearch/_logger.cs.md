Provides a strongly-typed, per-class logger for the DuckDuckGoWebSearch component. This private readonly field allows the class to emit diagnostic messages, tracing, and errors with a category tied to the DuckDuckGoWebSearch type, enabling easy filtration and correlation across logs. Typically injected via the application's DI container, the instance is assigned once in the constructor and then reused for the lifetime of the object to maintain consistent logging context.

## Remarks
ILogger<DuckDuckGoWebSearch> is a DI-provided logger; the generic type parameter yields a category named after the class, which helps filter logs per component and adds contextual structure to messages. This field is private and readonly to guarantee a stable logging context throughout the object's lifetime.

## Example
```csharp
_logger.LogInformation("Starting web search for query: {Query}", query);
```

## Notes
- Do not log sensitive user data (credentials, tokens, or personal information).
- Avoid excessive logging in performance-critical paths; prefer lower log levels or guard verbose logs with appropriate checks.
- If the class is renamed, the logger category changes accordingly; update tests or mocks that rely on the category name.