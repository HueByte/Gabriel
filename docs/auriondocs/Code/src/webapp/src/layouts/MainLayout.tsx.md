# MainLayout

> **File:** `src/webapp/src/layouts/MainLayout.tsx`  
> **Kind:** function

```typescript
export function MainLayout()
```


MainLayout defines the app’s shell by composing a persistent Sidebar, a routed content area via Outlet, and a ToastContainer for in-app notifications. Developers reach for it when they want a single, reusable layout wrapper that provides consistent chrome and toast behavior across all routed pages.

## Remarks
By centralizing layout concerns, MainLayout ensures a uniform look and feel across the app while the content changes behind the Outlet. The Outlet renders the currently active child route, enabling nested routing to plug into this chrome without duplicating structure. The ToastContainer configuration (position="bottom-right", autoClose={4000}, newestOnTop, closeOnClick, pauseOnHover, draggable={false}, theme="dark", toastClassName="gbr-toast") expresses a deliberate UX choice: notifications appear unobtrusively in the lower-right with the latest on top and a consistent dark styling that matches the app’s chrome.

## Notes
- Ensure your CSS defines .app, .main-col, and .gbr-toast for expected layout and toast styling.
- Only one ToastContainer should be mounted in the app to avoid duplicate toasts.
- If you need to adjust toast behavior, modify the props on ToastContainer in this component rather than sprinkling new containers elsewhere.