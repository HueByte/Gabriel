# installAuthInterceptor

> **File:** `src/webapp/src/api/authInterceptor.ts`  
> **Kind:** function

Installs a global axios response interceptor that handles 401 Unauthorized responses by attempting a session refresh and retrying the failed request. Use this during application startup (or when your HTTP client is configured) so that token refresh and automatic request retry happen transparently instead of surfacing a broken session to callers.

## Remarks
This function centralizes the logic for recovering from expired access tokens: it ignores auth-related endpoints (to avoid refresh loops), performs a single refresh attempt per failed request (marking the original request with _retried), and signals an unrecoverable session when refresh cannot recover the request. It keeps retry logic out of individual API calls and delegates session-expiration handling to the global signalSessionExpired/authorization machinery.

## Example
```typescript
// Call once during app initialization after axios/global configuration is ready
import { installAuthInterceptor } from './api/authInterceptor';

installAuthInterceptor();
```

## Notes
- Calling this more than once will register multiple interceptors and may cause duplicate refresh attempts — register it exactly once (typically at app startup).
- The interceptor attaches a non-standard _retried flag to axios' request config to prevent infinite retry loops; this relies on the request config being mutable.
- Simultaneous 401 responses from multiple in-flight requests may still trigger multiple concurrent refresh attempts; the function does not queue or de-duplicate concurrent refreshes.
- The interceptor ignores requests whose URL starts with any pattern in AUTH_PATHS to avoid trying to refresh while already on auth endpoints; ensure AUTH_PATHS entries match the request URL format used by your app (relative vs absolute).
- The interceptor is registered on the axios instance imported in this module; if you use a custom axios instance, adapt this installer to register against that instance instead of the global one.