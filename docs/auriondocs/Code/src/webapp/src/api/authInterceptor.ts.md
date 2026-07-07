# installAuthInterceptor

> **File:** `src/webapp/src/api/authInterceptor.ts`  
> **Kind:** function

```typescript
export function installAuthInterceptor()
```


installAuthInterceptor wires an Axios response interceptor to transparently refresh an expired access token when the server responds with 401, but only for non-auth endpoints. It compares the request URL against AUTH_PATHS and will skip refresh attempts for authentication calls to avoid a refresh loop. When a 401 occurs and a refresh is possible, it triggers refreshSession(), marks the original request as retried to prevent infinite retries, and then retries the original request with axios(original). If the refresh fails or the request has already been retried, it signals session expiration via signalSessionExpired() to drive the logout flow and rejects the error so the error propagates to callers. This function is a side-effect initializer; call it during application startup to install the interceptor.

## Remarks
By centralizing the token-refresh behavior, this interceptor keeps authentication concerns out of individual API calls and ensures a consistent logout path when tokens are no longer usable. It cooperates with AuthContext to clear local state and trigger logout when tokens are unusable, while preserving the user experience by transparently retrying a single refresh before failing.

## Notes
- Call installAuthInterceptor() once during application initialization; calling it multiple times will attach multiple interceptors unless guarded.
- Relies on AUTH_PATHS, refreshSession, and signalSessionExpired; misconfiguration may cause refresh to be skipped or fail unexpectedly.
- Designed for axios-based HTTP flows; adapting to a different HTTP client requires equivalent interceptor/wrapper logic.