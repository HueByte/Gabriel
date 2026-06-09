# UserSettingsPage

> **File:** `src/webapp/src/pages/UserSettingsPage.tsx`  
> **Kind:** function

Renders the application's user settings UI where a signed-in user can view account details, pick a model, manage stored memories, toggle visibility of the ReAct agent's intermediate steps (thinking, tool calls, tool results), and sign out. Use this component when adding a settings route or screen that centralizes per-user preferences and session controls; it relies on app-level hooks/providers for navigation, authentication state, and the toggle state.

## Remarks
This component composes several smaller pieces (ModelSelector, MemoryList) and uses hooks (useAuth, useNavigate, useHideThinking, useHideToolCalls, useHideToolResults) to obtain data and mutators. It intentionally renders three states for account info: undefined (loading), a user object (signed in), and null (not signed in). The chat-display checkboxes drive state through the corresponding hooks rather than managing local component state directly, so the hooks control how those preferences are stored or persisted.

## Example
```typescript
// Typical route registration using react-router
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { UserSettingsPage } from './pages/UserSettingsPage';

// inside your app root
<BrowserRouter>
  <Routes>
    <Route path="/settings" element={<UserSettingsPage />} />
  </Routes>
</BrowserRouter>
```

## Notes
- The component expects to be rendered inside a React Router context and an authentication provider that implements useAuth; the navigate and user/logout hooks will fail outside those providers.
- The account section handles three user states: undefined (shows "Loading…"), a user object (shows email and id), and null (shows "Not signed in.").
- The sign-out button calls logout() with `void` (it does not await the promise). If logout performs asynchronous navigation or cleanup, that behavior is the responsibility of the auth implementation.