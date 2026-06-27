# InfisicalConfigurationSource

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs`  
> **Kind:** class

Provides an IConfigurationSource implementation that captures InfisicalOptions and produces InfisicalConfigurationProvider instances for the configuration system. Use this when you need to register an Infisical-backed configuration provider with an IConfigurationBuilder so provider instances are created by the configuration system rather than constructed inline.

## Remarks
This class is a small factory/adapter used by the Microsoft.Extensions.Configuration pipeline. It stores the InfisicalOptions supplied at construction time and, when the configuration builder calls Build, it returns a new InfisicalConfigurationProvider constructed with those options. Keeping the options on the source decouples provider construction from the caller and lets the configuration system instantiate providers as needed.

## Example
```csharp
// Create options (fill in required properties for your environment)
var opts = new InfisicalOptions { /* ApiKey = "...", ProjectId = "..." */ };

// Register the source with an IConfigurationBuilder
var builder = new ConfigurationBuilder();
builder.Add(new InfisicalConfigurationSource(opts));

var configuration = builder.Build();
// configuration now includes values provided by InfisicalConfigurationProvider
```

## Notes
- The constructor does not validate the provided InfisicalOptions; passing null will store a null reference and may cause the provider to fail at runtime. Validate or guard against null when creating the source.
- Build ignores the IConfigurationBuilder parameter and always returns a new InfisicalConfigurationProvider constructed with the stored options. Multiple Build calls produce distinct provider instances.