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


Represents the shape of a router/location state object used by the login page: an optional from object that may contain an optional pathname string. Use this interface to type location.state (for example with react-router) when you want to carry an origin or post-login redirect path.

## Remarks
This simple abstraction documents the minimal payload the login flow expects from navigation state: an optional origin route (from.pathname) to return the user after successful authentication. Typing the location state with this interface makes access safer and clarifies the intended contract between the route that redirects to the login page and the login handler.

## Example
```typescript
import { useLocation, useNavigate } from 'react-router-dom';

const location = useLocation<LocationState>();
const navigate = useNavigate();

const from = location.state?.from?.pathname ?? '/';

function onLoginSuccess() {
  // Redirect back to the originating page or to a default
  navigate(from, { replace: true });
}
```

## Notes
- Both from and pathname are optional; guard access with optional chaining or provide a fallback.
- The location state may be undefined when the user navigates directly to the login page (no prior redirect).

---

## LoginPage

> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** function

Renders the application's sign-in screen: an email/password form with a visual avatar that can be rerolled, a password visibility toggle, client-side busy/error states, and post-login navigation back to the originally requested route (if provided). Use this component for the app's login route or wherever you need a standard sign-in experience wired to the app's auth flow.

## Remarks
This component coordinates authentication (via useAuth().login), routing (useNavigate/useLocation) and a small visual flourish (a seeded Avatar that is rerolled on each visit or when the user clicks the reroll button). If a ProtectedRoute redirected the user to the login page it is expected to stash the original destination on location.state.from; after a successful login LoginPage navigates back to that path (falls back to "/"). It also manages a busy flag to prevent duplicate submissions and surfaces server or thrown errors to the user.

## Example
```typescript
// In your route definitions
import { Route } from 'react-router-dom';
import { LoginPage } from './pages/LoginPage';

<Route path="/login" element={<LoginPage />} />
```

## Notes
- The component reads the return path from location.state.from; if no value is present the user is redirected to "/" after sign-in.
- The displayed error message is the thrown Error.message when login fails; this can surface backend messages — consider sanitising or mapping errors if you need different user-facing text.
- The password visibility toggle is rendered with tabIndex={-1}, so it is skipped by tab navigation; this affects keyboard accessibility and may be intentional or require change depending on your accessibility policy.


---