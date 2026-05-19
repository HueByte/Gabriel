using Gabriel.Core.Configuration;
using Gabriel.Engine.Providers;

namespace Gabriel.Infrastructure.Providers;

// xAI / Grok provider config. Inherits the shared LLM transport surface
// (BaseUrl, ApiKey, TimeoutSeconds, Models[]) from LLMProviderOptions and
// layers on the Grok-specific sampling knobs below. The static SectionName
// lets services.ConfigureSection<GrokOptions>(config) bind without restating
// the path.
public class GrokOptions : LLMProviderOptions, IConfigSection<GrokOptions>
{
    public static string SectionName => "Providers:Grok";

    // Sampling controls. 0.8-0.9 hits a nice spot for the natural-DM persona —
    // 1.0 reads as slightly too random, below 0.7 starts feeling robotic.
    // top_p 0.9 keeps the tail constrained without choking variance.
    public double? Temperature { get; set; } = 0.85;
    public double? TopP { get; set; } = 0.9;
}
