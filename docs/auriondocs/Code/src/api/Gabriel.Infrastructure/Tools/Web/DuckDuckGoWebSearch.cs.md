This constructor wires up the DuckDuckGoWebSearch service by accepting an IHttpClientFactory and an ILogger<DuckDuckGoWebSearch>, storing them for later use. It is designed for dependency injection and should be supplied by the DI container (or in unit tests) so that HttpClient instances can be created through the factory and all logging goes through the ILogger instance.

## Remarks
By using constructor injection, the class remains decoupled from concrete HTTP client implementations and logging infrastructure, which makes it easier to test with mocks and to swap dependencies without modifying the class.

## Notes
- No argument null checks are performed; passing null for httpFactory or logger will lead to NullReferenceException later when the dependencies are used.