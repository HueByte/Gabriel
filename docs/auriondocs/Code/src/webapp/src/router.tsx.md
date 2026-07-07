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


ErrorBoundaryProps specifies the props for an error boundary component, requiring a children node to render in normal operation and a fallback node to render when an error occurs. This contract makes the boundary reusable and predictable across different parts of the UI.

## Remarks
By expressing this contract as a dedicated interface, the boundary's UI and its error-handling behavior remain loosely coupled: the boundary can swap how errors are presented without changing its consumer code. Since both children and fallback are ReactNode, any renderable content — elements, strings, fragments, or components — can be supplied, enabling flexible and consistent error presentation across the app. This interface communicates a clear separation of concerns: the boundary handles errors, while the props decide what to show in both success and failure states.

## Notes
- ReactNode includes null and undefined; if you pass such values for either prop, the render may be empty. Provide meaningful content for fallback or children to avoid a blank UI.
- This interface does not implement the boundary logic itself; it merely defines the shape of props consumed by a separate error boundary component.

---

## ErrorBoundaryState
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** interface

```typescript
interface ErrorBoundaryState
```


Represents the shape of the state used by an error boundary in the web app router. It exposes a single boolean flag, hasError, that signals whether an error has been captured and whether a fallback UI should be rendered instead of the normal children.

## Remarks
This interface acts as a small contract describing error-boundary state. It separates concerns by letting components focus on rendering vs. error handling, and it can be reused across multiple boundaries within the router to keep state representation consistent.

## Notes
- The interface only defines the shape; it does not implement any error-handling logic.
- In React error boundaries, the hasError flag is typically toggled in componentDidCatch and can drive conditional rendering of a fallback UI.
- Mutating hasError directly is not the correct pattern in React; update state through the component's state management (e.g., setState).

---

## ErrorBoundary
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** class

```typescript
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState>
```


ErrorBoundary is a React class component that implements the standard error boundary pattern. It guards a subtree by catching rendering-time and lifecycle errors, toggles an internal hasError state via getDerivedStateFromError, and renders the provided fallback UI when an error occurs; otherwise it renders its children. In componentDidCatch, it distinguishes chunk-loading failures from other errors: a chunk-loading error logs a concise message suggesting a new version may be available, while other errors are logged with the React error information. This boundary enables a resilient UI by preventing a single failure from crashing the entire subtree and by presenting a stable fallback UI to users.

## Remarks
Error boundaries address the reality that JavaScript errors thrown during rendering or lifecycle methods can crash a portion of the UI. By isolating error handling to this boundary, developers gain a predictable recovery point and centralized diagnostics for issues within a subtree. The boundary also centralizes a small, targeted logging path for deployment-related chunk-loading issues, helping distinguish stale-cache scenarios from ordinary runtime errors without cluttering logs with stack traces for everyday failures.

## Notes
- The chunk-loading diagnostic path relies on the error message containing "Failed to fetch dynamically imported module"; if the bundler or environment changes this message, the targeted log may not fire. Consider extending the check or centralizing error categorization if you rely on this pattern across environments.

---

## ChunkErrorFallback
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function ChunkErrorFallback()
```


ChunkErrorFallback is a minimal React function component that renders a small, vertically stacked error UI when a dynamically loaded chunk fails to load. It displays a concise message: “Failed to load page. A new version may be available.” and provides a Reload button that, when clicked, triggers a full page refresh via window.location.reload(). This component is a self-contained presentational fallback intended for use in code-splitting scenarios (lazy-loaded routes) where a chunk load error should offer the user a straightforward recovery path without introducing complex retry logic.

## Remarks
ChunkErrorFallback isolates the user-facing recovery path from the rest of the UI. By offering a single reload action, it ensures the browser fetches the latest application bundle, which is often the simplest remedy for stale or corrupted chunks. It is intentionally presentational: it does not attempt automatic retries or state recovery, leaving such concerns to higher-level error handling strategies.

## Notes
- Reload may cause loss of unsaved data; consider warning the user or integrating with a broader recovery strategy.
- This component relies on window and DOM availability and will not run in non-browser contexts or during server-side rendering unless used only on the client.


---

## LazyPage
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function LazyPage(
```


LazyPage is a React functional component that receives a single prop, children, and acts as a wrapper around its content within the router. It provides a named, semantic wrapper you can rely on when composing routes or applying consistent layout around pages.

## Remarks
Providing a dedicated LazyPage wrapper isolates routing content from layout concerns and future enhancements. It acts as a conceptual boundary at the routing layer, making it easier to introduce cross-cutting behaviors (such as layout wrappers or lazy-loading strategies) without modifying individual route components. This separation also helps with testing and readability by giving routes a stable wrapper anchor.

## Notes
- Keep this wrapper lean; avoid introducing side effects or internal state unless explicitly needed.
- If you extend it with additional behavior, ensure props contain no mutable shared state and respect React's rendering semantics.

---

## PageLoader
> **File:** `src/webapp/src/router.tsx`  
> **Kind:** function

```typescript
function PageLoader()
```


Renders a lightweight authentication loading indicator used during route transitions or initial authentication checks. Use PageLoader when you need a consistent, minimal loading state without duplicating markup at call sites.

## Remarks
PageLoader is a pure presentational component with no props or local state. It centralizes the UX of the authentication-loading state behind a single CSS class, enabling styling changes in one place without touching call sites. If you later replace the implementation with a richer spinner or add accessibility improvements, you can swap it here without cascading changes.

## Notes
- The element is a plain div and currently lacks explicit ARIA live semantics; consider adding role="status" and aria-live="polite" to announce updates to assistive technologies.
- The text 'Loading…' is not localized; consider extracting it to a translation key if internationalization is required.
- Styling is delegated to the .auth-loading CSS rule; ensure adequate contrast and responsive behavior across themes.

---