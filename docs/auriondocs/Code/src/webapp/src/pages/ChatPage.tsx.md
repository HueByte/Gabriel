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


ChatPage is the React page component that renders the chat UI for a given conversation, using a server-driven GabrielSequence palette and avatar seed while gracefully falling back to seed-based visuals until the data arrives. It persists the active conversation for quick restoration on return, resets the visual palette when the conversation changes to avoid color bleed, and exposes a reroll avatar action that updates both the seed and the current palette without refetching the chat list.

## Remarks
ChatPage serves as an orchestration layer between route-derived state, server-provided sequence data, and local UI state. It decides when to rely on a server palette versus a seed-derived fallback, and it coordinates avatar seeds with the active palette to give a coherent visual identity per conversation and project context. By caching and refreshing only the necessary slices (avatar seed, palette, and sequence), it minimizes churn and preserves the sidebar's stability while the main conversation visuals update.

## Notes
- Palette fallback ensures visual continuity during sequence loading; when conversationId changes, sequenceStops is cleared to prevent bleeding.
- Avatar reroll updates avatarSeed and triggers a palette refresh; it bumps the sequence but does not refetch the conversation list.

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


Persists the currently active conversation identifier in the browser's storage. When an id is provided, it stores the value under ACTIVE_CONVERSATION_KEY; when null is provided, it removes that key. The operation is wrapped in a try/catch and errors are ignored to avoid impacting the UI, so callers can persist state without risking user-facing failures.

## Remarks
This function encapsulates the browser storage interaction, keeping ChatPage.tsx focused on UI concerns. By centralizing the key and error handling, it reduces boilerplate and makes restoration of the active conversation state straightforward across page reloads.

## Example
```typescript
persistActiveConversation('conv-42');
persistActiveConversation(null);
```

## Notes
- Silent catch means storage failures won't throw, but you won't be alerted to persistence issues; consider adding logging or a fallback if persistence is mission-critical.

---