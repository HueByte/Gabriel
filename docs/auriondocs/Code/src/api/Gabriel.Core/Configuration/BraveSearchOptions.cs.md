# BraveSearchOptions

> **File:** `src/api/Gabriel.Core/Configuration/BraveSearchOptions.cs`  
> **Kind:** class

```csharp
public class BraveSearchOptions : IConfigSection<BraveSearchOptions>
```


BraveSearchOptions is a strongly-typed configuration container for Brave Web Search integration. It implements IConfigSection&lt;BraveSearchOptions&gt; and exposes the Brave Search API base URL (BaseUrl), the subscription key (ApiKey), and the request timeout (TimeoutSeconds) used by the Brave Web Search client. The SectionName is the configuration key "Tools:Web:Brave" that identifies this section in the application's configuration. ApiKey is optional; when left empty Brave Search is effectively disabled and will report an unconfigured state rather than failing. The IsConfigured property returns true when ApiKey contains a non-whitespace value. The default BaseUrl ends with a trailing slash to ensure correct URL composition when concatenating with relative paths (the relative path lands at /res/v1/web/search).

