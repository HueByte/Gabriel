A private constant holding the base URL for DuckDuckGo's "lite" search interface (https://lite.duckduckgo.com/lite/). The constant centralizes the endpoint used by the DuckDuckGoWebSearch implementation when composing HTTP requests to the lightweight DuckDuckGo search page, avoiding hard-coded strings scattered through the code.

## Remarks
This value is intentionally defined as a compile-time constant to ensure the endpoint is immutable and easily discoverable within the class. The "lite" endpoint typically serves a minimal HTML response suitable for simple scraping or low-bandwidth clients; keeping it in one place makes it straightforward to update if the project must switch to a different DuckDuckGo host or path.

## Notes
- The value includes a trailing slash; callers that append paths or query segments should avoid introducing duplicate slashes.
- Because the field is private, changing it requires a code change and rebuild; consider configuration if you need runtime-swappable endpoints.
- The availability, behavior, or URL of the DuckDuckGo lite interface is external and may change; handle network failures and unexpected HTML changes in the caller code.