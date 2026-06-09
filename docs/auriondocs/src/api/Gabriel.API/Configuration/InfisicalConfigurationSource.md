# InfisicalConfigurationSource

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs`  
> **Kind:** class

Provides an IConfigurationSource that wraps a configured InfisicalOptions instance and produces an InfisicalConfigurationProvider. Use this when you want to register an Infisical-backed configuration provider with an IConfigurationBuilder so application configuration can be sourced from Infisical.

## Remarks
This class is a thin adapter between the Microsoft.Extensions.Configuration pipeline and the InfisicalConfigurationProvider implementation. It captures the InfisicalOptions provided at construction time and returns a new provider when Build is called; it does not itself perform any network or secret retrieval work.

## Example
```csharp
var infisicalOptions = new InfisicalOptions
{
    // populate options (e.g. ApiKey, ProjectId, Environment)
};

var builder = new ConfigurationBuilder()
    .Add(new InfisicalConfigurationSource(infisicalOptions));

IConfiguration configuration = builder.Build();
// configuration now includes values supplied by InfisicalConfigurationProvider
```

## Notes
- The IConfigurationBuilder parameter passed to Build is not used by this implementation; the source only forwards the captured InfisicalOptions to the provider.
- InfisicalOptions is stored by reference. Mutating the options instance after creating the source may affect provider behavior if the provider reads options lazily.
- No extension method is provided here; register the source by calling ConfigurationBuilder.Add(new InfisicalConfigurationSource(...)) or by wrapping it in your own convenience extension.