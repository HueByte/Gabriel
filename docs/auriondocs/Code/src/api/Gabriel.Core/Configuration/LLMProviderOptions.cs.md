# LLMProviderOptions

> **File:** `src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs`  
> **Kind:** class

Shared configuration object for a single LLM provider. Use this type when binding provider-specific configuration from JSON (or other configuration sources) so provider-level concerns — base URL, authentication key, request timeout, sampling defaults, and the provider's model catalog — are centralized and validated in one place.

## Remarks
This abstract class consolidates settings that are common across LLM vendors so each concrete provider can remain a thin subclass (typically only setting its SectionName). The JSON configuration section path is used as the binder-time discriminator; the runtime identity of a provider (its human-friendly name like "Grok" or "OpenAI") lives on the IChatProvider implementation and is used by registries at runtime. The Models list is the provider-level catalog surfaced to the UI; the selected model is persisted on ApplicationUser.PreferredModel.

## Example
```csharp
// Inspecting provider options at runtime
LLMProviderOptions opts = GetBoundProviderOptions();
LLMModel? defaultModel = opts.GetDefaultModel();
LLMModel? specific = opts.FindModel("gpt-4-x");

if (defaultModel != null)
{
    Console.WriteLine($"Default model: {defaultModel.Name}");
}
```

## Notes
- BaseUrl must include a trailing slash so relative HttpClient paths resolve correctly; missing the slash can produce incorrect request URLs.
- ApiKey should never be committed to source control — supply via environment variables, secrets storage, or a secret manager (e.g., Infisical).
- FindModel uses an ordinal (case-sensitive) string comparison and GetDefaultModel returns the first model with IsActive==true; both can return null if no match or no active model is present.