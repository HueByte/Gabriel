# BraveSearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`  
> **Kind:** class

```csharp
public class BraveSearchOptions : IConfigSection<BraveSearchOptions>
```


BraveSearchOptions is a typed configuration container for the Brave Web Search integration. It binds to the Tools:Web:Brave configuration section and exposes BaseUrl, ApiKey, and TimeoutSeconds, plus a derived IsConfigured flag that indicates whether an API key has been supplied. Use this class to configure the Brave search provider from your configuration (environment variables, user secrets, appsettings.json, etc.). The BaseUrl must end with a trailing slash to ensure correct URL composition to /res/v1/web/search; if ApiKey is empty, the search tool will report unconfigured instead of attempting requests.

## Remarks
BraveSearchOptions participates in the application's configuration binding as a concrete [`IConfigSection<BraveSearchOptions>`](IConfigSection.cs.md). It centralizes Brave search settings, letting code read a single, strongly-typed object instead of scattering constants and strings. The static SectionName guides the binder to the Tools:Web:Brave section; the default values enable a safe, testable configuration out of the box.

## Notes
- Trailing slash in BaseUrl is required to form the correct endpoint; omitting it will cause the final request path to be misassembled.
- ApiKey should be stored securely and supplied via environment variables or a secret store; avoid logging or emitting it.
- IsConfigured is derived from ApiKey; avoid binding to this property or persisting a separate value.