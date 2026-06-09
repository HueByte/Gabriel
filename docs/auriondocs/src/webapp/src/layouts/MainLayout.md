# MainLayout

> **File:** `src/webapp/src/layouts/MainLayout.tsx`  
> **Kind:** function

Renders the application's main layout: a persistent Sidebar, a central content column for nested routes (via react-router's Outlet), and a pre-configured ToastContainer for app-wide notifications. Use this component as the top-level layout for routes that should share the sidebar and toast behavior.

## Remarks
MainLayout centralizes common UI scaffolding so pages can focus on content. It enforces a consistent toast configuration (dark theme, bottom-right, 4s auto-close, newest on top, non-draggable) and layout CSS classes ("app", "main-col", and toast class "gbr-toast"). It is a purely presentational component with no props; routing context (Outlet) and styling are provided externally.

## Example
```typescript
// Typical usage with react-router
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import Home from './pages/Home';
import Settings from './pages/Settings';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Home />} />
          <Route path="settings" element={<Settings />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
```

## Notes
- Outlet requires a Router context (BrowserRouter/HashRouter); using MainLayout outside a router will throw.
- Only one ToastContainer is normally desired; adding additional containers can produce duplicate toasts or unexpected behavior.
- ToastContainer and react-toastify assume a browser environment; server-side rendering may require guarding or dynamic import to avoid window-related errors.
- Styling depends on the presence of the CSS classes used ("app", "main-col", "gbr-toast").
