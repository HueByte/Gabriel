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

```typescript
interface ErrorBoundaryProps
```


ErrorBoundaryProps defines the props contract for an error boundary component in a React UI. It requires a children ReactNode to render under normal operation and a fallback ReactNode to render when an error is caught, enabling graceful degradation of the UI rather than the entire subtree crashing.

## Remarks
This interface encapsulates a minimal, explicit boundary: it separates content from error handling presentation, so developers can swap in different fallbacks without changing the consuming component. It also enforces that both the normal content and the error UI are provided at compile time, reducing runtime ambiguity for boundary usage.

## Example
```typescript
// Example usage of ErrorBoundaryProps
const boundaryProps: ErrorBoundaryProps = {
  children: <App />,
  fallback: <div>Something went wrong.</div>,
};
```

## Notes
- Both props are required; omitting either will cause a TypeScript error.
- ReactNode is broad (elements, strings, numbers, or null), so choose a meaningful fallback UI that remains stable across renders.
- Error boundaries catch rendering-time errors in their subtree (including during rendering and lifecycle methods) but do not catch errors inside event handlers or asynchronous code; handle those separately as appropriate.

---

## ErrorBoundaryState
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

```typescript
interface ErrorBoundaryState
```


ErrorBoundaryState is a TypeScript interface that represents the state shape for a class-based React error boundary used within the web application's routing layer. It defines a single boolean flag, hasError, which signals whether an error has been caught and the boundary should switch to a fallback UI.

## Remarks
Error boundaries encapsulate rendering errors to provide a graceful recovery path without crashing the entire UI. By typing the boundary's state as ErrorBoundaryState, the codebase gains a consistent, explicit contract for error handling across routing components, improving readability and maintainability when implementing or refactoring error boundaries.

## Example
```typescript
import React from 'react';

type Props = { children?: React.ReactNode };

class MyErrorBoundary extends React.Component<Props, ErrorBoundaryState> {
  state: ErrorBoundaryState = { hasError: false };

  componentDidCatch(_error: any, _info: any) {
    this.setState({ hasError: true });
  }

  render() {
    if (this.state.hasError) {
      return <div>Something went wrong.</div>;
    }
    return this.props.children ?? null;
  }
}
```

## Notes
- Ensure the initial state sets hasError to false to reflect a non-error render. 
- This interface is intended for class-based error boundaries; function components using hooks would manage equivalent state differently and may not use this type directly.


---

## ErrorBoundary
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** class

```typescript
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState>
```


ErrorBoundary is a React class component that acts as a boundary for capturing runtime errors in its subtree and rendering a fallback UI instead of crashing the entire app. It uses the error boundary lifecycle by implementing a static getDerivedStateFromError to set a hasError flag, and rendering either a provided fallback UI or its children based on that state. In addition to catching errors, it logs details in componentDidCatch, with special handling for "Failed to fetch dynamically imported module" to surface chunk-loading/deploy issues, while otherwise logging a standard page error with error and errorInfo. This component is commonly used to wrap routes or sections of the UI where failures should not bring down the entire page.

## Remarks
ErrorBoundary provides a focused, user-friendly safety net for parts of the UI that are error-prone or dynamically loaded. By centralizing error handling and offering a consistent fallback experience, it helps maintain a stable user experience even when modules fail to load or render. The specialized logging in componentDidCatch aids in diagnosing deployment-related issues (e.g., stale chunks after a new version deploy) without surfacing raw errors to end users. Use this boundary to guard routing trees or high-risk UI boundaries where a graceful degradation is preferable to a full crash.

## Example
```typescript
<ErrorBoundary fallback={<div>Something went wrong. Please try again.</div>}>
  <App />
</ErrorBoundary>
```

## Notes
- Error boundaries catch errors in render, lifecycle methods, and constructors of the subtree, but do not catch errors inside event handlers or asynchronous callbacks placed outside render. Planning error handling for those cases remains important.
- Ensure the provided fallback UI is safe and user-friendly; avoid exposing internal error details to end users.
- The boundary includes a targeted log path for chunk-loading failures, which is helpful in detecting stale deploys; verify logging configuration and environment if you rely on these insights in production.

---

## ChunkErrorFallback
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function ChunkErrorFallback()
```


ChunkErrorFallback renders a compact user-facing UI when a lazily loaded page chunk fails to load. It informs the user that loading failed and suggests a possible new version, then provides a Reload button that refreshes the page to fetch fresh assets.

## Remarks
ChunkErrorFallback serves as the isolated chunk-failure UX, decoupling the error presentation from the rest of the app logic. It provides a single, reusable recovery surface for code-split routes and can be swapped for a more sophisticated boundary or styled to match the application's design without altering the calling code. The lightweight abstraction keeps chunk-loading failures discoverable and recoverable while staying minimal.

## Notes
- Reload triggers a full page refresh; in-flight state may be lost.
- The component assumes a browser environment (window is used directly); consider guards or mocks for SSR/testing.
- If a different recovery strategy is desired, replace or extend this component without changing usage sites.

---

## LazyPage
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function LazyPage(
```


LazyPage is a React functional component declared as function LazyPage({ children }). The fragment shows the component receives a props object from which it destructures children, but the implementation body isn’t present. Because the rendering strategy isn’t visible, we cannot confirm whether it directly renders its children, wraps them in additional layout, or implements any lazy-loading semantics. In practice, a component named LazyPage would typically act as a lightweight wrapper around routed page content, providing a single place to apply page-level chrome or layout while delegating the actual content to its children.

## Remarks
To the extent that LazyPage is a wrapper, it provides a stable location to apply page-level chrome (layout, theming scaffolding, or loading boundaries) around routed content, without embedding those concerns into each page component.

## Notes
- Be aware of prop forwarding: if you need to pass through props to the rendered output or to a wrapper element, ensure they are forwarded to avoid silent prop loss.
- The snippet is incomplete; verify the full implementation to understand exact rendering behavior and any lazy-loading or Suspense usage.

---

## PageLoader
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function PageLoader()
```


PageLoader is a tiny React functional component that renders a loading indicator. It returns a div with className "auth-loading" containing the static text "Loading…". It is a presentational, stateless component intended to indicate a loading state during authentication or route transitions, and it does not accept props or perform side effects.

## Remarks
PageLoader serves as a centralized visual cue for authentication-related loading, enabling consistent styling via the "auth-loading" CSS class. It can be swapped out for a spinner or more elaborate loading UI without changing the call sites.

## Example
```typescript
<PageLoader />
```

## Notes
- It has no props; to customize, modify the component or provide a wrapper.
- The static text "Loading…" can hinder localization; consider parameterization or i18n if needed.

---