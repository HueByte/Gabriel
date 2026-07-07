# GrokOptions

> **File:** `src/api/Gabriel.Core/Configuration/GrokOptions.cs`  
> **Kind:** class

```csharp
public class GrokOptions : LLMProviderOptions, IConfigSection<GrokOptions>
```


GrokOptions is a configuration-driven options class that encapsulates the transport settings for the Grok LLM provider. It derives all standard transport settings (BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, and Models) from LLMProviderOptions and adds no Grok-specific properties at present; its existence simply anchors the Providers:Grok section for configuration binding.

## Remarks
GrokOptions isolates provider-specific configuration and enables per-provider binding, validation, and future Grok-specific options without impacting the shared transport surface. By exposing a dedicated SectionName, it ensures DI and configuration bind to the correct slice of appsettings (Providers:Grok).

## Example
```csharp
// Example DI wiring for Grok provider configuration
var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

services.ConfigureSection<GrokOptions>(configuration.GetSection(GrokOptions.SectionName));

// Inject IOptions<GrokOptions> into services that need Grok settings
```

## Notes
- Binding relies on GrokOptions.SectionName; ensure the appsettings.json includes Providers:Grok with the relevant fields.
- No new properties are defined yet; Grok-specific options can be added here in the future without changing the shared transport surface.
- If the Providers:Grok section is missing or misnamed, binding may yield defaults from LLMProviderOptions or require explicit handling at startup.