# RegisterPage

> **File:** `src/webapp/src/pages/RegisterPage.tsx`  
> **Kind:** function

Renders the registration page: a form with email, password, an avatar preview that can be rerolled, and client-side handling of account creation via the app's useAuth.register function. Use this component as the register route or page when you want a full registration UX that disables inputs while a request is in-flight and navigates to the app root on success.

## Remarks
This is a small, self-contained page component that wires UI state (email, password, password visibility, avatar seed, busy/error states) to the authentication layer via useAuth(). It visually rerolls a preview avatar on each mount and when the user clicks the reroll button, but the comment in-source notes that the final avatar seed for the created account is assigned server-side. The component prevents duplicate submissions by using a busy flag, shows errors returned from the register call, and redirects with react-router's navigate on successful registration.

## Notes
- The password input enforces a minLength in the browser but that is client-side only; server-side validation is still required.
- Error text shown comes from the thrown value's message when it is an Error; non-Error rejections produce a generic "Registration failed." message.
- The password visibility toggle sets tabIndex={-1}, which prevents keyboard users from tab-focusing that control — review if keyboard accessibility is required.