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


LocationState is a tiny, typed carrier for navigation state used during routing flows. It carries an optional from object that may include a pathname, allowing a caller to remember where the user navigated from (for example, to return after login).

## Remarks
By isolating this into a dedicated interface, the codebase can pass around a minimal, well-typed hint about the origin without coupling to full location objects. It helps separate concerns between page components and the router, enabling simpler redirects to authentication or gated routes while preserving a sensible back-navigation target.

## Example
```typescript
const example: LocationState = { from: { pathname: '/dashboard' } };

function determineReturnPath(state?: LocationState): string {
  return state?.from?.pathname ?? '/';
}

console.log(determineReturnPath(example)); // '/dashboard'
```

## Notes
- The from and its pathname are both optional; check for undefined before reading.
- This interface models only a small slice of routing state; do not assume more fields exist.

---

## LoginPage
> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** function

```typescript
export function LoginPage()
```


LoginPage is the sign-in screen used when accessing protected routes. It renders a login form, submits credentials through the authentication context, and returns the user to the original destination (or '/' as a fallback) upon success; it also features a visual avatar seed that refreshes on each visit for a lively touch.

## Remarks
LoginPage orchestrates the authentication flow by reading the intended destination from location.state and by invoking login(email, password) from the authentication hook. After a successful login, it navigates back to the originally requested URL, preserving the user experience across protected routes. The avatar seed is strictly visual and does not persist across sessions; it is re-seeded on mount and can be re-rerolled via the UI to keep the login screen feeling dynamic. A busy state gates the UI to prevent duplicate submissions and to surface errors clearly.

## Notes
- The redirect target is derived from location.state.from.pathname with a fallback to '/'; if ProtectedRoute doesn't stash a target, login lands at the root.
- The avatar seed is ephemeral and re-rolls through the provided button; it does not influence authentication or persistence.
- The UI disables inputs and the submit button while a login attempt is in flight to prevent concurrent submissions, and displays an inline error message on failure.


---