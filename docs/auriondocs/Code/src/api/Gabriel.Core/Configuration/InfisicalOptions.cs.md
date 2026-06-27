# InfisicalOptions

> **File:** `src/api/Gabriel.Core/Configuration/InfisicalOptions.cs`  
> **Kind:** class

A configuration POCO that holds options used to connect to an Infisical secrets provider. Use this type when binding application configuration (appsettings, environment variables, or user-secrets) to supply host, project, credentials and runtime options for the Infisical integration; its IsConfigured property is a quick way to detect whether the provider has enough information to run.

## Remarks
This class centralizes the four required values (Host, ProjectId, ClientId, ClientSecret) and several optional settings (Environment, SecretPath, TimeoutSeconds). ClientId is considered non-sensitive and acceptable in appsettings.json, while ClientSecret is sensitive and intended to come from user-secrets (key: "Infisical:ClientSecret") or the environment variable INFISICAL__CLIENTSECRET. The static SectionName property is provided so callers can bind configuration from the "Infisical" section. If any of the four essential values are missing or blank, IsConfigured returns false — the provider is expected to skip initialization quietly in that case so local development still succeeds without Infisical access.

## Example
```csharp
// In Program.cs (ASP.NET Core minimal hosting):
var builder = WebApplication.CreateBuilder(args);

// Bind and register options from configuration (appsettings, env vars, user-secrets)
builder.Services.Configure<InfisicalOptions>(builder.Configuration.GetSection(InfisicalOptions.SectionName));

// Later, optionally read and check configuration to decide whether to enable the provider
var opts = new InfisicalOptions();
builder.Configuration.GetSection(InfisicalOptions.SectionName).Bind(opts);
if (opts.IsConfigured)
{
    // Wire up the Infisical provider or client
    // e.g. services.AddSingleton(new InfisicalClient(opts));
}
```

## Notes
- ClientSecret is sensitive: prefer user-secrets or the environment variable INFISICAL__CLIENTSECRET (or the equivalent platform secret store) instead of committing it to source-controlled configuration files.
- IsConfigured performs simple non-empty checks. If it returns false the code that integrates with Infisical is expected to skip initialization; this is silent by design and may make missing configuration harder to notice unless you log or fail explicitly.
- TimeoutSeconds is measured in seconds and defaults to 15; SecretPath defaults to "/" and Environment defaults to "dev" — adjust if your deployment requires different values.
