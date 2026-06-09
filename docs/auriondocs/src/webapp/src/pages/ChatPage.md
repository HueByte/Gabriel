# ChatPage.tsx

> **Source:** `src/webapp/src/pages/ChatPage.tsx`

## Contents

- [ChatPage](#chatpage)
- [persistActiveConversation](#persistactiveconversation)

---

## ChatPage

> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

Renders the route-level chat UI for the conversation identified by the URL param (conversationId). It wires conversation metadata, project context, avatar seed selection, and the server-driven Gabriel sequence (palette and live state) so child views can render a consistent visual identity and react to turn-level updates.

## Remarks
ChatPage is a coordinator component — it does not itself implement the message list or sequence rendering but manages the shared state those pieces need. It persists the active conversation for the index page, tracks whether the conversation belongs to a non-default (project) context, exposes a seed-based fallback palette until the server sequence arrives, and provides a lightweight "bump" mechanism to refresh the Gabriel sequence after a chat turn completes. The component intentionally avoids refetching the whole sidebar/conversation list on every turn to reduce work; only the sequence (live state + token stats + palette) is refreshed.

## Notes
- The server palette (sequenceStops) is intentionally cleared when conversationId changes to avoid briefly showing the previous conversation's colors.
- Rerolling the avatar (rerollAvatar) is a no-op if conversationId is empty and, on success, updates only the avatar seed and triggers a sequence refresh; it does not refresh the conversation list/sidebar.
- projectIsDefault controls whether the conversation uses a project-shared avatar/sequence (when false) or a standalone, conversation-scoped one (when true).
- sequenceStops will be null until the first successful Gabriel sequence fetch — consumers should fall back to the seed-derived palette while null.


---

## persistActiveConversation

> **File:** `src/webapp/src/pages/ChatPage.tsx`  
> **Kind:** function

Stores or clears the active conversation identifier in browser localStorage so the UI can remember which conversation was last open across reloads or future visits. Pass a string id to persist it, or null to remove the saved value.

## Remarks
This helper centralizes persistence of the "active conversation" key and intentionally swallows any exceptions from the Storage API. That prevents runtime errors in environments where localStorage is unavailable (server-side rendering, private browsing) or when storage operations fail (quota exceeded, disabled storage), keeping callers simpler.

## Example
```typescript
// Persist an active conversation id
persistActiveConversation('conversation-abc123');

// Clear the persisted active conversation
persistActiveConversation(null);
```

## Notes
- The function swallows all errors; failure to write to storage is silent — callers should not assume persistence always succeeded.
- localStorage may be unavailable in non-browser or restrictive environments; this function handles that by catching exceptions.
- The code relies on a module-level ACTIVE_CONVERSATION_KEY constant being defined; ensure it is present and stable across releases.

---