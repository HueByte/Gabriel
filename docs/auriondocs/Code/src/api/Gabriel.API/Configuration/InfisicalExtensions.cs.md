# InfisicalExtensions

> **File:** `src/api/Gabriel.API/Configuration/InfisicalExtensions.cs`  
> **Kind:** class

```csharp
public static class InfisicalExtensions
```


Adds Infisical as a configuration source to an IConfigurationBuilder. Use it when you want to source configuration values from Infisical at startup by configuring InfisicalOptions via the canonical Options-pattern and then registering an InfisicalConfigurationSource with the builder.

## Remarks
This method mirrors the well-known options pattern (think AddDbContext/AddSwaggerGen) to build options from a caller-supplied action and pass them to a configuration source. It delegates the actual retrieval and provider construction to InfisicalConfigurationSource, keeping Infisical integration consistent with other configuration providers and enabling fluent chaining with other configuration sources.

## Notes
- If required fields are omitted or misconfigured, the Infisical configuration source may not be able to fetch secrets at startup.