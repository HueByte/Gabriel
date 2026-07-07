# LoginPage.tsx

> **Source:** `src/webapp/src/pages/LoginPage.tsx`

## Contents

- [LocationState](#locationstate)
- [LoginPage](#loginpage)

---

## LocationState
> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** interface

```typescript
interface LocationState
```


LocationState defines the minimal shape of a navigation state used when routing to a login page. Its from field, if provided, carries the originating pathname so the app can redirect back after a successful login. This lightweight type keeps the login flow decoupled from a concrete router implementation while expressing exactly which piece of state is needed for post-authentication navigation.

## Remarks
This interface encapsulates a common pattern in guarded routes: capture where the user came from without coupling to a specific routing object. By modeling only an optional from with an optional pathname, it allows callers to pass through arbitrary routing state while a login page can still determine a sensible redirect. It sits between the navigation plumbing and the UI, enabling a smooth user experience without leaking router internals into business logic.

## Example
```typescript
// Example usage with a router that stores the previous path in location.state
function loginRedirect(state?: LocationState) {
  const path = state?.from?.pathname ?? '/';
  // navigate to `path` after login
}
```

## Notes
- Guard against undefineds with optional chaining when reading `state?.from?.pathname`.
- When persisting or sending this state across boundaries, ensure all values are JSON-serializable (pathname is a string).
- If you extend this interface with additional fields, update all call sites to reflect the new shape and keep the interface minimal for routing concerns.

---

## LoginPage
> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** function

```typescript
export function LoginPage()
```


LoginPage renders the sign-in screen for Gabriel, handling email/password input, password visibility, and submission flow. It uses useAuth to perform authentication and React Router to redirect back to the originally requested URL (or '/' as a fallback) after a successful login, while managing busy and error states to prevent concurrent submissions and surface failures to the user.

## Remarks
It acts as a presentational and orchestrator component: it delegates authentication to useAuth, orchestrates navigation with useNavigate/useLocation, and coordinates simple UI state. It sits at the boundary between protected routes and the rest of the app, providing a cohesive login UX while keeping business logic out of the UI layer.

## Example
```typescript
import { LoginPage } from './LoginPage';
import { Routes, Route } from 'react-router-dom';

<Routes>
  <Route path="/login" element={<LoginPage />} />
</Routes>
```

## Notes
- The redirect target is read from location.state.from; if the original route doesn't set it, the user is redirected to '/' by default. Ensure any protected route passes the intended destination.
- The avatar seed is cosmetic and does not persist across sessions; re-rolls on each visit.


---