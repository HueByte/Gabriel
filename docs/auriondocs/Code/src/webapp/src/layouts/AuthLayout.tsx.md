# AuthLayout

> **File:** `src/webapp/src/layouts/AuthLayout.tsx`  
> **Kind:** function

```typescript
export function AuthLayout()
```


AuthLayout is a compact layout component that renders nested routes via an Outlet and provides a global ToastContainer for notifications. Use it to establish a consistent layout wrapper for a section of your route tree while enabling toast messages to appear across all nested pages without adding a ToastContainer in each page.

## Remarks
AuthLayout centralizes the combination of route rendering and toast presentation, providing a single, reusable scaffold for pages that share a common layout and global notifications. By housing Outlet and ToastContainer together, it ensures consistent toast positioning (bottom-right), theming, and interaction defaults across all nested routes.

## Notes
- Avoid placing another ToastContainer elsewhere if you want a single toast stream; multiple containers can duplicate toasts.
- The className "gbr-toast" relies on project CSS; ensure the corresponding styles are loaded for correct appearance.