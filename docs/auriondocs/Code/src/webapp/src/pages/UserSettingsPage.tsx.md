# UserSettingsPage

> **File:** `src/webapp/src/pages/UserSettingsPage.tsx`  
> **Kind:** function

```typescript
export function UserSettingsPage()
```


UserSettingsPage is a React functional component that renders the user-centric Settings page. It gathers authentication state via useAuth, shows either a loading state, the signed-in user's email and id, or a not-signed-in notice, and provides sections for Model selection, Memory, and Chat display preferences. It also includes a back button for navigation and a Sign out action that invokes logout. The UI exposes three toggles to hide different parts of the chat display (thinking, tool calls, and tool results), wiring them to dedicated hooks so the user can customize their workspace without altering the underlying data.

## Remarks
UserSettingsPage centralizes the user-facing settings experience by tying together authentication state with per-user preferences. It delegates presentation of model and memory details to child components (ModelSelector and MemoryList) while coordinating navigation and sign-out flow. This composition makes it straightforward to extend or replace individual sections without affecting the overall page structure.

## Notes
- The Account section renders three distinct states based on the authentication status: undefined (loading), a user object (signed in with email/id), and null (not signed in).
- The toggles for hiding thinking, tool calls, and tool results are local UI state driven by respective hooks; persistence behavior relies on the implementations of those hooks elsewhere in the codebase.