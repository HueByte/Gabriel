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


ChatPage renders the chat UI for a single conversation identified by the URL, orchestrating metadata loading, avatar seed management, project context, and the GabrielSequence-driven color palette. It keeps per-conversation UI state (avatarSeed, projectId, projectIsDefault, sequenceStops) with seed-based fallbacks while the sequence data loads, persists the active conversation so the IndexPage can resume later, and provides actions to reroll the avatar and to refresh the sequence after a turn.

## Remarks
ChatPage acts as the per-conversation glue between routing, server-provided identity, and local UI state. It ensures a conversation’s avatar seed and color palette reflect server data when available, while gracefully falling back to deterministic seed-based visuals during loading. It also isolates conversation-specific concerns from the broader lists by resetting the server-driven palette when the active conversation changes, preventing visual bleed across chats. This component is the focal point where routing, metadata loading, and the GabrielSequence-driven UI converge, keeping the UI responsive and consistent as data arrives and evolves.

## Notes
- The palette relies on a server-provided GabrielSequence; until it loads, a seed-derived palette is used to keep the UI visually coherent.
- Rerolling the avatar updates the seed and triggers a palette refresh, but does not refetch the entire conversation list to avoid unnecessary network traffic.
- If the conversation is missing (e.g., deleted in another tab), the handler surfaces an error and navigates away; this behavior is centralized around graceful error handling rather than silent failure.

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


Stores the given active conversation ID in localStorage under a shared key; if null is passed, it clears that entry. The operation is wrapped in a try/catch and any errors are ignored to prevent storage failures from interrupting the UI flow. Use this helper when you want to remember or clear the user's last active conversation across page reloads.

## Remarks
Centralizes persistence semantics for the active conversation. It encapsulates the storage key and error handling, keeping ChatPage logic focused on UI concerns and reducing boilerplate across the codebase. It also defines a clear signal for clearing persisted state by using null as the parameter.

## Example
```typescript
// Persist the currently active conversation
persistActiveConversation("abc123");

// Clear persisted value when the user signs out or selects none
persistActiveConversation(null);
```

## Notes
- Errors from localStorage are intentionally ignored to avoid disrupting the user experience.
- Passing null clears the persisted value; empty strings will also clear due to truthiness rules in JavaScript.

---