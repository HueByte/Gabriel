# AuthLayout

> **File:** `src/webapp/src/layouts/AuthLayout.tsx`  
> **Kind:** function

```typescript
export function AuthLayout()
```


AuthLayout is a lightweight React layout component used for authentication-related pages. It renders an Outlet for nested routes (e.g., login, signup, forgot-password) and injects a preconfigured ToastContainer so that toast notifications appear consistently in the bottom-right with a dark theme and a 4-second auto-close. Use this layout to ensure all auth screens share the same frame and toast behavior without duplicating configuration on every page.

## Remarks
AuthLayout centralizes the concerns of route composition and user feedback within the authentication area. By providing a single ToastContainer, it guarantees a uniform notification experience across all auth screens while keeping each page focused on its specific content. This separation of concerns also makes it easy to adjust branding (position, theme, timeout) in one place without touching individual routes.

## Notes
- The ToastContainer is configured with a fixed position (bottom-right), newestOnTop, and a dark theme. If you need a different look for a sub-section, you may override defaults at the call site or replace the container.
- If AuthLayout is mounted in multiple parts of the app, multiple ToastContainer instances may be created. Ensure this aligns with your UX expectations to avoid duplicate or conflicting toasts.