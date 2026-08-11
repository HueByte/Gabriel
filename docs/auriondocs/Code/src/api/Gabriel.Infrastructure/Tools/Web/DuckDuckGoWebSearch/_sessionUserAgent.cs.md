Stores the User-Agent string for the current session. The field is nullable to indicate that it may not be initialized yet and will be populated lazily when first needed. By caching the User-Agent, subsequent HTTP requests made by the DuckDuckGo web search functionality can reuse the same header, avoiding repeated computation or retrieval.

## Remarks
This field implements a lazy-cache pattern: compute or obtain the User-Agent once and reuse it for the lifetime of the owning object. It helps ensure consistent request headers and reduces overhead compared to recomputing the UA for every request. If the instance may be accessed from multiple threads, ensure that initialization is thread-safe to prevent race conditions.

## Notes
- Nullability: the value may be null; code that reads it should either initialize it first or handle nulls gracefully.
- Thread-safety: lazy initialization, if used, should be synchronized to avoid race conditions.
- Mutability: changing the value after initial assignment can lead to inconsistent behavior across requests; prefer a single initialization point.