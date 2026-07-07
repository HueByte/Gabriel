# MainLayout

> **File:** `src/webapp/src/layouts/MainLayout.tsx`  
> **Kind:** function

```typescript
export function MainLayout()
```


MainLayout is a React functional component that defines the app's top-level shell. It renders a persistent Sidebar, a central main column for routed content via Outlet, and a configured ToastContainer for in-app notifications. Use this component when you want a consistent page chrome across routes and to centralize global UI concerns such as navigation and toasts, rather than rendering page content directly.

## Remarks
MainLayout encapsulates layout concerns that would otherwise be repeated across pages: the app chrome (sidebar and content area) and the notification system. By centralizing these concerns, all routed pages render inside a shared shell, ensuring a uniform look and behavior for navigation and toasts. It acts as a boundary between route content and global chrome, so nested routes can render inside Outlet while preserving the persistent navigation and feedback UI.

## Example
```typescript
// Typical usage with React Router
<Routes>
  <Route path="/" element={<MainLayout/>}>
    <Route path="dashboard" element={<Dashboard/>} />
    <Route path="reports" element={<Reports/>} />
  </Route>
</Routes>
```

## Notes
- The ToastContainer configuration (position, autoClose, newestOnTop, closeOnClick, pauseOnHover, draggable, theme, toastClassName) is baked into MainLayout; changing these affects all toasts application-wide. 
- Outlet renders the matched child route content; Sidebar remains static across routes. 
- If you need a different chrome for a subset of routes, consider a separate layout component or render a different element in the parent layout.