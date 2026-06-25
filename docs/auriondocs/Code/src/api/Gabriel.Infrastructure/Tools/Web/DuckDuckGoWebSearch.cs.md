DuckDuckGoWebSearch's constructor initializes a new instance by capturing its required collaborators: an IHttpClientFactory to create HTTP clients for making web requests, and an ILogger<DuckDuckGoWebSearch> to emit structured logs. You typically rely on dependency injection to supply these abstractions, rather than instantiating concrete implementations yourself, so the constructor remains lightweight and testable.

## Remarks

The constructor acts as the IoC-friendly boundary between the class and its collaborators, decoupling it from concrete HTTP clients and log sinks. By depending on IHttpClientFactory and ILogger, the component can adapt to different environments—such as test doubles, custom HTTP handlers, or alternate logging backends—without code changes.

## Notes

- Passing nulls will likely cause runtime errors since the constructor assigns fields without guards; ensure the DI container or calling code provides non-null dependencies.
- Register and resolve DuckDuckGoWebSearch via your DI container so its required collaborators are supplied automatically.
- In tests, provide mocks or fakes for IHttpClientFactory and ILogger<DuckDuckGoWebSearch> to isolate behavior.