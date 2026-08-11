# InfisicalExtensions

> **File:** `src/api/Gabriel.API/Configuration/InfisicalExtensions.cs`  
> **Kind:** class

```csharp
public static class InfisicalExtensions
```


InfisicalExtensions provides an extension method on IConfigurationBuilder that wires Infisical as a configuration source using the standard Options-pattern. Call AddInfisical with a configure delegate to populate InfisicalOptions, then the extension registers an InfisicalConfigurationSource built from those options and returns the builder for fluent chaining.

## Remarks
InfisicalExtensions acts as an adapter that wires InfisicalConfigurationSource into the IConfigurationBuilder, enabling Infisical-managed values to be surfaced through IConfiguration during startup. It follows the well-known AddXxx pattern used by ASP.NET Core (for example AddDbContext), making the integration familiar to developers. The InfisicalOptions properties map directly to the connection and scope settings (Host, ProjectId, Environment, SecretPath) and control how the loaded values are exposed in the configuration tree via SectionName.
