# ChatPage.tsx

> **Source:** `src/webapp/src/pages/ChatPage.tsx`

## Contents

- [ChatPage](#chatpage)
- [persistActiveConversation](#persistactiveconversation)

---

## ChatPage

> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

Renders the top-level chat page for a conversation and coordinates client-side state needed by the chat UI. Use this component when you need a full chat view tied to the current conversationId from the URL; it wires up avatar seeding, project context, live Gabriel sequence updates (palette and token stats), and recovery behavior when a conversation is absent.

## Remarks
ChatPage acts as an orchestrator between route state, the ConversationsService, and the Gabriel sequence UI: it reads the active conversationId from URL params, persists the active conversation so other pages can resume it, and exposes handlers that update the avatar seed, project context, and the server-driven palette (sequenceStops). Rather than refetching the entire conversation list on every chat turn, it refreshes only the Gabriel sequence/state (via bumpSequence) to keep live stats and visuals up to date while avoiding expensive sidebar or project list refetches.

## Example
```typescript
// Typical usage inside a react-router routes setup — the conversationId param is optional
import { Route } from 'react-router-dom';

<Route path="/chat/:conversationId?" element={<ChatPage />} />
```

## Notes
- sequenceStops may be null until the first GabrielSequence load resolves; the UI falls back to a seed-derived palette until then.
- Calling the reroll avatar action updates the avatar seed and bumps the sequence (causing palette/pattern change) but intentionally does not refetch the conversation list/sidebar.
- If a conversation is deleted or returns 404, the component surfaces an error and navigates the user back to the root so the app can choose or create a replacement conversation.

---

## persistActiveConversation

> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

Store or remove the active conversation identifier in localStorage. Use this when the UI or application state changes which conversation is considered active (so it can be restored on reload); call with null to clear the stored active conversation.

## Remarks
This function performs a best-effort, side-effecting write to the browser's localStorage and intentionally swallows any exceptions (for example, when storage is unavailable or quota is exceeded). It is synchronous and has no return value — failures are ignored to avoid disrupting normal UI flow.

## Example
```typescript
// Save an active conversation id
persistActiveConversation('conversation-123');

// Clear the persisted active conversation
persistActiveConversation(null);
```

## Notes
- The function treats any falsy id (null, undefined, empty string) as a signal to remove the stored key; only truthy strings are saved.
- Because errors are caught and ignored, callers cannot rely on this function to have succeeded; use other persistence or telemetry if you need guaranteed storage or failure visibility.
- This should not be called during server-side rendering; localStorage is a browser API and may be undefined in non-browser environments.

---