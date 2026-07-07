# BraveSearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`  
> **Kind:** class

```csharp
public class BraveSearchOptions : IConfigSection<BraveSearchOptions>
```


BraveSearchOptions is a configuration container that holds the settings required to configure Brave's web search integration. It participates in the app's configuration system via IConfigSection and exposes BaseUrl, ApiKey, TimeoutSeconds, and a computed IsConfigured flag used to enable or disable the Brave search feature at runtime.

## Remarks

These settings centralize the Brave Search integration behind a simple, strongly-typed surface. The SectionName identifies the configuration key used by the framework to bind configuration data. IsConfigured provides a clear runtime check before attempting to use the Brave API. Note that the BaseUrl must end with a trailing slash to ensure the final endpoint resolves to /res/v1/web/search when concatenated with a relative path, and an empty ApiKey disables the feature gracefully instead of throwing.

## Notes

- The ApiKey default is empty; always guard with IsConfigured to avoid unconfigured Brave API errors.