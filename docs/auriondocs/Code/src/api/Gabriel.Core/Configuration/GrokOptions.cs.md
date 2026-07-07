# GrokOptions

> **File:** `src/api/Gabriel.Core/Configuration/GrokOptions.cs`  
> **Kind:** class

```csharp
public class GrokOptions : LLMProviderOptions, IConfigSection<GrokOptions>
```


GrokOptions provides the configuration contract for the Grok provider by extending the shared LLM transport settings defined in LLMProviderOptions. There are no Grok-specific fields added yet; the class exists so its SectionName can point to Providers:Grok, which enables the standard Options pipeline to bind the correct slice of appsettings via services.`ConfigureSection<GrokOptions>`(Configuration.GetSection(GrokOptions.SectionName)).

## Remarks
This abstraction centralizes Grok provider configuration under a provider-scoped section, while reusing the common transport configuration (BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, Models) from LLMProviderOptions. It keeps the binding logic uniform across providers and makes it easy to extend Grok-specific options in the future without altering binding code.
