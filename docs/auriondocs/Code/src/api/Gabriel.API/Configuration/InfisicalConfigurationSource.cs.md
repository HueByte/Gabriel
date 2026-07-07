# InfisicalConfigurationSource

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs`  
> **Kind:** class

```csharp
public class InfisicalConfigurationSource : IConfigurationSource
```


InfisicalConfigurationSource acts as a minimal adapter that plugs Infisical into the .NET configuration system. It captures InfisicalOptions at construction and, when Build is called, returns an InfisicalConfigurationProvider configured with those options. This allows developers to add this source to an IConfigurationBuilder and access Infisical-related configuration values through the standard configuration APIs without manually wiring the provider.

## Remarks
InfisicalConfigurationSource is a thin adapter that decouples option setup from the provider creation. By storing the options and creating the provider in Build, it enables reuse of the same configuration setup across different parts of an application and keeps the provider creation logic centralized in one place. It follows the common IConfigurationSource pattern used by other configuration providers, making Infisical configuration consistent with the rest of the system.