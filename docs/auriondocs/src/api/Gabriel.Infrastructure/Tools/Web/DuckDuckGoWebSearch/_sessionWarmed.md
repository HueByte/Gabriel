Tracks whether the DuckDuckGo web search session has already been "warmed" (i.e. any required one-time initialization such as setting cookies, headers, or performing an initial request). This flag is used internally to avoid repeating the warm-up work on subsequent operations.

## Remarks
This private field exists to gate one-time initialization logic inside DuckDuckGoWebSearch. By checking this flag callers within the class can skip expensive or side-effecting setup after it has already run. Because it is an implementation detail it is not exposed outside the class and should only be mutated by the warm-up logic.

## Notes
- Defaults to false; becomes true once the warming path completes.
- Not inherently thread-safe — concurrent callers must use the same synchronization the class employs when checking or setting this flag to avoid race conditions.