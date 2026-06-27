# GrokOptions

> **File:** `src/api/Gabriel.Core/Configuration/GrokOptions.cs`  
> **Kind:** class

Holds configuration for the Grok LLM provider and serves as the target type for configuration binding. It inherits common LLM transport and runtime settings from LLMProviderOptions (BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, Models[], etc.) and exposes a SectionName so the Options pipeline can bind the "Providers:Grok" section from appsettings.

## Remarks
This subclass exists primarily to provide a stable configuration section identifier (SectionName) and to act as the concrete options type for dependency injection. There are no Grok-specific settings yet; keeping a dedicated type makes it straightforward to add provider-specific properties in the future and allows helper registration helpers (such as a ConfigureSection extension that uses [`IConfigSection<T>`](IConfigSection.cs.md)) to target the correct configuration slice.

## Example
```csharp
// appsettings.json (conceptual)
// "Providers": { "Grok": { "BaseUrl": "https://api.grok.example", "ApiKey": "...", "TimeoutSeconds": 30 } }

// In Program.cs or Startup.cs
builder.Services.ConfigureSection<GrokOptions>(configuration);

// Later you can inject IOptions<GrokOptions> or IOptionsMonitor<GrokOptions> where needed
``` 

## Notes
- The SectionName constant must match the configuration path; changing it will break automatic binding.
- GrokOptions currently adds no properties beyond LLMProviderOptions, so provider behavior is driven by the base-class settings until Grok-specific fields are introduced.
- The class implements [`IConfigSection<GrokOptions>`](IConfigSection.cs.md) so make sure the app's ConfigureSection helper (or equivalent) recognizes that pattern when registering options.