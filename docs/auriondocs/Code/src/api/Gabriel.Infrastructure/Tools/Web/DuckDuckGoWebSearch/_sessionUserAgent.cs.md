This private, nullable string field caches the User-Agent header used by the DuckDuckGo web search component during a session. It backs the header construction for HTTP requests, ensuring consistent identity across calls within the same session. It may be null if the session has not yet initialized a user agent; in that case, a default user agent is typically used.

## Remarks
Having a dedicated session User-Agent field centralizes header management and makes testing easier by allowing controlled injection of a known user agent. It also isolates the header value from the rest of the request-building logic, reducing duplication across HTTP request construction.

## Notes
- Nullable: be prepared for null and provide a default or initialization path before issuing requests.
- As an instance field, ensure thread-safety assumptions align with how the class is used; concurrent mutations may require synchronization or a single-threaded usage pattern.
- Do not rely on this field being initialized by external callers since it is private.