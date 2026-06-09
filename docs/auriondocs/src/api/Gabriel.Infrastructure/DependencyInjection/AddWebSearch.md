Registers IWebSearch implementations based on configuration and wires per-provider HttpClients and singletons. Read the Tools:Web:Active configuration key (a comma-separated list of provider keys like "ddg", "brave", "tavily") to decide which providers to register; if the list contains multiple providers a CompositeWebSearch is registered (with cross-provider ranking) and each concrete provider is wrapped by an InstrumentedWebSearch to record metrics. Defaults to DuckDuckGo when the configuration value is missing or empty.

## Remarks
This method centralizes provider selection and DI wiring for web search integrations. It maps short provider keys to their required options, named HttpClient configuration, and concrete singleton types, then builds the final IWebSearch abstraction either as a single concrete implementation or as a CompositeWebSearch when multiple providers are requested. InstrumentedWebSearch is applied so per-provider call metrics flow into the application's metric recorder without changing the concrete search implementations.

## Example
```csharp
// Called from the application's DI setup (e.g. Startup.ConfigureServices or Program.Main)
// Configuration should contain Tools:Web:Active = "tavily,brave,ddg" (comma-separated keys).
AddWebSearch(services, Configuration);

// Internally this registers each provider's HttpClient and options, the concrete
// provider singletons, and then either a single IWebSearch or a CompositeWebSearch
// wrapped with instrumentation.
```

## Notes
- Default provider: if Tools:Web:Active is missing or empty the method uses "ddg" (DuckDuckGo).
- Unknown keys in Tools:Web:Active are ignored with a warning; typos won't crash startup but the provider won't be available.
- Some providers require configuration (e.g. Brave/Tavily options and API keys). Missing or incorrect option values may result in failed HTTP calls at runtime.
- When only one provider is requested the CompositeWebSearch wrapper is bypassed and the concrete implementation is registered directly as IWebSearch; when multiple providers are requested they are queried in parallel and merged with provider-aware ranking.
- Instrumentation depends on IMetricRecorder/IMetricRepository being registered elsewhere (Engine/EF wiring); this method only adds the decorator wiring.