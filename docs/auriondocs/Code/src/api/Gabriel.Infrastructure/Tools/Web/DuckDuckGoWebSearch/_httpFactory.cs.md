This field holds a reference to the IHttpClientFactory used to create HttpClient instances for outbound HTTP calls made by this class. It is private and readonly, set once in the constructor and never reassigned, which makes HTTP client creation predictable and easier to reason about in tests. In the DuckDuckGoWebSearch workflow, the factory is used to obtain configured HttpClient instances rather than instantiating HttpClient directly.

## Remarks

HttpClientFactory centralizes HttpClient lifetimes, configuration, and resilience policies, reducing socket exhaustion and enabling named or typed clients. Keeping a factory reference in this field helps decouple HTTP concerns from business logic and makes the class easier to test by allowing injection of different client configurations or mocks.

## Notes

- Always obtain HttpClient via _httpFactory.CreateClient(...) instead of new HttpClient(...) to ensure proper pooling and lifetime management.
- Prefer named or typed clients to reuse configuration; dispose of the returned HttpClient when finished with the operation.