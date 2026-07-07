# RegisterPage

> **File:** `src/webapp/src/pages/RegisterPage.tsx`  
> **Kind:** function

```typescript
export function RegisterPage()
```


RegisterPage is a React function that renders the user registration screen and wires the form controls to local UI state. It presents email and password inputs, a live avatar preview with a seed that can be rerolled, and a password visibility toggle, while handling submission via the authentication context and routing on success. It guards against duplicate submissions with a busy flag and surfaces server or client errors inline.

## Remarks
RegisterPage encapsulates the sign-up UX and delegates authentication to a shared context, keeping API details out of the UI. The avatar seed is purely visual—server-side account avatar generation occurs after registration—so the reroll on each visit is a user-experience flourish rather than a identity cue. The component uses navigate with replace to prevent returning to the registration screen after a successful sign-up and disables inputs while processing to avoid duplicate requests.

## Notes
- The avatar seed is visual only; the real avatar seed is assigned server-side during registration.
- This component relies on the presence of the auth context providing a register function; without it, registration cannot complete.