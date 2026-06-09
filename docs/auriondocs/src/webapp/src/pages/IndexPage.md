# IndexPage.tsx

> **Source:** `src/webapp/src/pages/IndexPage.tsx`

## Contents

- [IndexPage](#indexpage)
- [loadLastConversation](#loadlastconversation)

---

## IndexPage

> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

Renders a short-lived loading view and bootstraps the app's conversation routing: when mounted it attempts to navigate to the most recently used conversation; if none exists it creates a new conversation on the server and then navigates to it. Use this component as the application root (e.g. mounted at the "/" route) when you want the app to automatically resume the last conversation or start a new one.

## Remarks
This component encapsulates the application's initial redirect/boot logic so route setup only needs to mount it at the root. The effect runs exactly once (a local ref prevents repeat runs, which also helps under React Strict Mode). Navigation uses replace so the created/selected conversation becomes the current entry rather than leaving the loading route in history. A new conversation is created with title: null so the backend can assign a unique, distinguishing identifier (the GUID) by default.

## Example
```typescript
// Typical usage in a router
import { Route } from 'react-router-dom';
import { IndexPage } from './pages/IndexPage';

<Route path="/" element={<IndexPage />} />
```

## Notes
- The component performs side effects (network request + navigation); it only shows a transient "Loading…" message and does not render any conversation UI itself.
- If ConversationsService.postApiConversations fails, the error is forwarded to notifyError and the component does not navigate — ensure notifyError provides user feedback or retry logic if needed.
- Conversation IDs are URI-encoded before being placed in the path; callers should expect encoded GUIDs in the URL.
- The implementation relies on loadLastConversation() to determine the last-used conversation; if that function returns falsy, a new conversation is created.


---

## loadLastConversation

> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

Reads the previously active conversation identifier from browser localStorage using the ACTIVE_CONVERSATION_KEY and returns it as a string, or null if the value is missing or access fails. Use this helper when restoring UI state on page load so callers don't need to guard storage access themselves.

## Remarks
This small abstraction centralizes access to localStorage for the "last conversation" value and is defensive by design: it catches any exceptions that may occur when reading from storage (for example, in environments where localStorage is unavailable or when access is blocked) and returns null instead of throwing. That makes it safe to call from initialization code that must run in many environments (including server-side render paths or restricted browsers).

## Example
```typescript
const last = loadLastConversation();
if (last !== null) {
  // `last` is the raw string stored under ACTIVE_CONVERSATION_KEY.
  // If you stored a JSON payload, parse it here.
  // restoreConversation(last);
} else {
  // no previous conversation found — start fresh
}
```

## Notes
- Returns null both when the key is not present and when any exception occurs while accessing localStorage (so callers cannot distinguish between "missing" and "error" cases).
- The function returns the raw stored string. If you stored a JSON object, parse it (JSON.parse) after checking for null.
- The implementation intentionally swallows errors for robustness; this also means debugging storage access failures requires different instrumentation if you need error details.

---