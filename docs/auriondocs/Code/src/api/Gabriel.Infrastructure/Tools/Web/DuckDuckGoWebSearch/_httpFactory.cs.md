Holds the IHttpClientFactory used to create HttpClient instances for outbound HTTP calls (e.g., DuckDuckGo web search requests). Injected via DI and assigned once in the constructor, it lets you obtain configured HttpClient instances on demand (via CreateClient) rather than constructing HttpClient directly.

## Remarks
By abstracting HttpClient creation behind IHttpClientFactory, this field centralizes HTTP configuration, lifetimes, and resilience policies, reducing socket exhaustion and fragmentation. It also improves testability by allowing tests to inject a mock or stub factory that supplies controllable clients. When using multiple external endpoints, you can configure named clients (for example, 'DuckDuckGo') so each client carries tailored timeouts and base addresses without duplicating configuration at the call sites.

## Notes
- Do not hold onto HttpClient instances created by the factory; obtain a client per operation via _httpFactory.CreateClient(...) and dispose when appropriate.
- Ensure HttpClientFactory is properly configured in DI (named or default client) so that CreateClient returns a correctly configured HttpClient.