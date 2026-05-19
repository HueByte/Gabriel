namespace Gabriel.Core.Configuration;

// Shared shape for every LLM provider's config block. Provider-level concerns
// (auth, transport, sampling, model catalog) live here so each concrete
// provider's options class can be a thin subclass that only sets its
// SectionName.
//
// Identity (the "Grok" / "OpenAI" / "Anthropic" name) lives on the
// IChatProvider implementation, not here — the JSON section path is the
// discriminator at bind time, the provider's Name is the discriminator at
// runtime (used by IChatProviderRegistry).
public abstract class LLMProviderOptions
{
    // Must end with a trailing slash so relative HttpClient paths resolve
    // correctly.
    public string BaseUrl { get; set; } = string.Empty;

    // Never commit. Supply via env var (PROVIDERS__<Name>__APIKEY),
    // user-secrets, or Infisical.
    public string ApiKey { get; set; } = string.Empty;

    // Total HTTP budget for one chat call, applied via the resilience
    // pipeline. SSE streaming calls can run for minutes — keep this generous
    // so long generations don't cut mid-token.
    public int TimeoutSeconds { get; set; } = 900;

    // Sampling controls. Optional — providers that don't honour them just
    // ignore the field. Live at the provider level (not per-model) because
    // most vendors apply them identically across their model catalog; if a
    // future model needs different defaults, add per-model overrides on
    // LLMModel instead.
    public double? Temperature { get; set; }
    public double? TopP { get; set; }

    // Catalog of models exposed under this provider. The UI selector pulls
    // from here; the user picks one and it persists onto
    // ApplicationUser.PreferredModel.
    public IList<LLMModel> Models { get; set; } = new List<LLMModel>();

    // Resolved default model — the entry config marks IsActive=true. Null
    // when no model is flagged; the IModelCatalog handles the catalog-wide
    // fallback in that case.
    public LLMModel? GetDefaultModel() => Models.FirstOrDefault(m => m.IsActive);

    public LLMModel? FindModel(string name) =>
        Models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.Ordinal));
}
