# DuckDuckGoWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/DuckDuckGoWebSearch.cs`  
> **Kind:** constructor

```csharp
public DuckDuckGoWebSearch(IHttpClientFactory httpFactory, ILogger<DuckDuckGoWebSearch> logger)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `httpFactory` | `IHttpClientFactory` | — |
| `logger` | `ILogger<DuckDuckGoWebSearch>` | — |

**Returns:** `public`


Initializes a new DuckDuckGoWebSearch instance by capturing the IHttpClientFactory and ILogger<DuckDuckGoWebSearch> dependencies used for performing web queries and recording diagnostic events. Reach for this constructor when the type is being created (typically by a dependency injection container) so the implementation can obtain HttpClient instances and log operations.

## Remarks
This constructor exists to centralize external dependencies required by the search implementation: IHttpClientFactory provides a safe, reusable way to obtain HttpClient instances (helping to avoid socket exhaustion and to share configuration), while ILogger<T> enables structured, testable logging. The constructor is intentionally lightweight and side-effect free — it only stores the injected collaborators for use by the instance's search methods.

## Example
```csharp
// Registering with Microsoft.Extensions.DependencyInjection
services.AddHttpClient();
services.AddTransient<DuckDuckGoWebSearch>();

// Consumed via DI in another component
public class Consumer
{
    private readonly DuckDuckGoWebSearch _search;

    public Consumer(DuckDuckGoWebSearch search)
    {
        _search = search;
    }
}

// Manual construction (less common; ensure non-null arguments)
var logger = loggerFactory.CreateLogger<DuckDuckGoWebSearch>();
var search = new DuckDuckGoWebSearch(httpClientFactory, logger);
```

## Notes
- The constructor does not perform null checks on its parameters; when constructing manually, ensure you pass non-null IHttpClientFactory and ILogger<DuckDuckGoWebSearch> instances. The DI container will normally provide these.
- No I/O or network activity occurs during construction; any HTTP calls happen when search methods are invoked.