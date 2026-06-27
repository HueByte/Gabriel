An async-friendly mutex (SemaphoreSlim initialized to a single slot) used to serialize first-use session initialization for the DuckDuckGo web search client. It prevents concurrent callers from performing the homepage "pre-warm" and from racing when committing the per-session User-Agent, ensuring the HttpClientHandler's CookieContainer and session-related state are only populated once per handler generation.

## Remarks
This lock exists because the CookieContainer is stored on the HttpClientHandler (configured elsewhere); multiple callers racing to perform the homepage pre-warm could cause duplicate HTTP requests, inconsistent cookie state, or conflicting User-Agent choices. The SemaphoreSlim provides a lightweight, awaitable synchronization primitive so callers can asynchronously wait for the first initialization to complete instead of blocking threads.

## Notes
- Always use the async APIs (WaitAsync) and release the semaphore in a finally block to avoid deadlocks if an exception occurs.
- Prefer awaiting the semaphore instead of using synchronous Wait to avoid thread-pool starvation or blocking thread-pool threads.
- If the containing type implements disposal, ensure the SemaphoreSlim is disposed when the parent is disposed to free native resources.