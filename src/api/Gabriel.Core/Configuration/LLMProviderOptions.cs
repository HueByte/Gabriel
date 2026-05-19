namespace Gabriel.Core.Configuration;

// Shared shape for every LLM provider's config block. Provider-level concerns
// (auth, transport, transport timeouts) live here so the concrete provider
// classes can focus on sampling / format-specific knobs.
//
// The Models[] array is the multi-model surface: each concrete provider's
// section in appsettings.json carries one block of models and the user picks
// which one to use at runtime via the UI. IsActive is just the bootstrap
// default — see ApplicationUser.PreferredModel for the per-user override.
public abstract class LLMProviderOptions
{
    // Must end with a trailing slash so relative HttpClient paths resolve correctly.
    public string BaseUrl { get; set; } = string.Empty;

    // Never commit. Supply via env var (PROVIDERS__<NAME>__APIKEY), user-secrets,
    // or Infisical.
    public string ApiKey { get; set; } = string.Empty;

    // Total HTTP budget for one chat call, applied via the resilience pipeline.
    // SSE streaming calls can run for minutes — keep this generous so long
    // generations don't cut mid-token.
    public int TimeoutSeconds { get; set; } = 900;

    // Catalog of models exposed under this provider. Add an entry per model
    // you want available in the UI selector; the user picks from this list.
    public IList<LLMModel> Models { get; set; } = new List<LLMModel>();

    // Resolved default model — the one config marks IsActive=true. Returns
    // null when no model is flagged; callers should treat that as a fallback
    // signal (try first model, or surface a config error).
    public LLMModel? GetDefaultModel() => Models.FirstOrDefault(m => m.IsActive);

    // Resolve a model by its wire-level name. Returns null if not found —
    // callers should treat that as "user picked a stale/removed model"
    // and fall back to GetDefaultModel().
    public LLMModel? FindModel(string name) =>
        Models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.Ordinal));
}
