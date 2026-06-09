# LLMProviderOptions

> **File:** `src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs`  
> **Kind:** class

Shared base configuration for an LLM provider. Use this abstract class when declaring provider-specific options so common concerns — HTTP base URL, credentials, timeouts, sampling defaults and the provider's model catalog — are stored in one place and reused by concrete provider option types.

## Remarks
This class centralises provider-level settings that most vendors apply across their model catalog, allowing concrete provider option classes to remain thin (typically they only supply the configuration section name). The provider implementation (IChatProvider) carries the runtime identity (e.g., "OpenAI", "Anthropic"); the configuration binder uses the JSON section path to choose which concrete options to bind, while the provider's Name is the runtime discriminator used by the IChatProviderRegistry. Model selection UI and persistence rely on the Models collection and the IsActive flag; when no model is flagged active the higher-level IModelCatalog is expected to provide a fallback.

## Example
```csharp
// concrete provider options typically subclass this and declare a configuration section name
public class OpenAIProviderOptions : LLMProviderOptions
{
    public const string SectionName = "Providers:OpenAI";
}

// common operations
var opts = new OpenAIProviderOptions
{
    BaseUrl = "https://api.openai.com/v1/",
    Temperature = 0.7,
};
opts.Models.Add(new LLMModel { Name = "gpt-4", IsActive = true });

var defaultModel = opts.GetDefaultModel(); // first model with IsActive == true
var found = opts.FindModel("gpt-4"); // case-sensitive lookup (Ordinal)
```

## Notes
- BaseUrl must end with a trailing slash so relative HttpClient paths resolve correctly.
- Do not commit ApiKey; supply it via environment variables (convention: `PROVIDERS__<Name>`__APIKEY), user secrets, or a secrets manager.
- TimeoutSeconds defaults to 900 to accommodate long-running streaming calls; reduce only if you intend to cap generation time aggressively.
- Temperature and TopP are nullable: some providers will ignore them if unsupported.
- FindModel uses StringComparison.Ordinal (case-sensitive). GetDefaultModel returns the first model with IsActive == true.
- This is a simple POCO for configuration binding — it is not synchronized for concurrent mutation.
