Tracks whether this DuckDuckGoWebSearch instance has already performed its session warm-up steps. The field is false by default and is set to true once the class completes whatever initialization or "warm" request sequence it uses to prepare an HTTP session.

## Remarks
This private boolean exists to prevent repeated warm-up operations (for example, an initial probe request or other one-time setup) during the lifetime of the instance. Keeping the flag private ensures only the implementation controls when the session is considered warmed.

## Notes
- Defaults to false (unwarmed) until the class sets it to true.
- The field is not synchronized; if the containing class is accessed concurrently, callers should not assume warm-up is atomic or thread-safe unless the class provides its own synchronization.