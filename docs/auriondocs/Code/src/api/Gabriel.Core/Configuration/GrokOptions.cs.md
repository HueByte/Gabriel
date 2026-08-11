# GrokOptions

> **File:** `src/api/Gabriel.Core/Configuration/GrokOptions.cs`  
> **Kind:** class

```csharp
public class GrokOptions : LLMProviderOptions, IConfigSection<GrokOptions>
```


GrokOptions provides the configuration contract for the Grok LLM provider. It inherits the standard transport surface from LLMProviderOptions (BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, Models) and currently adds no Grok-specific properties; its primary purpose is to enable binding to the Providers:Grok configuration section via the Options pattern so the application can configure this provider independently from others.

## Remarks
By keeping GrokOptions separate from LLMProviderOptions, the codebase can evolve provider-specific settings without affecting other providers. The SectionName property ties the settings to the Providers:Grok key, allowing services.`ConfigureSection<GrokOptions>`(config) to wire the correct slice of configuration through the standard Options pipeline. If Grok later introduces fields unique to this provider, GrokOptions is the natural place to house them without mutating the shared base class.

## Notes
- Be careful: GrokOptions currently does not declare new properties; any provider-specific settings belong here and should be mirrored in appsettings.
- Ensure that the SectionName matches the actual configuration path (Providers:Grok) used by the host to bind this section.