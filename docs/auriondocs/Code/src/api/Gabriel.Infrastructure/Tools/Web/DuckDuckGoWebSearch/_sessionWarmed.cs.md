This private boolean flag indicates whether the DuckDuckGo web search session has completed its one-time warm-up. It is used internally to guard initialization logic so that expensive setup steps (such as establishing a session, preloading state, or configuring headers) are performed only once per instance. After warming up, the flag is set to true to allow subsequent operations to reuse the prepared session state without repeating the setup.

## Remarks
Encapsulating the warm-up state in this field keeps the initialization flow isolated from public behavior; it prevents repeated initialization and helps keep the object's lifecycle predictable across multiple calls to the search functionality.

## Notes
- Default value is false; unless the field is set during construction or warm-up, _sessionWarmed starts as false.
- Not inherently thread-safe; if the object is used concurrently from multiple threads, reads and writes to this flag may race unless proper synchronization is applied.