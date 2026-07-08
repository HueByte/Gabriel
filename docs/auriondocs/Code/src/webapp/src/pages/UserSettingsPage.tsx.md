# UserSettingsPage

> **File:** `src/webapp/src/pages/UserSettingsPage.tsx`  
> **Kind:** function

```typescript
export function UserSettingsPage()
```


UserSettingsPage renders the Settings UI for the current user, coordinating authentication state, navigation, and a collection of preference panels. It conditionally shows account information (loading while user data is being fetched, the email and user ID when signed in, or a not-signed-in message) and provides controls for selecting a model, listing memory items scoped to the user, and toggling visibility of the thinking steps, tool calls, and tool results in the chat view. A back button and a sign-out action complete the page.

## Remarks

By composing ModelSelector, MemoryList, and the trio of visibility toggles, this symbol serves as a cohesive user-facing surface that centralizes session-scoped preferences. It isolates authentication/navigation concerns from the content of the panels and delegates data retrieval to child components, while presenting a consistent settings experience.

## Notes

- Back navigation relies on browser/history state; in contexts with no history, the action may be a no-op.
- Logout is invoked without awaiting; if a post-logout redirect or loading state is required, handle the promise accordingly.
- The three toggles source their state from dedicated hooks (useHideThinking, useHideToolCalls, useHideToolResults); ensure these hooks implement persistence if you expect preferences to survive reloads.