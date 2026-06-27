Stores the per-instance session User-Agent string used by DuckDuckGoWebSearch when constructing HTTP requests. The field is nullable and is expected to be populated by the containing class before it is read.

## Remarks
This private backing field centralizes the User-Agent value for the lifetime of a DuckDuckGoWebSearch instance so the value can be reused across multiple requests (avoiding repeated computation or lookups) and kept consistent for the session. Because it is private, callers interact with it only indirectly via the class's request-construction logic.

## Notes
- The field is nullable (string?), so callers inside the class should ensure it is initialized or guard reads with a null check.
- Access to this field is not synchronized; if the containing class is used concurrently from multiple threads, initialization or reads may require external synchronization to avoid race conditions.
- Changing the value will affect subsequent requests that use it as the User-Agent header.