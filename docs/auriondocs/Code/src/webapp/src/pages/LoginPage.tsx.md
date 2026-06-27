# LoginPage.tsx

> **Source:** `src/webapp/src/pages/LoginPage.tsx`

## Contents

- [LocationState](#locationstate)
- [LoginPage](#loginpage)

---

## LocationState

> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** interface

Represents the optional shape of navigation state used by the login page (or similar redirect flows). It models a possible `from` location with an optional `pathname` string — typically used to remember which route a user attempted to access before being redirected to a login screen.

## Remarks
This interface is a small, targeted type for `location.state` values commonly written and read by authentication redirect logic. It exists to give a concrete shape to the loosely-typed `state` payload provided by React Router (or similar routing libraries), so callers can safely access `from.pathname` when computing where to send the user after a successful login.

## Example
```typescript
// Example: compute post-login redirect inside a React component
import { useLocation, useNavigate } from 'react-router-dom';

const location = useLocation();
const navigate = useNavigate();

const state = location.state as LocationState | undefined;
const redirectTo = state?.from?.pathname ?? '/';

// After successful login:
navigate(redirectTo);
```

## Notes
- `location.state` (and thus this interface) may be undefined — always guard before accessing `from` or `pathname`.
- `pathname` itself is optional; provide a sensible fallback (e.g., '/') to avoid navigating to `undefined`.
- `location.state` is not part of the URL and is not preserved across full page reloads or external navigation; do not rely on it for long-term persistence.


---

## LoginPage

> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** function

Renders the application's sign-in screen and handles the interactive login flow — collecting email and password, calling the auth provider, showing a seeded avatar with a reroll button, and redirecting back to the originally requested page after successful authentication. Use this component as the route-level login page when you need a full-featured, accessible sign-in UI that integrates with the app's ProtectedRoute and useAuth hook.

## Remarks
This component ties together authentication and navigation concerns: it uses the app's login function (via useAuth) to perform sign-in, reads location.state.from to determine where to redirect after success, and calls navigate(from, { replace: true }) so users return to their intended page without leaving a login entry in the history stack. The avatar seed is purely visual and is rerolled on mount and when the user clicks the reroll button; the value is not persisted. Inputs are disabled while a login attempt is in progress and errors from the login call are surfaced in an inline alert.

## Example
```typescript
// Typical usage with React Router v6
import { Route, Routes } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';
import { ProtectedRoute } from './components/ProtectedRoute';

// Route configuration
<Routes>
  <Route path="/login" element={<LoginPage />} />
  <Route
    path="/app"
    element={
      <ProtectedRoute>
        <App />
      </ProtectedRoute>
    }
  />
</Routes>

// ProtectedRoute should redirect unauthenticated users like:
// navigate('/login', { state: { from: location } });
```

## Notes
- While a login attempt is in progress (busy state) inputs, reroll and submit buttons are disabled to prevent duplicate submissions.
- The component sets the error message from the thrown value if it is an Error; otherwise it falls back to a generic "Login failed." message — be aware this may surface backend errors directly if the login function throws descriptive Error messages.
- The password visibility toggle uses tabIndex={-1}, making it not focusable via keyboard tabbing; this is intentional in the UI but could affect discoverability for some keyboard users.
- If no redirect location was provided (location.state.from missing), the component falls back to "/" as the post-login destination.


---