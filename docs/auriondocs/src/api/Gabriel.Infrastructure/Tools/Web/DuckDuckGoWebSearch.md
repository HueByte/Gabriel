Initializes a new DuckDuckGoWebSearch instance by capturing the IHttpClientFactory used to create HttpClient instances for web queries and an ILogger used to record operational information. Call this constructor when creating the class manually; in typical applications it will be invoked automatically by the dependency-injection container.

## Remarks
This constructor exists purely to receive and store two runtime dependencies: an IHttpClientFactory so the implementation can obtain configured HttpClient instances for making requests to DuckDuckGo, and an ILogger<DuckDuckGoWebSearch> for structured logging. It keeps the class testable and DI-friendly by avoiding hard-coded creation of HTTP clients or loggers.

## Example
```csharp
// Letting the DI container construct it (recommended)
services.AddTransient<DuckDuckGoWebSearch>();

// Or manually constructing (e.g., in a unit test)
var search = new DuckDuckGoWebSearch(httpFactory, logger);
```

## Notes
- The constructor does not perform null validation on its parameters; passing null values will set internal fields to null and likely cause a NullReferenceException when methods are invoked. Ensure dependencies are non-null (the DI container will normally satisfy this).