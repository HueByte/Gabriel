# InfisicalOptions

> **File:** `src/api/Gabriel.Core/Configuration/InfisicalOptions.cs`  
> **Kind:** class

Holds configuration values used to connect to an Infisical secret provider. Use this class when binding the application's "Infisical" configuration section (or environment variables) to drive the Infisical client; the provider that consumes these options will skip initialization when the minimal required values are absent.

## Remarks
This type centralizes both non-sensitive and sensitive settings for Infisical integration, provides sensible defaults (Environment = "dev", SecretPath = "/", TimeoutSeconds = 15) and exposes a SectionName constant for configuration binding. The IsConfigured property encodes the provider's startup behavior: if Host, ProjectId, ClientId or ClientSecret are missing/empty, the consumer should silently skip connecting to Infisical, allowing local development without secrets access.

## Example
```csharp
// appsettings.json (non-sensitive values)
{
  "Infisical": {
    "Host": "https://infisical.example.com",
    "ProjectId": "my-project",
    "Environment": "prod",
    "SecretPath": "/app/secrets",
    "ClientId": "public-client-id",
    "TimeoutSeconds": 30
  }
}

// Provide ClientSecret via user-secrets or environment variable:
// Environment variable name: INFISICAL__CLIENTSECRET

// In Program.cs / Startup.cs: bind the section
builder.Services.Configure<InfisicalOptions>(configuration.GetSection(InfisicalOptions.SectionName));

// Consumer example: read the bound options (IOptions<InfisicalOptions>) and check IsConfigured
if (options.Value.IsConfigured)
{
    // Initialize Infisical client
}
else
{
    // Skip provider initialization
}
```

## Notes
- ClientSecret is sensitive: do not store it in source-controlled appsettings.json. Use user-secrets or the environment variable INFISICAL__CLIENTSECRET instead.
- IsConfigured requires all four values (Host, ProjectId, ClientId, ClientSecret); missing any of them causes consumers to treat Infisical as unavailable.
- TimeoutSeconds is measured in seconds (default 15).