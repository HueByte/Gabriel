# AuthLayout

> **File:** `src/webapp/src/layouts/AuthLayout.tsx`  
> **Kind:** function

```typescript
export function AuthLayout()
```


AuthLayout is a compact layout component that renders an Outlet for nested routes and a globally available ToastContainer. The ToastContainer is pre-configured to show toasts at the bottom-right corner, auto-dismissing after 4000 milliseconds, with new toasts appearing on top, and a dark theme with a custom CSS class of gbr-toast. This component is meant to be used as a shared wrapper around authentication-related routes (or any set of pages) so that child pages can trigger toasts without each page including its own toast container.

## Remarks
AuthLayout centralizes toast presentation and route rendering so individual pages only focus on content. It provides a consistent bottom-right toast experience across all routes rendered within it, decoupling toast behavior from page logic. Keeping a single ToastContainer here helps avoid duplicate overlays and inconsistent styling.

## Notes
- If you render AuthLayout multiple times in the app, you may end up with multiple ToastContainers; prefer a single root layout or ensure only one instance is mounted.
- The ToastContainer uses a dark theme and a custom class; ensure you have corresponding CSS to avoid unstyled toasts.
- Toasts auto-close after 4 seconds; if you need longer notifications, override per-toast options when creating the toast.