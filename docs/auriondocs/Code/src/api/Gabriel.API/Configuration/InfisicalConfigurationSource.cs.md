# InfisicalConfigurationSource

> **File:** `src/api/Gabriel.API/Configuration/InfisicalConfigurationSource.cs`  
> **Kind:** class

```csharp
public class InfisicalConfigurationSource : IConfigurationSource
```


InfisicalConfigurationSource is a lightweight IConfigurationSource that captures InfisicalOptions and creates an InfisicalConfigurationProvider when Build is called. It serves as the bridge between the application's configuration pipeline and the Infisical provider, allowing Infisical to supply configuration data through the standard configuration builder without leaking provider construction details into startup code.

## Remarks
InfisicalConfigurationSource centralizes the Infisical integration behind the IConfigurationSource interface. Callers add this source to the configuration builder; at build time, the provider is instantiated with the stored options, keeping concerns separated between wiring and data retrieval. The Build method ignores the incoming builder beyond constructing the provider, which keeps the source predictable and side-effect free.

## Notes
- If InfisicalOptions is mutable and shared after construction, changes will affect the provider since the field holds a reference; prefer treating options as immutable after wiring.