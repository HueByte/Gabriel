Configures dependency injection for the web-search feature by wiring up the providers requested via configuration. It reads Tools:Web:Active, normalizes and deduplicates the list, then registers an HttpClient + concrete implementation for each known provider (Brave, Tavily, DuckDuckGo) and exposes them as singletons. When multiple providers are enabled, the method relies on a CompositeWebSearch to merge results; with a single provider, the bare implementation is registered directly as IWebSearch. All provider implementations are wrapped with InstrumentedWebSearch to record metrics (written into IMetricRecorder) and surfaced via the /diagnostics/web-search endpoint. Unknown keys are ignored with a warning. If Tools:Web:Active is not set, a sensible default of ddg (DuckDuckGo) is used.

## Remarks
Centralizes the wiring of external web-search backends behind a single configuration-driven surface. This enables swapping, enabling, or combining providers without changing startup code, and isolates metric instrumentation behind the InstrumentedWebSearch decorator. It also ensures that, regardless of how many providers are active, the outcome is a cohesive IWebSearch experience (single provider when only one is configured, or a merged composite when multiple).

## Example
```csharp
// appsettings.json (illustrative)
{
  "Tools": {
    "Web": {
      "Active": "ddg,tavily,brave"
    }
  }
}
```

## Notes
- Unknown keys in Tools:Web:Active are ignored with a warning to avoid hard failures on typos.
- If Tools:Web:Active is missing or empty, the default is DuckDuckGo (ddg).
- Each provider is registered with a provider-specific HttpClient configuration (base URL, timeout, headers), so misconfiguration of options can lead to runtime failures unless the corresponding options sections exist.