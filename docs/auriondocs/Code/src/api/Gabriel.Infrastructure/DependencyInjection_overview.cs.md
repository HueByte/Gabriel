Static DependencyInjection acts as the central wiring point for infrastructure services, exposing extension methods like AddInfrastructure, AddWebFetch, and AddWebSearch to configure storage, HTTP access, and search behavior via the DI container. AddInfrastructure wires disk-backed project-file storage with options bound from Projects:Files; AddWebFetch registers a browser-like HttpClient with a sensible timeout and redirects allowed to support the web-fetch tool, while SSRF protection runs against the final destination; AddWebSearch wires a configurable set of web-search providers (driven by Tools:Web:Active), wrapping each provider in instrumentation and composing them in a rank-aware composite, with DuckDuckGo as a safe fallback when configuration yields no known providers.

## Remarks

Consolidates infrastructure concerns behind a small, testable surface, enabling easy swapping of providers, adjustments to metrics, or feature toggling in tests or different environments. The web-search wiring is configuration-driven: a comma-separated list of provider keys selects providers, per-provider instrumentation is applied via InstrumentedWebSearch, and diagnostics are exposed through the /diagnostics/web-search endpoint. By centralizing this logic, consumer code remains agnostic to provider identities and focuses on higher-level workflows.

## Notes

- The web-search wiring falls back to DuckDuckGo when no known keys are found in configuration; verify Tools:Web:Active to avoid unexpected defaults.
- AddWebFetch is private; to alter fetch behavior, modify this wiring class rather than adding external HttpClient usage.
- AddInfrastructure relies on configuration bindings from Projects:Files to determine storage behavior; ensure the config section exists to avoid misconfigured storage paths.