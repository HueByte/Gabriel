# IndexPage.tsx

> **Source:** `src/webapp/src/pages/IndexPage.tsx`

## Contents

- [IndexPage](#indexpage)
- [loadLastConversation](#loadlastconversation)

---

## IndexPage

> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

Renders a transient loading view and immediately redirects the user into a conversation. On first mount it checks for a previously opened conversation (via loadLastConversation); if found it navigates to that conversation. If none exists it creates a new conversation by POSTing to ConversationsService with a null title (the backend assigns a unique identifier) and then navigates to the new conversation's route.

## Remarks
This component is intended to be used as the application's index or root route so users are placed directly into a conversation rather than staying on a static landing page. The effect runs only once — ranBootRef guards against double invocation (useful in StrictMode or re-renders). Navigation uses replace: true to avoid leaving the index page in history, and encodeURIComponent is applied to conversation IDs to produce safe URLs.

## Example
```typescript
// react-router v6 route example
import { Route } from 'react-router-dom';
import { IndexPage } from './pages/IndexPage';

// in your router configuration
<Route path="/" element={<IndexPage />} />
```

## Notes
- Must be rendered inside a Router (useNavigate is required); it performs client-only side effects (useEffect) so it doesn't run during server-side rendering.
- Errors from creating a conversation are forwarded to notifyError; on failure the user remains on the loading view and no navigation occurs.
- The component intentionally posts { title: null } so the backend provides a distinct default identifier — renaming is expected to happen later via a PATCH if needed.

---

## loadLastConversation

> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

Returns the value stored under ACTIVE_CONVERSATION_KEY in localStorage, or null if the key is not present or accessing localStorage throws. Use this when restoring or resuming the last-open conversation during page load or app initialization; it provides a safe, exception-free lookup.

## Remarks
This small wrapper centralizes defensive access to localStorage so callers don't need to wrap every read in try/catch. It intentionally converts any error (missing key, storage disabled, running in an environment without window.localStorage) into a null result, simplifying control flow for callers that only need to know whether a previous conversation id exists.

## Example
```typescript
const lastConversationId = loadLastConversation();
if (lastConversationId) {
  // application-specific navigation/restore
  navigateToConversation(lastConversationId);
} else {
  // start a new conversation or show an empty state
  createNewConversation();
}
```

## Notes
- All exceptions thrown while accessing localStorage are swallowed and result in null; callers cannot distinguish between "key not set" and "storage unavailable".
- The function returns the raw string value; if the stored value is JSON, callers must parse it themselves.
- localStorage is synchronous and may be unavailable in server-side rendering or in browsers with storage disabled; this function handles those cases by returning null.

---