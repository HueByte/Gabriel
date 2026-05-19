namespace Gabriel.Core.Configuration;

// xAI / Grok provider config. Inherits the shared LLM transport surface
// (BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, Models[]) from
// LLMProviderOptions. There's nothing Grok-specific to add right now — the
// subclass exists so its SectionName slot can point at "Providers:Grok",
// which is what makes services.ConfigureSection<GrokOptions>(config) bind
// the right slice of appsettings via the standard Options pipeline.
public class GrokOptions : LLMProviderOptions, IConfigSection<GrokOptions>
{
    public static string SectionName => "Providers:Grok";
}
