# installAuthInterceptor

> **File:** `src/webapp/src/api/authInterceptor.ts`  
> **Kind:** function

```typescript
export function installAuthInterceptor()
```


Installs a global Axios response interceptor that automatically refreshes the user’s session when a 401 Unauthorized response is encountered and retries the original request if the refresh succeeds. It avoids refreshing authentication endpoints to prevent loops, and signals session expiry if the refresh fails or if a retried request still returns 401.

## Remarks
This symbol centralizes the session-renewal flow so individual API calls don’t duplicate refresh logic. It coordinates with refreshSession to obtain a new access token and with signalSessionExpired to trigger logout and client-state cleanup when recovery is not possible. It guards against refresh loops by skipping known auth-endpoints and by marking retried requests with a _retried flag to ensure a single retry attempt per request.

## Notes
- Install guard: calling installAuthInterceptor more than once will attach multiple interceptors; call once during app startup.
- Mutation: the code writes a non-standard _retried property on the request config; ensure your TypeScript typings allow augmenting AxiosRequestConfig or avoid strict type constraints where needed.
- Logout trigger: when the refresh cannot redeem tokens, signalSessionExpired is invoked to perform logout cleanup on both client and server sides.