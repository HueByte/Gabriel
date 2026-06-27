# router.tsx

> **Source:** `src/webapp/src/router.tsx`

## Contents

- [ErrorBoundaryProps](#errorboundaryprops)
- [ErrorBoundaryState](#errorboundarystate)
- [ErrorBoundary](#errorboundary)
- [ChunkErrorFallback](#chunkerrorfallback)
- [LazyPage](#lazypage)
- [PageLoader](#pageloader)

---

## ErrorBoundaryProps

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

Properties for an ErrorBoundary component: the normal child content to render and the fallback UI to show if a descendant throws during rendering. Use this interface to type-check the props you pass into an error boundary wrapper.

## Remarks
This is a minimal props contract used by an error boundary wrapper: `children` is the subtree that the boundary guards, and `fallback` is the React node rendered when the boundary catches an error. Both properties are required by the interface; if you need the fallback to receive error details or control callbacks (retry, reset), extend this interface so the boundary can pass those values into a component rather than a plain node.

## Example
```typescript
// Simple usage with a static fallback UI
<ErrorBoundary fallback={<div>Something went wrong.</div>}>
  <App />
</ErrorBoundary>

// Fallback can be a composed component that handles retry logic
<ErrorBoundary fallback={<RetryableErrorMessage />}> 
  <Suspense fallback={<Loader />}>
    <Feature />
  </Suspense>
</ErrorBoundary>
```

## Notes
- Both `children` and `fallback` are typed as `ReactNode`; if you want a render-prop style fallback that receives the caught error or a reset function, update the prop type to accept a function or a component with props.
- `ReactNode` allows `null`/`undefined` — ensure your fallback is non-empty if you want visible feedback when errors occur.

---

## ErrorBoundaryState

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

A minimal TypeScript interface that describes the state shape for a React error boundary: a single boolean flag indicating whether the boundary has caught an error. Use this type to strongly-type the state of class-based error boundary components (e.g., components that implement getDerivedStateFromError or componentDidCatch) so rendering logic and type-checking rely on a known, explicit state shape.

## Remarks
This interface is intentionally small — it only tracks whether an error has occurred. It exists to make the component state explicit and to drive conditional rendering of fallback UI when an error is present. If you need to surface the actual Error object or additional metadata (stack, timestamp, etc.), extend this interface rather than widening it in-place.

## Example
```typescript
import React from 'react';

interface Props {}

class AppErrorBoundary extends React.Component<Props, ErrorBoundaryState> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): ErrorBoundaryState {
    // Update state so the next render shows the fallback UI.
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Log error or report to a service
    console.error(error, info);
  }

  render() {
    if (this.state.hasError) {
      return <div>Something went wrong.</div>;
    }
    return this.props.children as React.ReactNode;
  }
}
```

## Notes
- Initialize hasError to false in the component's constructor or state initializer; otherwise, rendering logic that checks this flag will be unreliable.
- For functional components use a boolean from useState (e.g., const [hasError, setHasError] = useState(false)); this interface applies to class-based state typing only.
- If you need to display error details to users or send them to a backend, extend the interface to include an Error and/or metadata rather than overloading the boolean flag.

---

## ErrorBoundary

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** class

A React error boundary component that captures runtime errors thrown in its child component tree and renders a provided fallback UI instead of letting the whole React tree crash. Reach for this when you want to isolate failures (for example around route handlers or large UI subtrees) so the rest of the application can continue to function.

## Remarks
Implements the React error boundary lifecycle by using static getDerivedStateFromError to flip an internal hasError flag and componentDidCatch to perform logging. The component special-cases dynamic-import chunk loading failures (it looks for the message text "Failed to fetch dynamically imported module") and logs a clearer, actionable message indicating a new version may be available; all other errors are logged along with React's component stack (errorInfo).

## Example
```typescript
// Wrap the app or a subtree to prevent rendering errors from breaking the whole page
<ErrorBoundary fallback={<div>Something went wrong — please reload.</div>}>
  <App />
</ErrorBoundary>
```

## Notes
- Once an error is captured the internal hasError stays true and the boundary renders the fallback; to attempt recovery you must remount the ErrorBoundary (for example by changing its key) or implement a mechanism to reset state.
- The chunk-loading detection uses a simple substring check on error.message; other chunk or network failure messages may not be recognised by that conditional.
- For non-chunk errors the component logs both the Error and the React errorInfo (component stack) which is useful for diagnosing where in the tree the error originated.

---

## ChunkErrorFallback

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

Renders a minimal fallback UI shown when a page's JS chunk fails to load or an otherwise unrecoverable client-side loading error occurs. It displays a short message and a single "Reload" button that calls window.location.reload() to attempt recovery by loading the latest assets. Use this component as the UI for an error boundary or router-level fallback when chunk/network failures are possible.

## Remarks
This component exists to provide a safe, dependency-free recovery path when the app's runtime cannot continue because required script chunks are missing or incompatible (for example after a deployment). Instead of trying to recover in-place, it prompts the user to perform a full page reload so the browser fetches the newest bundles. The implementation is intentionally tiny to avoid running additional app code that might also fail under the same conditions.

## Example
```typescript
// Render directly as a fallback UI
<ChunkErrorFallback />

// Example: using with react-error-boundary
import { ErrorBoundary } from 'react-error-boundary';

<ErrorBoundary FallbackComponent={ChunkErrorFallback}>
  <App />
</ErrorBoundary>
```

## Notes
- The reload triggers a full page refresh and will discard any unsaved state; warn users if necessary.
- If the server or CDN is still serving the old or missing assets, repeatedly reloading can lead to a reload loop.
- Visual appearance depends on existing CSS classes ("auth-loading", "auth-submit"); adjust or replace classes to match your app's styling.

---

## LazyPage

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

A functional React component that accepts a props object containing a children property. The component is intended as a page-level wrapper (it's declared in router.tsx) and is used to receive and render content supplied by the router or a parent component. The implementation is not present in the provided snippet, so callers should inspect the source for any additional behavior (layout, suspense boundaries, side effects).

## Remarks
This symbol appears in router.tsx and serves as a thin abstraction point where page content can be wrapped, forwarded, or augmented. Because the implementation is missing from the snippet, its exact responsibilities (for example whether it provides layout, error/suspense handling, or simply returns children) are not guaranteed — treat it as a place to add page-level concerns if you control the implementation.

## Example
```typescript
// Typical usage in JSX
<LazyPage>
  <MyPageComponent />
</LazyPage>
```

## Notes
- The provided source fragment only shows the parameter list; consult the full implementation to confirm whether children are forwarded unchanged or processed.
- Ensure the children prop is typed appropriately (usually React.ReactNode) and guard for undefined if the component's implementation does not handle missing children.

---

## PageLoader

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

Renders a minimal, presentational loading indicator: a div with className "auth-loading" containing the text “Loading…”. Use this component anywhere a small, consistent page-level loader is needed (for example as a Suspense fallback or while waiting for route data).

## Remarks
This component centralizes the markup for a simple page loader so the app can reuse the same class and text across routes. It deliberately has no props or internal state — styling and any animation should be provided via the .auth-loading CSS rule. It is intentionally minimal so callers can wrap or replace it with a more featureful loader if needed.

## Example
```typescript
// used directly in JSX
<PageLoader />

// used as a Suspense fallback for lazy-loaded routes
<Suspense fallback={<PageLoader />}>
  <YourLazyRoute />
</Suspense>
```

## Notes
- This component is purely presentational and exposes no props; to change appearance, modify the .auth-loading CSS.
- It does not include ARIA attributes or roles; add role="status" or aria-busy="true" if you need explicit screen-reader announcements.
- The displayed text is the single ellipsis character (…), ensure your source file encoding preserves it if you alter the string.

---