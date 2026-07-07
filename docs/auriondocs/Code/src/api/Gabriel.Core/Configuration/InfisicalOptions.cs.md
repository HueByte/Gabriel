# InfisicalOptions

> **File:** `src/api/Gabriel.Core/Configuration/InfisicalOptions.cs`  
> **Kind:** class

```csharp
public class InfisicalOptions : IConfigSection<InfisicalOptions>
```


InfisicalOptions is the strongly-typed configuration section for Infisical integration. It binds to the Infisical configuration subsection and exposes Host, ProjectId, Environment, SecretPath, ClientId, ClientSecret, and TimeoutSeconds, along with a computed IsConfigured flag that indicates whether the essential values are present and thus whether the Infisical provider should activate.

## Remarks
This class serves as the centralized configuration anchor for Infisical, encapsulating which values are required to initialize the integration and providing sensible defaults. It enables the rest of the application to work with Infisical by relying on a single, typed binding that participates in the configuration pipeline via the [`IConfigSection<InfisicalOptions>`](IConfigSection.cs.md) contract. The SectionName is "Infisical", guiding the configuration binder to the correct subsection and keeping concerns separated from business logic. The IsConfigured flag acts as a guard so the provider can be skipped gracefully when Infisical is not available in the environment.

## Notes
- Do not store ClientSecret in appsettings.json; supply it via user-secrets or environment variable INFISICAL__CLIENTSECRET.
- IsConfigured requires Host, ProjectId, ClientId, and ClientSecret to be non-empty; if any are missing, the Infisical provider will skip to keep local development resilient.
- The settings bind under the Infisical section (SectionName = "Infisical"), with Environment defaulting to "dev" and TimeoutSeconds defaulting to 15.
