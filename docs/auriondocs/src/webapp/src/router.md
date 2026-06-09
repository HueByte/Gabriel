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

Represents the props expected by an error-boundary-style component: a required children tree to render inside the boundary and a required fallback ReactNode to present when the boundary should show an alternate UI. Use this type when declaring props for a wrapper that catches rendering errors so TypeScript enforces that callers supply both the content and a fallback UI.

## Remarks
This is a focused DTO that makes an error-boundary component's contract explicit: the component must be given something to render (children) and something to show instead (fallback). Keeping this as a small interface improves reuse and makes component signatures self-documenting.

## Example
```typescript
// Typical usage with a component that implements error-boundary behavior
function ErrorBoundaryWrapper(props: ErrorBoundaryProps) {
  const { children, fallback } = props;
  // ...error handling logic would go here
  return (
    // pseudocode: either render children or fallback depending on error state
    /* isErrored ? fallback : children */
    <>{children}</>
  );
}

// Passing props
<ErrorBoundaryWrapper fallback={<div>Something went wrong</div>}>
  <App />
</ErrorBoundaryWrapper>
```

## Notes
- Both properties are required by the type; TypeScript will report an error if either is omitted.
- ReactNode includes values like null, undefined, strings and fragments; a fallback of null is allowed by the type but would result in no visible fallback UI if used at runtime.
- This interface only defines shape; it does not implement any error handling behavior itself.

---

## ErrorBoundaryState

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

```typescript
interface ErrorBoundaryState
```


A minimal state shape used by an error boundary component to record whether an error has occurred. Use this interface when typing a React class component's state that only needs to track the presence of an error (not the error details).

## Remarks
This interface represents the simplest form of error-boundary state: a single boolean flag that indicates an error was caught. It is intended for components that only need to switch rendering (for example, show a fallback UI) when an error happens, and do not require storing the actual Error object or a stack trace.

## Example
```typescript
// Typical usage in a React class component implementing an error boundary
import React from 'react';

class AppErrorBoundary extends React.Component<{}, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false };

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Log the error or report to a monitoring service
    console.error(error, info);
  }

  render() {
    if (this.state.hasError) {
      return <div>Something went wrong.</div>;
    }

    return this.props.children;
  }
}
```

## Notes
- The interface intentionally excludes error details; if you need the Error object or metadata, extend the state with additional properties.
- Remember to reset hasError (for example on navigation) if you want the boundary to attempt re-rendering children after the error condition is cleared.

---

## ErrorBoundary

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** class

A React error boundary component that catches errors thrown during rendering, in lifecycle methods, and in constructors of its child component tree. When an error is caught it flips an internal flag and renders the provided `fallback` React node instead of the children; use this to isolate parts of the UI so a failure doesn't crash the whole page.

## Remarks
This is a minimal class-based error boundary: it implements React's getDerivedStateFromError to mark the boundary as failed and componentDidCatch to log the error. It specially recognizes the common "stale chunk after deploy" failure (message containing "Failed to fetch dynamically imported module") and logs a clearer message suggesting a new version may be available. For all other errors it logs the error together with React's component stack (errorInfo).

## Example
```typescript
// Wrap a subtree (for example, a route or the entire app) to show a fallback UI
<ErrorBoundary fallback={<ErrorPage />}>
  <App />
</ErrorBoundary>
```

## Notes
- Error boundaries catch rendering, lifecycle, and constructor errors in their child tree, but they do NOT catch errors thrown from event handlers, asynchronous callbacks (promises/timeouts), or server-side rendering failures.
- This boundary does not provide a built-in recovery/reset mechanism. To recover from the fallback state you must remount the boundary (for example by changing a key) or implement a wrapper that clears the error state.
- The component logs to console.error only; it does not send errors to an external reporting service.
- The component assumes the caught value is an Error and accesses `error.message`. If non-Error values are thrown at runtime (e.g. a string), accessing `error.message.includes` could itself throw. Ensure thrown values are Error objects or extend this boundary to guard that access.

---

## ChunkErrorFallback

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

A minimal React component shown when a chunk (dynamic import) fails to load. It informs the user that the page could not be loaded (possibly because a new version is deployed) and offers a single "Reload" button that performs a full page reload.

## Remarks
This component is intended as a fallback UI for chunk-loading or other client-side load failures where simply reloading the page is the most straightforward recovery. It keeps behavior intentionally simple: a short message and an action that calls window.location.reload(). Styling is provided via CSS classes (auth-loading, auth-submit) and an inline layout style.

## Example
```typescript
import { ErrorBoundary } from 'react-error-boundary';
import ChunkErrorFallback from './router';

// Use with an error boundary so chunk-loading errors show this UI
function AppWrapper() {
  return (
    <ErrorBoundary FallbackComponent={ChunkErrorFallback}>
      <App />
    </ErrorBoundary>
  );
}
```

## Notes
- The Reload button triggers a full page reload (window.location.reload()), which will discard client state; ensure this is acceptable for your app's UX.
- The component relies on CSS classes for visual styling; include appropriate styles for `.auth-loading` and `.auth-submit` in your stylesheet.
- For improved accessibility and internationalization, consider adding ARIA roles/labels and replacing the hard-coded strings with localized resources.

---

## LazyPage

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function LazyPage(
```


A function named `LazyPage` that accepts a single destructured parameter containing `children`. Only the parameter fragment is available in the provided source; the implementation, return value and intended behavior cannot be determined from the available code.

## Remarks
The name and the presence of a `children` parameter commonly indicate a wrapper or container (for example, a React component that renders child nodes), but that is speculative without the body. This documentation intentionally avoids assuming framework-specific behavior because the function body and return expression are absent.

## Notes
- The source is incomplete — provide the full function body to generate accurate usage guidance and examples.
- Do not assume this returns JSX/React nodes or is a React component based solely on the `children` parameter.

---

## PageLoader

> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

Renders a minimal loading placeholder: a div with className "auth-loading" containing the text “Loading…”. Reach for this component when you need a tiny, reusable presentational indicator during page transitions, authentication checks, or as a fallback for lazy-loaded routes/components.

## Remarks
This is a purely presentational component that centralizes the markup and CSS class used for an app-wide loading state. It’s intended as a drop-in replacement wherever a consistent loading appearance is desired (for example as a React.Suspense fallback or while waiting for route/auth state). The component does not manage timing, progress, or visibility — those concerns remain with the caller.

## Example
```typescript
// As a Suspense fallback
import React, { Suspense } from 'react';
import PageLoader from './router'; // adjust path as needed

const LazyPage = React.lazy(() => import('./SomePage'));

function App() {
  return (
    <Suspense fallback={<PageLoader />}>
      <LazyPage />
    </Suspense>
  );
}

// As a simple inline placeholder
function AuthGate({ isLoading, children }: { isLoading: boolean; children: React.ReactNode }) {
  if (isLoading) return <PageLoader />;
  return <>{children}</>;
}
```

## Notes
- The component relies on the "auth-loading" CSS class for visual styling; ensure the stylesheet exists and is loaded.
- For better accessibility, callers may want to wrap or replace this with an element that provides role="status" and/or aria-live attributes so screen readers announce the loading state.
- The component is not configurable — to customize text or add a spinner, replace or wrap it at the call site.

---