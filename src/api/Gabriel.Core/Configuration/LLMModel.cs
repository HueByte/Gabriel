namespace Gabriel.Core.Configuration;

// Per-model metadata. A provider typically exposes several models — different
// sizes, different price points, different pricing tiers across the context
// window — and we want the switching to be a config edit, not a code change.
//
// Pricing is per-million-tokens to match the units every vendor's price page
// uses; multiply by usage / 1_000_000 to get the per-call dollar figure.
public class LLMModel
{
    // The wire-level model identifier sent to the provider (e.g. "grok-4.3",
    // "claude-opus-4-7"). What the API expects, not the marketing name.
    public string Name { get; set; } = string.Empty;

    // The default/bootstrap selection. Per-user PreferredModel (on
    // ApplicationUser) overrides this at runtime; IsActive is just the
    // fallback when no user preference has been set yet. Models discovery
    // (GET /api/models) returns every entry across every provider regardless
    // of this flag.
    public bool IsActive { get; set; }

    // Total context budget for this model. Used by token-accounting code to
    // decide when to trim or summarize.
    public int ContextWindowTokens { get; set; }

    // Optional per-model override for the rolling-summary compact trigger,
    // expressed as a fraction of ContextWindowTokens (e.g. 0.18 = compact
    // when estimated history hits 18% of the window). Useful when a vendor
    // tiers pricing inside the window — grok-4.3, for example, charges
    // differently above 200k tokens, so you can set this to ~0.18 to keep
    // the conversation inside the cheap tier.
    //
    // Falls back to AgentOptions.CompactThreshold when null.
    public double? CompactThreshold { get; set; }

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
