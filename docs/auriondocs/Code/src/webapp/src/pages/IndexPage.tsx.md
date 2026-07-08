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


IndexPage bootstraps the chat experience by routing the user to the most relevant conversation on mount: if a previously loaded last conversation exists, it navigates there; otherwise it creates a new conversation (title: null so the backend assigns a GUID) and navigates to that chat. While this startup sequence runs, it renders a simple loading indicator.

## Remarks
IndexPage centralizes the initial navigation flow for the chat UI. It determines whether to resume an existing conversation or start a fresh one, leveraging loadLastConversation and ConversationsService.postApiConversations to establish the destination and using the router to navigate without a full page reload. The one-time bootstrap is guarded by an internal ranBootRef to prevent duplicate navigation in environments that invoke effects multiple times (e.g., React StrictMode during development). The backend-driven ID generation (title: null) is intentional and can be renamed later via PATCH.

## Notes
- The component renders only a loading state during bootstrap; there is no additional UI until navigation completes, and errors are surfaced via notifyError without an explicit retry UI.
- If the startup flow fails to create or locate a conversation, the catch handler calls notifyError, but the user may remain on the loading screen unless a higher-level error handler intervenes.
- The backend generates the initial conversation identifier when title is null; renaming the conversation is handled by PATCH as noted in the code comments.

---

## loadLastConversation
> **File:** `src/webapp/src/pages/IndexPage.tsx`  
> **Kind:** function

```typescript
function loadLastConversation(): string | null
```

**Returns:** `string | null`


Safely reads the last conversation from the browser's localStorage using ACTIVE_CONVERSATION_KEY. It returns the stored string value when available, or null if the key is missing or an access error occurs. This non-throwing accessor is intended for restoring UI state (e.g., pre-filling a chat) on load without risking a crash if storage access is blocked or unavailable.

## Remarks
loadLastConversation centralizes a small, potentially brittle interaction with the Web Storage API behind a clean, predictable surface. It ensures the page can render even when localStorage is blocked or throws, at the cost of losing visibility into storage failures since the catch is silent. Callers should treat a non-null return as an opportunity to restore previous discourse, and remember to parse the returned string if the stored value is JSON.

## Notes
- It cannot distinguish between a missing value and a read error because both yield null (the function returns null for any failure path).
- The value is a string; if you store JSON, parse it before use.


---