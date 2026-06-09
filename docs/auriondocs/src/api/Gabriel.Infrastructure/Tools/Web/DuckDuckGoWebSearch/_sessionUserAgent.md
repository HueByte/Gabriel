Stores the session-specific User-Agent header value used by the DuckDuckGoWebSearch implementation. This private, nullable field acts as an internal cache for the User-Agent string that the class applies to outgoing HTTP requests; callers should use the public methods of the class rather than accessing this field directly.

## Remarks
This field exists to keep a consistent User-Agent for the lifetime of a search session and to avoid recomputing or reselecting the header value on every request. Because it is private, it is managed by the DuckDuckGoWebSearch class itself and is not part of the public API surface.

## Notes
- The field is nullable; callers inside the class must handle the null case (compute or supply a default User-Agent) before using it.
- Access to this field is not synchronized by itself. In multi-threaded scenarios the class should ensure proper synchronization if multiple threads may read or write the value concurrently.