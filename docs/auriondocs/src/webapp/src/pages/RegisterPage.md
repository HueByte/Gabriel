# RegisterPage

> **File:** `src/webapp/src/pages/RegisterPage.tsx`  
> **Kind:** function

Renders the user registration screen: collects email and password, shows a randomly generated avatar (with a reroll control), and calls the authentication provider to create an account. On successful registration it navigates to the app root; while submitting it disables inputs and shows a busy state.

## Remarks
This component is a UI-level wrapper around the app's auth and navigation primitives (it uses the authentication hook's register function and react-router's navigate). It manages local form state (email, password, password visibility), a visual avatar seed that is rerolled on mount and on demand, and simple client-side validation (required fields and a minimum password length). Error messages from the registration attempt are surfaced in an accessible alert region.

## Notes
- The avatar seed shown is purely visual; the server assigns the actual avatar seed for the new account during registration.
- The password visibility toggle uses tabIndex={-1}, so it is not reachable via keyboard tabbing — keyboard users may not be able to focus it unless a pointer event is used. Consider removing tabIndex or providing an alternative keyboard-accessible control if that is unintended.
- Form submission is guarded by a local busy flag to prevent duplicate requests; submit button is disabled when busy or when required fields are empty.
- Errors thrown by register are displayed using the thrown Error's message when available, otherwise a generic "Registration failed." message is shown.