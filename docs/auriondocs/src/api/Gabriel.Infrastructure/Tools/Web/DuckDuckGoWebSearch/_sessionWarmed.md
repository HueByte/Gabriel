Tracks whether the DuckDuckGo web session has been "warmed" (i.e., any one-time initialization or priming request has completed). Developers rely on this flag internally to avoid repeating warm-up work before sending search requests.

## Remarks
This private flag records that the instance has already performed its session warm-up (for example: initial requests to establish cookies, headers, or other state). It exists to prevent redundant initialization logic when the search client is reused across multiple operations.

## Notes
- Defaults to false (session not warmed). The code that performs warm-up is expected to set this to true once complete.
- The field itself is not synchronized; if the search client can be used concurrently from multiple threads, callers or the implementation should ensure proper synchronization (e.g., use locks or a volatile field) to avoid races.