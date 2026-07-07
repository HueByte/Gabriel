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


IndexPage bootstraps the chat experience on first load. It guards initialization with a ref to ensure the boot logic runs only once. If a previously loaded conversation exists, it navigates to /c/{encoded last} with replace; otherwise it creates a new conversation by posting { title: null } and then navigates to /c/{conv.id}. While these asynchronous steps complete, it renders a Loading… indicator.

## Remarks
IndexPage centralizes startup routing for the web app's chat feature, bridging persisted state, backend creation, and client-side navigation. It maintains a single bootstrap path that can be evolved to support additional startup policies (e.g., prefetching or preloading data) without duplicating navigation logic across pages.

## Notes
- The boot logic runs only once per mount due to ranBootRef, preventing multiple navigations during rerenders.
- If the backend post call fails, notifyError surfaces the error to the user.

---

## loadLastConversation
> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

```typescript
function loadLastConversation(): string | null
```

**Returns:** `string | null`


Reads the last conversation stored in the browser's localStorage and returns it as a string when present, or null if there is no saved value or if reading storage fails. This tiny helper centralizes access to the ACTIVE_CONVERSATION_KEY so the UI can gracefully fall back to a default state without throwing.

## Remarks
This function isolates persistence concerns from the page logic that renders the index view. By returning null on failure, it lets callers decide how to handle the absence of a saved conversation (e.g., starting a new chat). The small, synchronous surface keeps initialization simple and predictable.

## Example
```typescript
// Retrieve and use the last conversation, if available
const last = loadLastConversation();
if (last !== null) {
  console.log("Loaded last conversation:", last);
}
```

## Notes
- Access to localStorage can fail in certain environments or privacy modes; the function handles this by returning null instead of throwing.
- A null return indicates either the value is not stored or reading storage failed; callers should treat both as "no prior conversation".
- The function is synchronous and relies on a browser-global, so it may be a no-op in non-browser environments or during server-side rendering unless guarded by environment checks.

---