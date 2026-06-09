Holds an IHttpClientFactory instance used by this class to obtain HttpClient objects for outbound HTTP requests. Use this factory (rather than new-ing HttpClient) when the class needs to perform web calls so callers benefit from pooled connections, managed lifetimes, and centralized configuration.

## Remarks
This private, readonly field is intended to be assigned at construction time (typically via dependency injection) and reused throughout the class to create HttpClient instances as needed. Using IHttpClientFactory prevents socket exhaustion and lets callers configure named or typed clients centrally (timeouts, handlers, base addresses) instead of configuring each HttpClient inline.

## Notes
- Clients created by the factory should not be manually disposed; the factory manages their lifetimes.
- The field is private and readonly, so it should be assigned once (usually in the constructor). If it is not initialized, attempts to use it will result in a NullReferenceException.