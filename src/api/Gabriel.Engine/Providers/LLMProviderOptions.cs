namespace Gabriel.Engine.Providers;

// Shared shape for every LLM provider's config block. Provider-level concerns
// (auth, transport, transport timeouts) live here so the concrete provider
// classes can focus on sampling / format-specific knobs.
//
// The Models[] array is the multi-model surface: each concrete provider's
// section in appsettings.json carries one block of models and flips IsActive
// to switch between them at runtime without a redeploy.
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
    // you want to be able to switch to; flip IsActive on exactly one.
    public IList<LLMModel> Models { get; set; } = new List<LLMModel>();

    // Resolved active model. Returns null only when the config block is
    // malformed (no active model declared) — callers should treat that as a
    // hard failure rather than silently picking a default.
    public LLMModel? GetActiveModel() => Models.FirstOrDefault(m => m.IsActive);
}
