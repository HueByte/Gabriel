# InfisicalExtensions

> **File:** `src/api/Gabriel.API/Configuration/InfisicalExtensions.cs`  
> **Kind:** class

```csharp
public static class InfisicalExtensions
```


InfisicalExtensions exposes a canonical, ASP.NET Core–style extension for wiring Infisical into a .NET application's configuration pipeline. The AddInfisical method follows the Options-pattern: callers supply an `Action<InfisicalOptions>` to configure the InfisicalOptions instance, after which an InfisicalConfigurationSource is registered with the IConfigurationBuilder. The extension returns the same builder to enable fluent, chainable configuration (similar to AddDbContext or AddSwaggerGen). This decouples Infisical setup from application startup logic and centralizes configuration in a single, testable place.

## Remarks
This extension encapsulates the Infisical configuration concerns behind a lightweight, reusable surface. By accepting a delegate to configure InfisicalOptions, it cleanly separates how options are provided from how the configuration source is consumed, helping tests substitute options and validating the integration at startup time.

## Example
```csharp
// Typical usage during application startup
var builder = new ConfigurationBuilder();
builder.AddInfisical(opts => {
    // Configure options here (properties depend on InfisicalOptions)
    // e.g. opts.ProjectId = "my-project"; opts.SecretsEndpoint = "https://...";
});
```

## Notes
- Calling AddInfisical multiple times registers multiple InfisicalConfigurationSource instances; the final configuration depends on the ordering of sources within IConfiguration.
- Ensure required properties on InfisicalOptions are set within the configure delegate to avoid runtime configuration issues.
- This pattern keeps Infisical-specific setup isolated from business logic, aligning with standard .NET startup conventions.