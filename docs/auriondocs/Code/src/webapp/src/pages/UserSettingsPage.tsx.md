# UserSettingsPage

> **File:** `src/webapp/src/pages/UserSettingsPage.tsx`  
> **Kind:** function

```typescript
export function UserSettingsPage()
```


Renders the application's user settings screen: account info, model selection, memory management, chat-display toggles, and a sign-out action. Use this component on the settings route (or wherever you need a full settings UI) — it composes authentication state, preference toggles and smaller subcomponents rather than managing those concerns itself.

## Remarks
This component is a presentational/container component that delegates responsibilities to smaller pieces: authentication state comes from useAuth, the model selector and memory list are separate components, and the three chat-display options are driven by custom hooks (useHideThinking, useHideToolCalls, useHideToolResults) which provide the boolean state and setter functions. It does not implement persistence or routing itself; it relies on the provided hooks and the router's useNavigate for navigation.

## Notes
- The component distinguishes three user states: undefined (still loading), null (not signed in), and a user object (signed in) — each state renders different UI.
- The back button calls navigate(-1), so its behavior depends on the router history (if no history entry exists the result depends on the router implementation).
- The sign-out handler calls logout() without awaiting its result (void logout()), so any async side effects are not awaited here.
- The chat-display toggles are controlled inputs; their state is entirely managed by the corresponding hooks (this component only reads and updates those values).