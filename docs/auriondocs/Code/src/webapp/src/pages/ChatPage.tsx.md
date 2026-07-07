# ChatPage.tsx

> **Source:** `src/webapp/src/pages/ChatPage.tsx`

## Contents

- [ChatPage](#chatpage)
- [persistActiveConversation](#persistactiveconversation)

---

## ChatPage
> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

```typescript
export function ChatPage()
```


ChatPage is a React function component that renders the chat UI for a specific conversation. It reads the conversationId from the route, preserves an avatar seed, manages a server-driven color palette derived from the Gabriel sequence, and coordinates loading of conversation metadata so visuals reflect the correct project context (default vs. project-wide). Until the sequence data arrives, a seed-derived palette provides a stable visual baseline; once a sequence loads, its palette becomes the canonical color theme for the UI. The component also persists the active conversation to support resumption, and resets the server-driven palette when the active conversation changes to avoid color bleed. It exposes actions like rerollAvatar to request a new avatar seed from the backend; a successful reroll updates the seed and triggers a sequence refresh so avatar and palette shift cohesively. Overall, ChatPage acts as the UI glue that ties route-driven state to server-provided visuals while keeping loading, fallback, and project-context concerns in a coherent, isolated surface.

---

## persistActiveConversation
> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

```typescript
function persistActiveConversation(id: string | null)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `id` | `string | null` | — |


Persist the active conversation identifier in localStorage under a shared key when you pass a non-empty string, or clears that key when you pass a falsy value. This lightweight helper enables the chat page to remember which conversation is active across reloads, so a user returning to the chat sees the same context. It intentionally swallows localStorage errors to avoid breaking the UI in environments where storage access is restricted or unavailable.

## Remarks
This centralizes storage concerns behind a single API, decoupling the UI from direct localStorage calls and from the specific storage key. It also guarantees that an empty or null input clears the stored id, ensuring a consistent "no active conversation" state across sessions.

## Example
```typescript
// Persist a new active conversation
persistActiveConversation("conv_42");

// Clear the active conversation
persistActiveConversation(null);
```

## Notes
- Passing an empty string will remove the stored value rather than persisting an empty id.
- Failures interacting with localStorage are swallowed; callers should not rely on this function for error reporting.

---