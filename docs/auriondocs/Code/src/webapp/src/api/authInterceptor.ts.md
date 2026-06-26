# installAuthInterceptor

> **File:** `src/webapp/src/api/authInterceptor.ts`  
> **Kind:** function

Installs a global axios response interceptor that detects 401 Unauthorized responses and performs a refresh-token flow then retries the failed request. Call this once during application startup to enable transparent session refresh and centralized session-expiration signaling instead of handling 401s at every request site.

## Remarks
This function centralizes authentication error handling for outgoing HTTP requests: it ignores auth-related endpoints (to avoid refresh loops), marks requests it has retried to prevent infinite retry cycles, and delegates token refresh and session-expiration behavior to the surrounding authentication layer (e.g. AuthContext via refreshSession and signalSessionExpired). The interceptor treats the session as recoverable if refresh succeeds and as expired only when both the access attempt and the refresh attempt fail.

## Example
```typescript
import { installAuthInterceptor } from './api/authInterceptor';

// Install once during app initialization (before mounting your app)
installAuthInterceptor();

// Then render the application or create your router, etc.
```

## Notes
- The interceptor uses an ad-hoc `_retried` flag on axios request config to avoid retry loops; you may need to augment axios types in TypeScript if you reference this property elsewhere.
- If multiple requests fail with 401 at the same time this implementation may call refreshSession concurrently for each — consider deduping refresh attempts at a higher level if that is a concern.
- The retry issues the original request object as-is after refresh; ensure your refreshSession updates whatever Authorization header/store your requests rely on (for example axios defaults or the instance headers) so the retried request uses the refreshed credentials.