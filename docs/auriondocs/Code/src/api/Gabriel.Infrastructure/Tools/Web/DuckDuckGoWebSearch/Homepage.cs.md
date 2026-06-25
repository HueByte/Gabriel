Private constant that holds the DuckDuckGo homepage URL used by the web search logic to assemble requests. A developer would update this single value rather than changing multiple string literals spread through the class.

## Remarks
Centralizing the URL reduces duplication and the risk of inconsistent endpoints across request-building code. Because it is private, the detail remains an implementation concern, keeping the public API clean and ensuring callers cannot depend on the internal URL. The const nature also communicates that this value is fundamental to the class behavior and should remain unchanged at runtime.

## Notes
- Being const means the value is baked into the assembly; changing it requires recompiling the code that uses it.
- If you ever need environment-specific endpoints, replace this with a configurable value rather than a const.
- As it's private, external tests can't reference the value directly; validate behavior via public methods instead.