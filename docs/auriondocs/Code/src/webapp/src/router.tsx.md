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


ErrorBoundaryProps defines the props contract for an error boundary component in this codebase. It requires two renderable values: children, the content to render under normal operation, and fallback, the UI to render when an error occurs within the subtree. This interface exemplifies the standard React error-boundary pattern by separating normal rendering from error UI and enabling strong typing for boundary usage.

## Remarks
This interface isolates error-handling concerns from rendering logic by providing a clear, strongly-typed boundary contract. Requiring a concrete fallback ensures a predictable UX when an error is caught, rather than letting the error bubble up. The ReactNode type broadens what can be rendered for both children and fallback, supporting simple text, complex elements, or even null.

## Notes
- Both props are required by the type; consumers cannot omit them at compile time.
- ReactNode covers a wide range of renderable values, so design the fallback and children to be resilient to different shapes (elements, strings, or null).

---

## ErrorBoundaryState
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

```typescript
interface ErrorBoundaryState
```


ErrorBoundaryState defines the minimal, compile-time contract for the state of an error boundary component: a single hasError flag that signals when an error has been caught so the UI can render a fallback. Use this interface to type the boundary’s state rather than relying on ad-hoc object shapes, ensuring consistent error-path rendering.

## Remarks

By modeling the error flag behind an interface, the boundary's implementation can focus on the semantics of error handling rather than the particulars of how state is stored. It pairs with error-handling hooks in the boundary to drive the conditional render path. Remember: TypeScript interfaces exist only at compile time and have no runtime footprint; the actual state object must be created and conform to ErrorBoundaryState at runtime according to your framework's conventions.

## Notes

- This interface has no runtime footprint; a concrete object that conforms to ErrorBoundaryState must be created to represent the boundary's state at runtime.
- If you extend this shape with additional fields, keep mutation and update patterns in sync so the boundary's render path remains predictable.


---

## ErrorBoundary
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** class

```typescript
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState>
```


ErrorBoundary is a React class component that acts as a protective wrapper around its children, catching rendering errors within its subtree and rendering a fallback UI instead of crashing the entire application. It uses a static getDerivedStateFromError to flip hasError to true, and logs errors via componentDidCatch, with a special-case for chunk-loading failures to surface clearer logs when a new version may be available. When an error is detected, render returns the provided fallback UI; otherwise, it renders its children.

## Remarks
Error boundaries isolate rendering errors to a subtree, preventing a failure in one part of the UI from crashing the whole page. This implementation includes a focused log path for stale chunk scenarios by checking the 'Failed to fetch dynamically imported module' message, helping diagnose deployment-related issues. It relies on ErrorBoundaryProps and ErrorBoundaryState to drive whether to render the fallback or the children.

## Notes
- Catches errors during rendering, lifecycle methods, and constructors of the subtree; does not automatically catch errors in event handlers.
- The static getDerivedStateFromError updates the boundary state, while componentDidCatch handles logging and diagnostics.
- The chunk-loading error path is a heuristic to surface deployment-related issues and may vary across environments.

---

## ChunkErrorFallback
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function ChunkErrorFallback()
```


ChunkErrorFallback is a tiny React function component that renders a browser-styled error UI when a lazily-loaded route chunk fails to load. It presents a brief message and a Reload button that performs a full page refresh to fetch a fresh version of the application.

## Remarks
The component encapsulates a straightforward recovery UX so callers don't need to implement their own retry logic for failed dynamic imports. By offering a single, deterministic action—reload—it avoids partial or inconsistent state that can occur after a failed chunk load. The UI is self-contained and reusable across different error boundaries or routes.

## Example
```typescript
// Simple usage
<ChunkErrorFallback />
```

## Notes
- This component relies on browser APIs (window). It is safe for client-side rendering, but rendering on the server will not trigger the onClick handler and may require guards in SSR setups.
- Triggering a full reload clears in-memory state; consider a more nuanced retry strategy if preserving user data is important.

---

## LazyPage
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function LazyPage(
```


LazyPage is a React function component that accepts a props object containing a children property. Based on the visible signature, it appears to function as a lightweight wrapper around its children, intended for use within the router configuration in src/webapp/src/router.tsx. The snippet does not reveal how it renders its content or any side effects, so its exact behavior cannot be determined from this fragment alone.

## Remarks
Wrapper-style components like LazyPage are often used to isolate layout concerns from individual routes. Naming a wrapper around routed content communicates intent and makes it easier to apply consistent wrappers, suspense boundaries, or lazy-loading behavior across multiple pages without duplicating boilerplate.

## Notes
- To preserve type safety in TypeScript, define children as React.ReactNode (instead of any) and consider making it optional if not every route renders children.

---

## PageLoader
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function PageLoader()
```


PageLoader is a small, presentation-only React function component that renders a loading indicator: a div with the className auth-loading and the text Loading…. It provides a reusable UI fragment you can drop in during authentication flows or page transitions to signal that content is loading, avoiding inline markup duplication.

## Remarks
This is deliberately presentation-focused and has no state or props. It centralizes the loading UI behind a single component and a CSS hook (auth-loading), making it easy to swap in a different loader or apply consistent styling across the app without touching call sites.

## Example
```typescript
<PageLoader />
```

## Notes
- The component is stateless and always renders the same markup; for dynamic messaging or progress, parameterize or replace with a different loader.
- Ensure the auth-loading CSS exists in your project to style the indicator; without styling, the loader may be unstyled or invisible.

---