# LLMProviderOptions

> **File:** `src/api/Gabriel.Core/Configuration/LLMProviderOptions.cs`  
> **Kind:** class

```csharp
public abstract class LLMProviderOptions
```


LLMProviderOptions is the shared base configuration for all LLM provider implementations. It consolidates provider-wide concerns—authentication, transport, sampling, and the model catalog—so each concrete provider can remain a thin subclass that only sets its SectionName. The provider’s identity (for example, Grok/OpenAI/Anthropic) is expressed by the IChatProvider implementation; the JSON section path serves as the discriminator at bind time, while the provider’s Name acts as the runtime discriminator in the IChatProviderRegistry.

Its members configure how to talk to a provider:
- BaseUrl: must end with a trailing slash so relative HttpClient paths resolve correctly.
- ApiKey: never committed. Supplying via environment variables (`PROVIDERS__<Name>`__APIKEY), user secrets, or a secret store ensures credentials are kept secure.
- TimeoutSeconds: total HTTP budget for a single chat call; streaming responses can last longer, so this generous default (900 seconds) helps avoid mid-generation cancellation.
- Temperature and TopP: sampling controls. These are optional and are applied at the provider level across the catalog; per-model overrides may be supported by specific providers.
- Models: the catalog of models exposed by this provider. The UI uses this list to present choices; the selected model is persisted to the user's PreferredModel.
- GetDefaultModel(): returns the first model marked as IsActive, or null if none are active.
- FindModel(string name): searches the catalog for a model with the given name using ordinal comparison.

These facilities together enable a consistent configuration surface across providers while allowing per-provider specialization through thin subclasses.