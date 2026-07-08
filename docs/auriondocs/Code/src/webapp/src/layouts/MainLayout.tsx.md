# MainLayout

> **File:** `src/webapp/src/layouts/MainLayout.tsx`  
> **Kind:** function

```typescript
export function MainLayout()
```


MainLayout is the top-level layout component that provides the app's chrome by wrapping a persistent Sidebar, a routed content region via Outlet, and a global ToastContainer for toast notifications. Use this component as the root layout for your routes to ensure a consistent navigation chrome and a centralized toast area across pages.

## Remarks
MainLayout centralizes layout concerns so page components can focus on content. It wires together three collaborators—Sidebar for navigation, Outlet for embedding nested routes, and ToastContainer for user feedback—so they can evolve separately without duplicating chrome code.

## Notes
- The ToastContainer is configured with autoClose={4000} and draggable={false}; changing these will affect the user experience for toasts.
- The wrapper uses className="app" and main-col; ensure the corresponding CSS exists to preserve layout and styling across pages.