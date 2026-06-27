# BraveSearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`  
> **Kind:** class

BraveSearchOptions provides typed configuration for the Brave Search integration and is intended to be bound from configuration (e.g., appsettings, environment variables, or user-secrets). Use this when wiring up the Brave search client so callers can read the base API URL, subscription token, and request timeout from configuration rather than hard-coding them.

## Remarks
This class is a small POCO used as a config section (the SectionName constant is the expected configuration path: "Tools:Web:Brave"). It centralizes defaults useful for the Brave Search client: a sensible BaseUrl that already targets the Brave web search endpoint, a default timeout in seconds, and an ApiKey that, when left empty, explicitly marks the integration as unconfigured so code can disable the feature instead of throwing.

## Example
```csharp
// In Program.cs or equivalent during startup
var braveSection = configuration.GetSection(BraveSearchOptions.SectionName);
var braveOptions = braveSection.Get<BraveSearchOptions>();

if (!braveOptions.IsConfigured)
{
    // Feature disabled or return a friendly error to callers
    return;
}

// Create an HttpClient configured from the options
var client = new HttpClient
{
    BaseAddress = new Uri(braveOptions.BaseUrl),
    Timeout = TimeSpan.FromSeconds(braveOptions.TimeoutSeconds)
};

// Note: add the subscription token to requests according to Brave's API docs.
```

## Notes
- BaseUrl must end with a trailing slash so relative-path concatenation resolves correctly (the default already includes it).
- An empty or whitespace ApiKey means the integration is treated as disabled (IsConfigured == false).
- TimeoutSeconds is expressed in seconds and is applied directly to HttpClient.Timeout in typical usage.
- Changing SectionName will affect where the runtime looks for configuration values; keep config keys in sync with this value.