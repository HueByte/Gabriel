# InfisicalOptions

> **File:** `src/api/Gabriel.Core/Configuration/InfisicalOptions.cs`  
> **Kind:** class

```csharp
public class InfisicalOptions : IConfigSection<InfisicalOptions>
```


InfisicalOptions represents the configuration section for Infisical integration. It groups the host, project, environment, secret path, and credentials needed to initialize the Infisical provider, and exposes a guard (IsConfigured) to indicate readiness.

## Remarks
InfisicalOptions acts as a centralized holder that binds application configuration to the Infisical client. It separates non-sensitive defaults (ClientId lives in appsettings.json) from sensitive data (ClientSecret) that should come from secure channels like user-secrets or environment variables. The SectionName Infisical is the configuration key used by the hosting framework to bind values from configuration sources. This abstraction allows the app to boot cleanly in environments without Infisical access by omitting any of the four essentials.

## Example
```csharp
var options = new InfisicalOptions
{
    Host = "https://infisical.example",
    ProjectId = "proj-123",
    Environment = "dev",
    SecretPath = "/",
    ClientId = "client-xyz",
    ClientSecret = "super-secret"
};

if (options.IsConfigured)
{
    // Initialize and use the Infisical provider with options
}
```

## Notes
- The provider is activated only when IsConfigured is true (Host, ProjectId, ClientId, and ClientSecret must all be non-empty). If any are missing, Infisical integration is skipped to keep local development unblocked.
- ClientSecret is sensitive; avoid storing it in source control. Use user-secrets or environment variables (e.g. INFISICAL__CLIENTSECRET).
- Defaults: Environment defaults to "dev"; SecretPath defaults to "/"; TimeoutSeconds defaults to 15.
