# AuthLayout

> **File:** `src/webapp/src/layouts/AuthLayout.tsx`  
> **Kind:** function

A minimal layout component for authentication-related routes that renders the current route's element (via Outlet) and a configured react-toastify ToastContainer for showing toasts. Use this layout when you want a consistent toast configuration and container available to all pages under the auth section (for example: login, register, password reset).

## Remarks
This component centralizes toast configuration for the authentication portion of the app and separates UI concerns: route rendering is handled by Outlet while ToastContainer controls toast presentation (position, theme, auto-close, and behavior). Placing the ToastContainer in the layout ensures toasts from any nested auth page appear with the same global settings and styling (including the custom "gbr-toast" class).

## Example
```typescript
// React Router v6 example
import { Routes, Route } from 'react-router-dom';
import { AuthLayout } from './layouts/AuthLayout';
import Login from './pages/Login';
import Register from './pages/Register';

function AppRoutes() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
      </Route>
      {/* other routes... */}
    </Routes>
  );
}
```

## Notes
- Outlet requires a React Router route context (BrowserRouter/MemoryRouter); render AuthLayout only inside a Router.
- react-toastify styles are not included automatically; import 'react-toastify/dist/ReactToastify.css' (or provide your own) so toasts are styled correctly.
- Avoid mounting multiple ToastContainer components with overlapping responsibilities: having more than one ToastContainer may cause duplicate or unexpected toast behavior.
- The ToastContainer props shown (autoClose = 4000, newestOnTop, draggable = false, theme = 'dark') are global defaults but individual toasts can override them when created.