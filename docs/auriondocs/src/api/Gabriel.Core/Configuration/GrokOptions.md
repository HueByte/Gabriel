# GrokOptions

> **File:** `src/api/Gabriel.Core/Configuration/GrokOptions.cs`  
> **Kind:** class

Represents the configuration slice for the Grok LLM provider and is used when binding provider settings from configuration (appsettings.json, environment variables, etc.) into the standard Options pipeline. It inherits common transport and tuning properties from LLMProviderOptions (e.g. BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, Models[]) and exposes a static SectionName so the application can bind the correct configuration path.

## Remarks
This subclass intentionally does not add Grok-specific properties; it exists primarily to provide a concrete type and a SectionName ("Providers:Grok") so calls like services.`ConfigureSection<GrokOptions>`(configuration) bind the correct configuration section. Use this type when you want strongly-typed access to the shared LLM provider settings for the Grok provider via `IOptions<GrokOptions>` or `IOptionsMonitor<GrokOptions>`.

## Example
```csharp
// appsettings.json (excerpt)
{
  "Providers": {
    "Grok": {
      "BaseUrl": "https://api.grok.example",
      "ApiKey": "your-key-here",
      "TimeoutSeconds": 30,
      "Temperature": 0.7,
      "TopP": 0.9,
      "Models": [ "grok-1" ]
    }
  }
}

// Program.cs / Startup.cs
// Bind the Grok section into the options system (uses the static SectionName on GrokOptions)
services.ConfigureSection<GrokOptions>(Configuration);

// Consuming service
public class MyService
{
    public MyService(Microsoft.Extensions.Options.IOptions<GrokOptions> options)
    {
        var grokOptions = options.Value;
        // use grokOptions.BaseUrl, grokOptions.ApiKey, etc.
    }
}
```

## Notes
- GrokOptions does not declare provider-specific fields — all available settings come from its base LLMProviderOptions.
- The SectionName value ("Providers:Grok") must match your configuration layout; changing it requires updating configuration or the binding call.
- This type is for configuration binding only; registering or wiring the runtime Grok provider implementation is a separate step.