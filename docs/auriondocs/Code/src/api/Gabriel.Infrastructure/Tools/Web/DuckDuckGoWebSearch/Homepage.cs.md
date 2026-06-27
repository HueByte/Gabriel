Constant string holding the DuckDuckGo base URL (https://duckduckgo.com/). The containing class uses this value as the root URI when constructing web search requests.

## Remarks
Centralizes the provider's base URI so the homepage address is defined in a single place rather than scattered string literals. Because it is a private compile-time constant, it is intended only for use inside the containing class and simplifies updates when the provider's root URL must change.

## Notes
- The value includes a trailing slash; take care when concatenating paths or query strings to avoid producing double slashes.
- This is a hard-coded, compile-time constant — changing it requires recompiling the assembly and is not configurable at runtime.