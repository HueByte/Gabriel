# installAuthInterceptor

> **File:** `src/webapp/src/api/authInterceptor.ts`  
> **Kind:** function

```typescript
export function installAuthInterceptor()
```


Installs a global Axios response interceptor that automatically refreshes the authentication session when a 401 Unauthorized response is received, then retries the original request. It avoids refreshing during authentication calls to prevent a loop, and if the refresh cannot recover the session (or the request has already been retried once), it signals session expiry so the AuthContext can perform a logout cleanup.

## Remarks
Centralizes the session refresh flow in one place, so individual API calls don't have to duplicate retry logic. It uses AUTH_PATHS to detect calls that target authentication endpoints, and it coordinates with refreshSession and signalSessionExpired to drive the client logout workflow. The per-request _retried flag prevents infinite retry loops, while a failed refresh leads to a clean logout by signaling session expiry. In multi-request scenarios, overlapping 401s may trigger concurrent refresh attempts unless a higher-level coordination is introduced; consider centralizing refresh state if that becomes a concern.

## Example
```ts
// Initialize the interceptor once during app bootstrap
installAuthInterceptor();

// Any subsequent API call to a protected endpoint will automatically refresh
// the session on 401 and retry the original request when possible.
axios.get('/api/protected/resource')
  .then(res => { /* handle success */ })
  .catch(err => { /* handle error, e.g. show login */ });
```

## Notes
- Refreshing is attempted only for 401 responses that are not targeting authentication endpoints (to avoid refresh loops). Ensure AUTH_PATHS is properly configured to cover all token-refresh triggers.
- If refreshSession rejects or throws, the interceptor will surface the error; ensure you handle failures gracefully in your UI and state management.
- Multiple 401s arriving in parallel can trigger multiple refresh attempts; if this becomes undesirable, consider introducing a shared in-flight refresh mechanism or locking strategy.