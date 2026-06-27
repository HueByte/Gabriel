# AuthLayout

> **File:** `src/webapp/src/layouts/AuthLayout.tsx`  
> **Kind:** function

A small React layout component intended for authentication-related routes. It renders a react-router Outlet for nested route content and a pre-configured react-toastify ToastContainer so all child auth pages share the same toast behaviour and appearance.

## Remarks
AuthLayout centralizes toast configuration for the app's authentication flows (login, register, password reset, etc.), ensuring a consistent position, timing, and styling for notifications shown from those pages. By exposing an Outlet it lets route definitions nest auth pages under a single layout while keeping toast setup in one place. The ToastContainer is configured with theme="dark", positioned bottom-right, auto-closing after 4000ms, newestOnTop, closeOnClick, pauseOnHover, draggable disabled, and a custom toastClassName ("gbr-toast") for styling.

## Example
```typescript
// react-router v6 route setup
import { Route, Routes } from 'react-router-dom';
import { AuthLayout } from './layouts/AuthLayout';
import Login from './pages/Login';
import Register from './pages/Register';

function AppRoutes() {
  return (
    <Routes>
      <Route path="/auth" element={<AuthLayout />}>
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
      </Route>
      {/* other routes */}
    </Routes>
  );
}
```

## Notes
- The component must be used inside a Router (e.g., BrowserRouter) because Outlet requires router context. 
- Avoid mounting multiple ToastContainer instances with overlapping scopes; duplicate containers can cause confusing duplicate toasts. 
- The toastClassName "gbr-toast" implies custom CSS; ensure the class is defined if you rely on custom styling. Draggable is disabled here (draggable: false), which prevents drag-to-dismiss behavior.