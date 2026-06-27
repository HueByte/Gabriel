# MainLayout

> **File:** `src/webapp/src/layouts/MainLayout.tsx`  
> **Kind:** function

Renders the application's primary layout: a Sidebar, a main content column that hosts route children via react-router's Outlet, and a centrally configured ToastContainer for notifications. Use this component as the top-level layout for routes so every page shares the same navigation, content area, and toast behaviour.

## Remarks
This component composes three concerns into a single top-level shell: navigation (Sidebar), routing outlet (Outlet) and global notifications (ToastContainer). Centralizing the ToastContainer here ensures consistent toast appearance and timing across the app. The component intentionally accepts no props — styling and behaviour are driven by the contained components and CSS classes ("app" and "main-col").

## Example
```typescript
// Example route setup (React Router v6)
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import Home from './pages/Home';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Home />} />
          {/* other nested routes render inside MainLayout's <Outlet /> */}
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
```

## Notes
- Only one ToastContainer should be mounted application-wide; mounting multiple containers can produce duplicate or unexpected toasts.
- The layout relies on CSS classes `app` and `main-col` for structure — changing those class names or their styles will affect page layout.
- MainLayout is a simple functional component (not memoized) and will re-render when its parent or route changes; avoid placing heavy computation here.