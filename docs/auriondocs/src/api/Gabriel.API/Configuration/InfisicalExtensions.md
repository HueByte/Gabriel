# InfisicalExtensions

> **File:** `src/api/Gabriel.API/Configuration/InfisicalExtensions.cs`  
> **Kind:** class

Adds an Infisical-backed configuration source to an IConfigurationBuilder using the common options-pattern delegate. Use this extension when you want to register Infisical as a configuration source and configure connection/auth details via an InfisicalOptions callback (the same shape as common ASP.NET Core Add* extensions).

## Remarks
This helper creates a new InfisicalOptions instance, invokes the provided configuration callback to populate it, and then adds an InfisicalConfigurationSource configured with those options to the builder. It exists to provide a familiar, declarative registration pattern (`Action<TOptions>`) that integrates Infisical into the existing IConfigurationBuilder pipeline.

## Example
```csharp
// Registering during configuration setup
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddInfisical(opts =>
    {
        opts.ApiKey = Environment.GetEnvironmentVariable("INFISICAL_API_KEY");
        opts.Project = "my-project";
    });

// Or inside a Host builder
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddInfisical(opts =>
        {
            opts.Token = Environment.GetEnvironmentVariable("INFISICAL_TOKEN");
        });
    });
```

## Notes
- The method does not validate its arguments: passing a null builder or a null configure action will cause a NullReferenceException at runtime. Ensure both are non-null before calling.
- The source is added in the order the extension is invoked. Provider order affects precedence — later-added providers can override earlier values.
- The configure callback is executed synchronously during registration; avoid long-running or blocking operations inside that delegate.
