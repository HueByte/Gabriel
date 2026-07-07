# LLMProviderOptions

> **File:** `src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs`  
> **Kind:** class

```csharp
public abstract class LLMProviderOptions
```


LLMProviderOptions is an abstract base that defines the shared, provider-wide configuration for all LLM providers. It centralizes authentication, HTTP transport, sampling controls, and the model catalog so concrete provider option classes can remain thin, primarily setting their SectionName. Identity (the provider name such as Grok, OpenAI, or Anthropic) lives on the IChatProvider implementation rather than here—the JSON section path acts as the discriminator during binding, and the provider’s Name is the runtime discriminator used by IChatProviderRegistry. The properties exposed here include BaseUrl, ApiKey, TimeoutSeconds, Temperature, TopP, and Models, along with helper accessors GetDefaultModel() and FindModel(string).

## Remarks
Centralizing these concerns makes it easy to apply uniform policies across providers (e.g., how long a request can run, how sampling is configured) while keeping provider-specific defaults and model catalogs separate in thin subclasses. The Models catalog feeds the UI model selector; the chosen model is persisted to ApplicationUser.PreferredModel, while GetDefaultModel() provides a convenient fallback when no explicit preference is set.
GetDefaultModel returns the first active model, or null if none are active; FindModel(name) looks up a model by its Name using ordinal comparison.

## Notes
- BaseUrl must end with a trailing slash to ensure relative HttpClient paths resolve correctly.
- ApiKey is sensitive and must not be committed; supply it securely via environment variables (`PROVIDERS__<Name>`__APIKEY), user secrets, or Infisical.
- GetDefaultModel/FindModel semantics: GetDefaultModel returns the first IsActive model or null if none; FindModel uses ordinal string comparison to locate a model by name.