Registers and configures web search provider implementations into the dependency injection container based on the Tools:Web:Active configuration key. Call this during application startup when you want the app to resolve IWebSearch backed by one or more concrete providers (DuckDuckGo by default, or Brave/Tavily when configured); the method wires provider-specific HttpClient instances, options bindings, and provider singletons, then wraps implementations with an instrumenting decorator and — if multiple providers were enabled — exposes a CompositeWebSearch that merges results across providers.

## Remarks
This method centralizes DI wiring for all supported web-search providers so callers only need to change configuration to enable/disable providers. Per-provider HTTP client setup and options binding are performed here (e.g. base URL, timeout, API keys), while call metrics are recorded by an InstrumentedWebSearch decorator that writes into the generic IMetricRecorder/IMetricRepository stack. When more than one provider is requested the code registers a CompositeWebSearch that queries providers in parallel and merges results in a rank-aware way; when a single provider is requested the composite is bypassed and the bare provider implementation is registered as IWebSearch.

## Example
```csharp
// appsettings.json
{
  "Tools": {
    "Web": {
      "Active": "tavily,brave,ddg",
      "Brave": {
        "ApiKey": "<your-brave-key>",
        "BaseUrl": "https://api.brave.com/",
        "TimeoutSeconds": 10
      },
      "Tavily": {
        "ApiKey": "<your-tavily-key>",
        "BaseUrl": "https://api.tavily.example/",
        "TimeoutSeconds": 10
      }
    }
  }
}

// Program.cs / Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // other registrations...
    AddWebSearch(services, Configuration);
}
```

## Notes
- Default provider: if Tools:Web:Active is missing or empty the method falls back to DuckDuckGo ("ddg").
- Unknown keys in the Tools:Web:Active list are ignored with a warning — typos won't crash startup but will not enable a provider.
- Brave and Tavily require provider-specific configuration (API keys and base URLs) to be present; missing keys mean those providers won't function correctly.
- The ordering of provider keys in Tools:Web:Active does not affect result ranking; the composite merger is already rank-aware and prioritizes cross-provider hits.