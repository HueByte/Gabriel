# InfisicalExtensions

> **File:** `src/api/Gabriel.API/Configuration/InfisicalExtensions.cs`  
> **Kind:** class

Adds Infisical as a configuration source to an IConfigurationBuilder using the familiar options-pattern. Use this extension when integrating Infisical-backed configuration into an application's configuration pipeline (for example in Program.cs or Startup) and you want to configure behavior via an InfisicalOptions callback.

## Remarks
This is a small convenience wrapper that follows the canonical ASP.NET Core options-style registration (same shape as AddDbContext/AddSwaggerGen). It constructs an InfisicalOptions instance, invokes the provided configuration delegate to populate it, and then registers an InfisicalConfigurationSource with the builder. The method keeps configuration registration concise and idiomatic.

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInfisical(opts =>
{
    opts.ApiKey = "your-api-key";      // example option property
    opts.Project = "my-project";
    opts.Environment = "development";
});

var app = builder.Build();
```

## Notes
- The configure delegate is executed synchronously during registration; the configured InfisicalOptions instance is passed to InfisicalConfigurationSource at that time.
- The method does not validate or null-check the builder argument; calling it with a null IConfigurationBuilder will result in a NullReferenceException.
- Whether later mutations to the InfisicalOptions instance affect the configuration source depends on how InfisicalConfigurationSource stores or copies the options (the instance is passed to its constructor).