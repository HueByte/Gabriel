Creates a new DuckDuckGoWebSearch instance with the services it needs to perform HTTP requests and log activity. Use this constructor when the class is resolved by a dependency injection container or when manually creating an instance with an IHttpClientFactory and an ILogger<DuckDuckGoWebSearch>.

## Remarks
This constructor captures two collaborators: an IHttpClientFactory (used to obtain HttpClient instances for making web requests to DuckDuckGo) and an ILogger<T> (used for logging diagnostics and errors). It is intended to be satisfied by a DI container so the factory and logger are supplied by the application's logging/HTTP client infrastructure.

## Example
```csharp
// Registering in ASP.NET Core DI (Program.cs / Startup.cs)
services.AddHttpClient(); // ensures IHttpClientFactory is registered
services.AddTransient<DuckDuckGoWebSearch>();

// Consuming via constructor injection
public class MyService
{
    private readonly DuckDuckGoWebSearch _webSearch;

    public MyService(DuckDuckGoWebSearch webSearch)
    {
        _webSearch = webSearch;
    }
}

// Manual construction (e.g. in tests) - supply test doubles or real implementations
var httpFactory = /* an IHttpClientFactory instance or mock */;
var logger = /* an ILogger<DuckDuckGoWebSearch> instance or mock */;
var duckSearch = new DuckDuckGoWebSearch(httpFactory, logger);
```

## Notes
- The constructor does not validate arguments; passing null for either parameter will likely cause a NullReferenceException when the instance is used.
- Prefer resolving this type from the application's DI container so IHttpClientFactory and ILogger are provided correctly and benefit from shared HttpClient lifetimes and configured logging.
