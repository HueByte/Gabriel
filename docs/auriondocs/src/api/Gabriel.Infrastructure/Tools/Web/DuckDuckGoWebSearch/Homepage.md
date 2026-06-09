Base URL for DuckDuckGo web searches used by this class. Reach for this constant when building request URLs for DuckDuckGo within the containing class to avoid repeating the literal and to make the base address easy to change in one place.

## Remarks
This private compile-time constant centralizes the DuckDuckGo homepage/base endpoint so the rest of the class can compose search or query URLs without embedding the raw string in multiple places. Keeping it private emphasizes that it's an implementation detail of the surrounding class rather than a public configuration.

## Example
```csharp
// common usage inside the same class
var query = "open source search";
var requestUrl = Homepage + "?q=" + Uri.EscapeDataString(query);
// requestUrl -> "https://duckduckgo.com/?q=open%20source%20search"
```

## Notes
- Because this is a const, its value is inlined at compile time; changing it requires recompiling the containing assembly.
- The constant includes a trailing slash ("https://duckduckgo.com/"); take care to avoid duplicating slashes when concatenating paths or query fragments.