# BraveSearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`  
> **Kind:** class

Configuration options for the Brave Search integration. Use this type when binding application configuration (or environment/user-secrets) for the Brave web-search tool; it centralizes the base URL, API key, and timeout and provides a convenience IsConfigured flag so callers can detect when the tool should be treated as disabled.

## Remarks
This class exposes the configuration section name (Tools:Web:Brave) so the app's configuration system can bind values consistently. It implements [`IConfigSection<T>`](IConfigSection.cs.md) to participate in the project's configuration-pattern conventions; consumers typically obtain an instance via the options pattern or direct configuration binding and then pass it to the component that performs Brave Search requests.

## Example
```csharp
// Bind using IConfiguration (e.g. in Program.cs)
var braveSection = configuration.GetSection(BraveSearchOptions.SectionName);
var braveOptions = braveSection.Get<BraveSearchOptions>();

if (braveOptions != null && braveOptions.IsConfigured)
{
    // Use braveOptions.BaseUrl, braveOptions.ApiKey, braveOptions.TimeoutSeconds
}

// Or register with the DI options system
services.Configure<BraveSearchOptions>(configuration.GetSection(BraveSearchOptions.SectionName));
// then inject IOptions<BraveSearchOptions> where needed
```

## Notes
- BaseUrl must end with a trailing slash (the code expects BaseUrl + relative path to concatenate correctly).
- An empty ApiKey disables the tool (IsConfigured will be false); supplying an API key via environment/user-secrets enables it.
- The codebase expects the Brave API key to be available via secrets/Env (example env name used in comments: TOOLS__WEB__BRAVE__APIKEY) — ensure the key is set in your chosen configuration source.
- TimeoutSeconds is expressed in seconds and defaults to 15.