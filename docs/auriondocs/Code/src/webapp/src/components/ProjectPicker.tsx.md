# ProjectPicker.tsx

> **Source:** `src/webapp/src/components/ProjectPicker.tsx`

## Contents

- [MenuAnchor](#menuanchor)
- [ProjectPickerProps](#projectpickerprops)
- [ProjectPicker](#projectpicker)
- [close](#close)
- [handleNew](#handlenew)
- [loadActiveProjectId](#loadactiveprojectid)
- [onDown](#ondown)
- [onKey](#onkey)
- [openSettings](#opensettings)
- [persistActiveProjectId](#persistactiveprojectid)
- [selectProject](#selectproject)
- [toggleMenu](#togglemenu)

---

## MenuAnchor
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

```typescript
interface MenuAnchor
```


MenuAnchor is a small TypeScript interface that describes where a dropdown-like menu should appear relative to its triggering element. It captures the vertical position (top), horizontal position (left), and the anchor's width to enable precise alignment in the ProjectPicker component.

## Remarks
By encapsulating placement data in a single object, MenuAnchor decouples layout concerns from the menu rendering logic, making it easier to reuse the same anchoring contract wherever a popover is needed within the UI. It also eases testing by providing a stable shape that consumers can rely on, and it can be extended with additional metadata in the future without changing callers.

## Notes
- Interfaces in TypeScript are erased at runtime; ensure values are validated at runtime since type checks are compile-time only.
- Top/left assume a consistent coordinate space with the menu system (e.g., relative to the containing view); mismatches can cause misaligned menus.

---

## ProjectPickerProps
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

```typescript
interface ProjectPickerProps
```


ProjectPickerProps defines the props contract for the ProjectPicker component. It requires activeProjectId: string | null to reflect the currently active project (null when none is active). It exposes onActiveProjectChange: (projectId: string | null) => void to notify the parent when the user selects a different project. The optional onActiveProjectMetaChange?: (project: ProjectResponse | null) => void fires with the full active-project metadata whenever the picker resolves a new active project, enabling the parent to coordinate routing or diagnostics behavior. The optional refreshKey?: number lets the parent trigger a refetch by bumping the key; the picker will refetch on its own mutations as well.

## Remarks

The interface decouples the picker UI from the app's routing and diagnostics logic. By providing onActiveProjectMetaChange, the parent gains visibility into the selected project's full metadata, enabling it to switch between project-scoped and conversation-scoped flows (for example, which diagnostics URL to open from a chat action). The refreshKey prop provides an explicit signal to reload data after mutations, without requiring the picker to manage its entire data-fetch lifecycle.

## Notes

- The activeProjectId may be null; UI should render a 'no selection' state and handle transitions to and from null gracefully.
- onActiveProjectMetaChange is optional; callers must handle the possibility of a null argument and avoid unnecessary re-renders or navigation changes.
- refreshKey is a number that should be changed by the parent to trigger a refresh; do not reuse stale values to avoid missed updates.

---

## ProjectPicker
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function ProjectPicker(
```


ProjectPicker is a React function component that renders a user interface for selecting an active project within the web application. It receives the currently active project identifier and two callbacks to notify the parent when the active project changes or when its metadata changes, plus a refreshKey to signal a reload. Use this symbol when you need a consistent, reusable pattern for project selection rather than wiring an ad-hoc select control in each page; it centralizes how the active project is chosen and reported upward, and it provides a clear hook for triggering refreshes from the outside.

## Remarks

ProjectPicker acts as a focused boundary between the page's UI and the project-domain data. It encapsulates the common pattern of selecting and reacting to changes in the active project, while delegating the actual data fetching and state management to the parent via callbacks. This separation makes pages easier to compose and test, and it keeps the active-project flow consistent across the application.

## Notes

- Prefer stable callback references for onActiveProjectChange and onActiveProjectMetaChange to avoid unnecessary re-renders or effect reruns.
- If refreshKey changes, expect the component to refresh its internal data; avoid changing refreshKey to a random value without cause.
- The prop activeProjectId may be undefined while loading; ensure the parent handles loading state and renders a fallback UI.

---

## close
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const close = () => setMenu(null)
```


Closes the project picker menu by resetting the menu state to null through the outer setMenu function. This tiny helper provides a single-purpose action that you can attach to UI elements (for example, a close button or an outside-click handler) without duplicating setMenu(null) inline. Keeping the close logic in one place makes future changes to how the menu is dismissed easier, such as adding cleanup steps before hiding it.

## Remarks
Isolating the close action communicates intent clearly: this function represents the concept of “closing” the menu as a reusable unit. It relies on setMenu being available in the lexical scope, effectively acting as a light wrapper around the state setter rather than introducing a separate state mechanism.

## Notes
- Assumes null is the sentinel value representing "closed" for the menu; if your UI uses a different sentinel, adjust this function accordingly.

---

## handleNew
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const handleNew = async () =>
```


Prompts the user for a new project name, creates the project via ProjectsService, and upon success refreshes the UI and activates the newly created project by persisting its id and notifying listeners. Use this function as the click/handler for a 'New project' action in the ProjectPicker UI.

## Remarks

handleNew centralizes the end-to-end flow of creating a project from user input to activation, keeping UI components focused on presentation. It coordinates with ProjectsService for persistence, local state refresh via setLocalRefresh, and activation hooks via persistActiveProjectId and onActiveProjectChange; an optional onActiveProjectMetaChange callback is also invoked to surface metadata to listeners. This abstraction helps unit tests focus on behavior rather than wiring together multiple concerns across the component.

## Notes

- It relies on a browser environment (window.prompt); in non-browser environments or during SSR/tests, mock or guard against window.prompt.
- The input is trimmed and if empty after trimming, the operation aborts gracefully.
- After a successful creation, the function immediately activates the project by persisting its id and invoking the relevant callbacks; ensure listeners can handle the new active project promptly.

---

## loadActiveProjectId
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function loadActiveProjectId(): string | null
```

**Returns:** `string | null`


Reads the active project identifier from the browser's localStorage using the ACTIVE_PROJECT_KEY and returns the value as a string when available. If the item doesn't exist or access to localStorage throws (for example in non-browser environments or under strict privacy modes), it returns null. This helper is typically used by UI components to initialize or preselect the currently active project without forcing a page reload.

## Remarks
This function centralizes access to the active project key and provides a safe fallback in environments where localStorage access may fail. By returning null instead of throwing, callers can treat "no active project" as a normal case and apply their own fallback or prompt logic. It also relies on the ACTIVE_PROJECT_KEY being defined in scope, encapsulating the storage key behind a single, reusable symbol.

## Example
```typescript
// Typical usage: obtain the active project ID if one is saved
const projectId = loadActiveProjectId();
if (projectId) {
  // proceed with the active project
} else {
  // no active project saved; prompt user to select one
}
```

## Notes
- The function returns null both when the key is missing and when localStorage access fails; callers should handle null as "no active project".
- Ensure ACTIVE_PROJECT_KEY is defined in the scope where this function is used; a missing key constant could cause a compilation/runtime error.
- This only works in environments with Web Storage (i.e., a browser); during server-side rendering or in non-browser contexts, the call will return null.

---

## onDown
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const onDown = (e: globalThis.MouseEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `globalThis.MouseEvent` | — |


Handles a global mouse-down event to dismiss the Project Picker when the user clicks outside the menu and its trigger. It returns early if the click target is inside either element; otherwise, it calls close() to hide the picker.

## Remarks
This small handler centralizes the outside-click dismissal pattern for popover-like surfaces. It depends on DOM node references (menuRef and triggerRef) and a single close() action, keeping the interaction logic separate from rendering and reducing duplication across components that need similar behavior.

## Notes
- If menuRef.current or triggerRef.current are not mounted when the event fires, the containment checks won’t short-circuit, and close() may run on any outside click during initial render. Ensure proper mount order or guard close() accordingly.
- This function uses a global MouseEvent context (not React’s synthetic event); ensure it’s wired to a document/window listener and cleaned up on unmount to avoid leaks.

---

## onKey
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const onKey = (e: globalThis.KeyboardEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `globalThis.KeyboardEvent` | — |


onKey is a small keyboard event handler that closes a UI surface when the Escape key is pressed. Developers reach for this when they want a lightweight, consistent way to dismiss components (like a project picker) without wiring up a full, custom key-handling flow.

## Remarks
onKey centralizes Escape-key dismissal logic for UI overlays. It acts as a thin adapter that triggers the surrounding scope’s close() when Escape is pressed, enabling reuse across components that share the same UX. Its behavior is intentionally narrow (only Escape triggers close), so callers know exactly when the UI will close.

## Example
```typescript
// Example usage: attach as a global keydown listener
document.addEventListener('keydown', onKey);
// When cleaning up (e.g., component unmount)
document.removeEventListener('keydown', onKey);
```

## Notes
- The handler assumes a close() function exists in scope; if not, a runtime error can occur.
- Attach/detach the listener in the appropriate lifecycle to avoid leaks.

---

## openSettings
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const openSettings = () =>
```


openSettings is a concise navigation helper that takes the user to the active project’s settings screen. It guards against missing context by exiting early when there is no active project; when a project is present, it constructs a URL of the form /p/{encodedId}/settings using encodeURIComponent to ensure the ID is URL-safe, and delegates to the app’s navigation function to perform a client-side route transition.

## Remarks
By encapsulating this routing logic, the UI can trigger settings navigation without duplicating path construction or encoding rules. It also centralizes the guard against null project context, reducing the chance of runtime errors in places where openSettings might be invoked without a selected project. The approach supports consistent user experience when switching between projects and ensures that IDs with special characters are safely transmitted in the URL.

## Example
```typescript
// Navigate to the settings page of the currently active project
openSettings();
```


---

## persistActiveProjectId
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
function persistActiveProjectId(id: string | null)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `id` | `string | null` | — |


Persists the currently active project identifier in the browser's localStorage. Call this helper instead of interacting with localStorage directly to ensure a consistent key and safe failure handling. When id is a non-null string, it stores the value under ACTIVE_PROJECT_KEY; when id is null, it removes that key. The storage operation is wrapped in a try/catch that swallows errors to avoid breaking the UI if storage is unavailable or blocked.

## Remarks

By centralizing how the active project is persisted, this function decouples storage concerns from UI components and makes it easier to change the persistence strategy later (for example, switching to sessionStorage or a different key) without touching call sites. It also enforces the rule that an explicit null clears the stored value, which matters for triggering a fresh selection when the app restarts.

## Example

```typescript
persistActiveProjectId("proj-42");
```

```typescript
persistActiveProjectId(null);
```

## Notes

- Silent failures: localStorage errors are ignored, so persistence may appear to succeed while actually not storing.

---

## selectProject
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const selectProject = (id: string) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `id` | `string` | — |


selectProject orchestrates the actions that follow a user selecting a project: it persists the selected project's id, informs interested parties of the change, supplies optional project metadata to a callback if present, and closes the project picker menu.

## Remarks
Acts as a small orchestrator that coordinates persistence, event notification, and UI state in response to a project selection. By reading the project metadata from the surrounding collection (projects) and passing it to onActiveProjectMetaChange when available, it decouples lookup and consumption from the UI handler. The optional chaining on onActiveProjectMetaChange makes the metadata callback a non-breaking extension point.

## Notes
- If the selected id isn't present in the projects array, the metadata callback receives null, so listeners should handle that case.
- There is no explicit error handling shown here; failures from persistActiveProjectId or from listener callbacks would propagate to the caller and may require additional guards in a real-world scenario.

---

## toggleMenu
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const toggleMenu = () =>
```


toggleMenu toggles a dropdown-like menu anchored to a trigger element. If the menu is already open, it closes it by resetting the menu state to null; otherwise it reads the trigger button's position from the DOM and opens the menu positioned just below the button, with the same width as the button to preserve alignment.

## Remarks

Encapsulates the common popover pattern: a trigger toggles a menu, and its opening position is anchored to the trigger's bounding rectangle. By capturing top, left, and width at open time, the menu remains visually aligned with the trigger; note that this does not handle edge collisions or dynamic repositioning.

## Notes

- The function exits early if btn is null (e.g., before the trigger is mounted).
- The width is captured on open and won't adjust if the trigger resizes after opening.


---