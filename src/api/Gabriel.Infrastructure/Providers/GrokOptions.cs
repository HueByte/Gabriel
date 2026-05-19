namespace Gabriel.Infrastructure.Providers;

public class GrokOptions
{
    public const string SectionName = "Providers:Grok";

    // BaseUrl must end with a trailing slash so relative paths concatenate correctly.
    public string BaseUrl { get; set; } = "https://api.x.ai/v1/";

    // Override per-deployment via config or env var (PROVIDERS__GROK__MODEL).
    public string Model { get; set; } = "grok-4-latest";

    // Supply via User Secrets or env var (PROVIDERS__GROK__APIKEY). Never commit.
    public string ApiKey { get; set; } = string.Empty;

    // Total request budget for a single chat call, applied via the standard
    // resilience pipeline (not HttpClient.Timeout — which is set to infinite so
    // the pipeline is the single source of truth). Must comfortably exceed the
    // longest expected SSE stream; the default is generous so long generations
    // don't cut mid-token.
    public int TimeoutSeconds { get; set; } = 900;

    // Approximate context window for the active model. grok-4 is 256k; older models
    // are smaller. Override per-deployment if you change the Model field.
    public int ContextWindowTokens { get; set; } = 256_000;

    // Sampling controls. 0.8-0.9 hits a nice spot for the natural-DM persona —
    // 1.0 reads as slightly too random, below 0.7 starts feeling robotic. top_p
    // 0.9 keeps the tail constrained without choking variance.
    public double? Temperature { get; set; } = 0.85;
    public double? TopP { get; set; } = 0.9;
}
