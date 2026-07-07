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


LocationState describes the shape of the navigation-state you attach when redirecting to login to preserve the user's intended destination. It includes an optional from field, which itself may contain an optional pathname, signaling where the user wanted to go before authentication.

## Remarks
LocationState decouples routing intent from the login UI, enabling post-authentication redirects without forcing a full location object into the login page. It is a tiny, typed contract that makes the navigation context explicit for future maintenance. Because both from and pathname are optional, code consuming LocationState should defensively read these fields and provide sensible fallbacks.

## Example
```typescript
// Example usage showing a redirect-after-login state
const state: LocationState = { from: { pathname: '/dashboard' } };
```

## Notes
- Both from and pathname are optional; any consumer should guard reads and provide defaults.
- This interface describes a fragment of location state and is not a full routing object on its own.

---

## LoginPage
> **File:** `src/webapp/src/pages/LoginPage.tsx`  
> **Kind:** function

```typescript
export function LoginPage()
```


LoginPage is a React functional component that renders a complete sign-in form and coordinates the login flow using an authentication hook and router navigation. It redirects users to the originally requested URL after a successful login (or to '/' if none was saved), while providing input state, a password visibility toggle, a transient 'busy' state, and inline error feedback.

## Remarks
LoginPage centralizes the login UX for protected routes, isolating UI concerns from the authentication mechanism. It relies on a convention where a ProtectedRoute stores the target path in location.state.from, and then uses navigate(from, { replace: true }) to restore the user's navigation history in a non-disruptive way. The avatar seed is intentionally ephemeral and cosmetic to keep the login screen feeling fresh without persisting user state.

## Notes
- The busy flag disables inputs and the login button during submission to prevent duplicate requests.
- Error handling surfaces a message when login fails; the message is either the exception message or a generic 'Login failed.'
- The show/hide password toggle is accessible via a dedicated button with proper aria-label and title attributes; the password input switches between 'password' and 'text' types accordingly.

---