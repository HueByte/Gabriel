# InfisicalOptions

> **File:** `src/api/Gabriel.Core/Configuration/InfisicalOptions.cs`  
> **Kind:** class

```csharp
public class InfisicalOptions : IConfigSection<InfisicalOptions>
```


InfisicalOptions encapsulates the configuration required to connect to Infisical secret management and exposes the endpoints, credentials, and runtime options via a configuration section. The IsConfigured property gates initialization, returning true only when Host, ProjectId, ClientId, and ClientSecret are present; if any essential value is missing, the provider gracefully skips, allowing local development without Infisical access, while ClientSecret should be sourced from secure stores (e.g., user secrets or INFISICAL__CLIENTSECRET) rather than appsettings.json.

## Remarks
InfisicalOptions acts as a single source of truth for Infisical configuration, separating non-sensitive fields from the sensitive ClientSecret. It enables safe defaults for development and a clear separation of concerns between configuration sources (appsettings.json, environment variables, and user secrets).

## Example
```csharp
// Typical usage with Microsoft.Extensions.Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var options = new InfisicalOptions();
config.GetSection("Infisical").Bind(options);

if (options.IsConfigured)
{
    // Initialize Infisical client with the provided options
}
```

## Notes
- Store ClientSecret securely; avoid storing it in appsettings.json. Prefer user secrets or environment variables (INFISICAL__CLIENTSECRET).
- IsConfigured only checks that the essential values are present; it does not verify runtime connectivity or permissions.
- Environment defaults to "dev" and can be overridden per environment; ensure consistency across deployments.