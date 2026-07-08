# RegisterPage

> **File:** `src/webapp/src/pages/RegisterPage.tsx`  
> **Kind:** function

```typescript
export function RegisterPage()
```


RegisterPage is a React functional component that renders a complete user registration screen and wires it into the app's authentication flow. It manages local UI state for email, password, password visibility, and loading status, and on submission it delegates to the authentication hook's register(email, password) method, then navigates to the home page on success; if registration fails, it surfaces an error message to the user. The UI includes a visual avatar seeded by a numeric seed, which is rerolled on mount and on demand to keep the screen feeling alive until the server assigns the real avatar during registration.

## Remarks
RegisterPage encapsulates the presentation layer of the signup flow, coordinating form state, validation cues, and routing while keeping the actual registration logic inside the useAuth hook. This separation supports swapping authentication strategies or reusing the same signup UI in different contexts without touching business logic. The seed-based avatar is a purely visual flourish used to lend liveliness to the screen, with the server finalizing the avatar on account creation.

## Notes
- Busy state disables inputs and the avatar reroll button to prevent duplicate submissions.
- Errors are surfaced in a dedicated alert area with an icon to ensure visibility.
- The password field enforces a minimum length and includes a toggle to show/hide the password for usability.
- The submit button indicates progress with a spinner and a localized label while creation is in progress.