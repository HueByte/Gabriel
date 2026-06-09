Base URL for the DuckDuckGo website used internally by DuckDuckGoWebSearch when building web search requests. Use this constant whenever the class needs the service's root address rather than hard-coding the string in multiple places.

## Remarks
Centralizes the service root URL to make updates easier and to keep calling code consistent. The value includes a trailing slash; callers that append paths or query strings should account for that to avoid accidental double slashes. This field represents the homepage/root URL, not a specific search API endpoint — query parameters or path segments must be appended by the caller.

## Notes
- The field is private and intended for internal use only.
- It's a compile-time constant: changing it requires recompiling the assembly.
- The trailing slash is significant when concatenating; trim or handle slashes explicitly to avoid malformed URLs.