# UserSettingsPage

> **File:** `src/webapp/src/pages/UserSettingsPage.tsx`  
> **Kind:** function

```typescript
export function UserSettingsPage()
```


UserSettingsPage renders the Settings screen for the current user, showing account information, model selection, memory entries, and chat display toggles, while wiring navigation and authentication actions. Use this component to present and manage user preferences and account details within the app’s settings route.

## Remarks
Acts as a page-level composition that coordinates multiple settings concerns through dedicated subcomponents and hooks. By centralizing the UI for account state, model selection, memory, and chat display preferences, it keeps the settings surface consistent and easy to extend. It relies on hooks for authentication state and local visibility controls, delegating persistence and domain logic to those hooks and to child components.

## Notes
- The component renders different blocks depending on user state: undefined (loading), a user object (signed in), or null (not signed in).
- The visibility toggles are driven by external hooks (useHideThinking, useHideToolCalls, useHideToolResults); changes may affect other parts of the UI that consume the same state.
- The back button uses navigate(-1); in certain routing setups this may not return to the intended page, so consider explicit navigation when appropriate.