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
}
