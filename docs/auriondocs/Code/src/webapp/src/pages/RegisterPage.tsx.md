# RegisterPage

> **File:** `src/webapp/src/pages/RegisterPage.tsx`  
> **Kind:** function

```typescript
export function RegisterPage()
```


RegisterPage is a React function component that renders a self-contained registration screen for creating a new user account. It presents a form with email and password fields, a visual avatar preview seeded for each visit, a password visibility toggle, and a responsive submit experience that prevents duplicate submissions. On successful registration, it redirects to the home page; on failure, it displays an inline error message.

## Remarks
This component encapsulates the onboarding UI and delegates authentication to a hook, keeping the UI and business logic loosely coupled. The avatar seed is purely cosmetic and refreshed on each visit to create a lively feel, while the actual account seed is assigned server-side during registration. The busy state and input disabling ensure idempotent interactions, and accessibility considerations (aria-labels and role="alert" for errors) improve usability for assistive technologies. This pattern makes it straightforward to swap out the authentication mechanism without altering the registration UI.

## Example
```typescript
import React from 'react';
import { RegisterPage } from './src/webapp/src/pages/RegisterPage';

export function App() {
  return (
    <div>
      <RegisterPage />
    </div>
  );
}
```

## Notes
- The avatar seed is regenerated on mount and can be re-rerolled via the UI; the server assigns the real avatar seed during registration. 
- The form fields are marked required and the submit button is disabled while busy or when inputs are empty, preventing accidental submissions.
- The error handling displays a concise message and leaves the form usable for retry without a full page reload.