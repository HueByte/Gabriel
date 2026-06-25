Stores the canonical DuckDuckGo homepage URL as a private, compile-time constant used by the DuckDuckGoWebSearch implementation. By centralizing the endpoint in a single, immutable value, it avoids string duplication and typos when constructing requests or navigation logic inside the class.

## Remarks
Private constants like this act as a small, self-contained configuration detail that the class relies on to form complete requests to the DuckDuckGo homepage. It helps keep the code readable by giving a named, obvious endpoint instead of scattering the literal URL across methods.

## Example
```csharp
// Example: using the constant to create a request to the homepage
var request = new HttpRequestMessage(HttpMethod.Get, Homepage);
```

## Notes
- This value is baked into the consuming assembly; changing it requires recompilation of dependents.
- Being private means external code cannot reference it directly; if you need sharing or testing access, consider exposing an internal or public accessor or injecting the value via configuration.