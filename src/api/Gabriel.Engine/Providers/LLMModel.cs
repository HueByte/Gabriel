namespace Gabriel.Engine.Providers;

// Per-model metadata. A provider (Grok, Anthropic, etc.) typically exposes
// several models — different sizes, different price points — and we want the
// switching to be a config edit, not a code change. Hence the `Models` array
// on LLMProviderOptions: provider-shared fields (BaseUrl, ApiKey) live on the
// outer object, model-shared fields live here.
//
// Pricing is per-million-tokens to match the units every vendor's price page
// uses; multiply by usage / 1_000_000 to get the per-call dollar figure.
public class LLMModel
{
    // The wire-level model identifier sent to the provider (e.g. "grok-4-latest",
    // "claude-opus-4-7"). What the API expects, not the marketing name.
    public string Name { get; set; } = string.Empty;

    // Exactly one model in the array should be IsActive=true. The provider
    // picks it via LLMProviderOptions.GetActiveModel(); a missing or duplicate
    // active flag is treated as a config error at validation time.
    public bool IsActive { get; set; }

    // Total context budget for this model. Used by token-accounting code (e.g.
    // GrokChatProvider.ContextWindowTokens) to decide when to trim or summarize.
    public int ContextWindowTokens { get; set; }

    // USD per million tokens. Zero means either "free" or "unknown" — we treat
    // the latter as the former for accounting purposes; a real cost-tracking
    // pass should validate non-zero values for the active model.
    public decimal InputPricePerMTokens { get; set; }
    public decimal OutputPricePerMTokens { get; set; }

    // Some providers (Anthropic, OpenAI) charge differently for cached prompt
    // reads vs cache writes. Leave 0 for providers that don't support caching.
    public decimal CacheReadPricePerMTokens { get; set; }
    public decimal CacheWritePricePerMTokens { get; set; }
}
