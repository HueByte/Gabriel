# AuthController

> **File:** `src/api/Gabriel.API/Controllers/AuthController.cs`  
> **Kind:** class

A single authentication surface that implements the application's auth-related HTTP endpoints (register, login, refresh, logout, revoke, revoke-all, and me). Reach for this controller when you need the canonical server-side flows for creating accounts, issuing and rotating JWT pairs, setting/clearing HttpOnly cookies for browser clients, and exposing token responses for external clients that cannot rely on cookies.

## Remarks
This controller centralizes both browser and API client authentication: endpoints that create or rotate tokens set HttpOnly cookies and also return token information in the response body so the same endpoint can serve web apps (which rely on cookies) and external clients (which use the body). Registration is guarded by a per-request feature flag (AuthOptions.RegistrationEnabled) so the operator can disable new signups without restarting the app. Identity-related failures during registration are surfaced as domain-level errors so callers can observe validation problems (password rules, duplicate email, etc.).

## Notes
- Registration can be globally disabled at runtime by toggling AuthOptions.RegistrationEnabled; the controller checks this on every request.
- When user creation fails, Identity errors are aggregated and thrown as a DomainException (the application’s error-handling middleware determines the final HTTP translation).
- Login uses SignInManager.CheckPasswordSignInAsync, which updates Identity lockout counters on failures; repeated failed attempts can lock an account.
- Endpoints that mint/rotate tokens both set HttpOnly cookies and return token data in the response body so browser and non-browser clients can be supported by the same API surface.