namespace Gabriel.Infrastructure.Tools.Web;

public class BraveSearchOptions
{
    public const string SectionName = "Tools:Web:Brave";

    // Brave Search API base. Must end with a trailing slash so the
    // BaseAddress + relative path concat lands at /res/v1/web/search.
    public string BaseUrl { get; set; } = "https://api.search.brave.com/res/v1/web/";

    // Subscription token from https://api.search.brave.com/keys. Supply via
    // Infisical (TOOLS__WEB__BRAVE__APIKEY) or user-secrets. Empty = disabled
    // (the search tool returns an "unconfigured" error instead of crashing).
    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
