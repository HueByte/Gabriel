# IndexPage.tsx

> **Source:** `src/webapp/src/pages/IndexPage.tsx`

## Contents

- [IndexPage](#indexpage)
- [loadLastConversation](#loadlastconversation)

---

## IndexPage
> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

```typescript
export function IndexPage()
```


IndexPage boots the chat experience by deciding which conversation to display on first render. It first attempts to restore the last active conversation; if found, it navigates to that chat’s page and replaces the current history entry. If not, it creates a new conversation by posting a request with a null title (letting the backend generate a unique identifier), then navigates to the new conversation page. While this bootstrap work runs, it renders a lightweight loading indicator.

## Remarks
By centralizing this initialization logic in a dedicated page, the app guarantees a consistent entry point into a valid conversation view. The ranBootRef guard ensures the bootstrap runs only once per mount, preventing multiple navigations in development or with strict mode. Navigating with replace keeps the bootstrap step out of the user's history, so back navigation returns to the app root rather than the loading screen.

## Notes
- Passing title: null relies on the backend to assign a unique title or identifier; if this behavior changes, adjust payload accordingly.
- The code uses encodeURIComponent when constructing the path to avoid issues with special characters in IDs.

---

## loadLastConversation
> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

```typescript
function loadLastConversation(): string | null
```

**Returns:** `string | null`


Loads the last conversation from the browser's storage by reading the ACTIVE_CONVERSATION_KEY and returning the stored string. If access to storage fails or the key is missing, it returns null, allowing callers to decide whether to resume a previous conversation or start anew.

## Remarks
Encapsulates storage access and error handling behind a small, reusable helper, so UI logic doesn't need to manage try/catch around localStorage directly. It provides a single place to define and change the storage key, and it makes testing easier by providing a predictable null outcome when nothing is stored or storage is unavailable.

## Example
```typescript
const last = loadLastConversation();
if (last !== null) {
  resumeConversation(last);
} else {
  startNewConversation();
}
```

## Notes
- The function swallows any error thrown by localStorage and returns null; callers cannot distinguish between "not stored" and "storage access failure" from the return value alone.
- localStorage stores values as strings; if you stored a JSON object, parse it separately (e.g., JSON.parse(last)).
- ACTIVE_CONVERSATION_KEY must be defined in scope; otherwise a runtime ReferenceError could occur before attempting to read storage.

---