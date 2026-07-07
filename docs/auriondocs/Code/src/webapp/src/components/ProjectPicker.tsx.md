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


MenuAnchor describes the geometric descriptor used to anchor a UI element (such as a dropdown or popup) to a reference point in the screen layout. It captures the anchor's position with top and left coordinates and its horizontal extent via width. By wrapping these three numbers in a single interface, it allows layout and rendering code to communicate anchor geometry without relying on DOM specifics.

## Remarks
This interface serves as a minimal contract for positioning logic, decoupling measurement from rendering. It enables reusable placement helpers to operate on a simple, testable data shape and makes it easy to mock anchor geometry in tests. In the context of UI components like ProjectPicker.tsx, MenuAnchor provides the concise geometry needed to align menus with their trigger element without exposing DOM details throughout the codebase.

## Example
```typescript
const anchor: MenuAnchor = { top: 120, left: 32, width: 240 };
// Example usage: apply anchor to a popover's inline styles (conceptual)
const style = {
  position: 'absolute' as const,
  top: anchor.top,
  left: anchor.left,
  width: anchor.width
};
```

## Notes
- The values are numbers, typically representing pixels; maintain consistent units when applying to CSS.
- Height is not represented; if vertical sizing is needed, consider extending the interface with a height field.
- Ensure the top/left coordinate space matches the intended offset container (e.g., viewport vs. containing element) to avoid misplacement.

---

## ProjectPickerProps
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

```typescript
interface ProjectPickerProps
```


ProjectPickerProps defines the props that the ProjectPicker component expects. It includes the current active project's ID (activeProjectId: string | null), a callback to set a new active project (onActiveProjectChange: (projectId: string | null) => void), and two optional members: onActiveProjectMetaChange, which receives the full active-project metadata as a ProjectResponse whenever the picker resolves the active project, and refreshKey, a number that, when incremented, triggers a refetch of the picker’s data. This interface enables a parent to react to user interactions and to align navigation or UI state with the picker’s active project context.

## Remarks

This interface encapsulates the contract between the ProjectPicker and its consumer, enabling the parent to stay synchronized with the selected project and, when available, the full project metadata needed for routing or context-sensitive UI decisions (such as choosing the correct diagnostics view). The optional refreshKey offers a lightweight mechanism to force the picker to re-fetch data without requiring parent-managed state changes.

## Notes

- activeProjectId may be null to indicate no project is currently active; ensure UI handles an empty state.
- If provided, onActiveProjectMetaChange will be invoked with a ProjectResponse when the active project is resolved; it may be invoked with null if there is no active project.
- refreshKey: changing its value triggers a refetch; stable values may not cause a refresh.

---

## ProjectPicker
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function ProjectPicker(
```


ProjectPicker is a React function component that renders a user interface for selecting the active project within the application. It accepts the props activeProjectId, onActiveProjectChange, onActiveProjectMetaChange, and refreshKey to control its behavior. When a user selects a different project, the component signals the new selection through onActiveProjectChange, and it propagates any updated project metadata via onActiveProjectMetaChange. The refreshKey prop serves as a trigger for reloading the picker’s data, allowing the parent to refresh the list or metadata when needed.

## Remarks
ProjectPicker encapsulates the project-selection UX, decoupling it from page-level state. It coordinates with its parent to keep the active project and its metadata in sync while giving the parent the authority to refresh data by altering refreshKey. This abstraction makes it easy to reuse the same picker across different pages or contexts without duplicating selection logic.

## Example
```typescript
<ProjectPicker
  activeProjectId={selectedProjectId}
  onActiveProjectChange={setSelectedProjectId}
  onActiveProjectMetaChange={setSelectedProjectMeta}
  refreshKey={projectsRefreshKey}
/>
```

## Notes
- Changing refreshKey should be used to trigger a data reload; ensure the parent increments it thoughtfully to avoid unnecessary reloads.
- If activeProjectId is undefined, provide a clear UI prompt (e.g., "Select a project") to avoid an empty picker state.
- Pass stable callback references from the parent to prevent unnecessary re-renders; consider wrapping callbacks with useCallback if needed.

---

## close
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const close = () => setMenu(null)
```


Closes the ProjectPicker menu by resetting the menu state to null. This lightweight helper is used whenever the UI needs to hide the menu (for example after a selection is made or when the user clicks outside). It keeps the intent of closing the menu explicit and readable, and allows callers to pass a single, reusable callback instead of duplicating the state mutation.

## Remarks
By encapsulating the close action, this symbol keeps event handlers concise and makes it straightforward to swap or extend closing behavior in one place without updating every caller. It fits a pattern of exposing small, purpose-driven state mutations as named helpers to improve readability and maintainability of UI state management.

## Example
```tsx
// Most common usage: attach as a click handler to close the menu
<button onClick={close}>Close Menu</button>
```

## Notes
- Be mindful that this closes the menu by setting the state to null; if the state shape changes, update this helper accordingly.
- If multiple menus exist, ensure this close function targets the intended menu state (i.e., align with the correct setter, not a different or shared one).


---

## handleNew
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const handleNew = async () =>
```


Prompts the user for a new project name, creates the project through the API, and on success immediately activates it by refreshing the UI state, persisting the active project id, and notifying the relevant listeners. If the user cancels or submits an empty name, the operation is aborted; if the API call fails, an error notification is shown.

## Remarks
Centralizes the create-and-activate flow for projects in the ProjectPicker. It ensures the newly created project becomes the active context by coordinating API calls, local persistence, and callback invocations; it also forwards any returned metadata via onActiveProjectMetaChange when provided.

## Notes
- Prompts rely on window.prompt; in non-browser environments or testing, this can block or be unavailable.
- The onActiveProjectMetaChange callback is optional; callers should provide it if they need to receive the created project's metadata.
- Error handling uses a generic notifyError with a fixed message; consider expanding handling for known error shapes from ProjectsService.

---

## loadActiveProjectId
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function loadActiveProjectId(): string | null
```

**Returns:** `string | null`


Loads the ID of the currently active project from the browser's localStorage. It reads the value stored under ACTIVE_PROJECT_KEY and returns it as a string, or null if the key is missing or if accessing localStorage fails.

## Remarks
Provides a safe, synchronous read of the active project identifier from client storage, hiding error handling from callers and preventing UI crashes when storage is unavailable. It complements the storage key constant and the ProjectPicker UI by giving a single, reusable way to obtain the currently selected project.

## Notes
- Null can indicate either 'no active project saved' or 'storage access failed', so callers should handle null explicitly.
- This function is client-side only; environments without localStorage (SSR) will fall back to null without throwing.

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


Closes a dropdown-like UI when the user clicks outside the menu and its trigger. This function acts as a global mouse-down handler that returns early if the click target is contained within either the menu or the trigger; otherwise it calls close() to dismiss the UI.

## Remarks
Centralizes outside-click dismissal logic for dropdown-style components by checking containment against both the menu and its trigger refs. This ensures interactions inside the component don’t inadvertently close it while outside clicks reliably close it, keeping open/close semantics consistent across usage scenarios.

## Example
```typescript
// Common usage: attach a global mousedown listener to close the picker when clicking outside
document.addEventListener('mousedown', onDown);

// Remember to detach when the component unmounts or is no longer needed
document.removeEventListener('mousedown', onDown);
```

## Notes
- Attach/detach the listener carefully to avoid memory leaks; if onDown is recreated on each render, memoize it to ensure the listener remains stable.
- The checks tolerate unmounted refs (due to optional chaining); if refs are not yet mounted, the handler safely does nothing more than returning.
- The behavior assumes close() is defined in scope and performs the actual UI dismissal; ensure it’s accessible from the handler’s context.

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


Handles a keyboard event to close the ProjectPicker when the Escape key is pressed. Use this small handler to provide keyboard dismissal for overlays, modals, or dropdowns by wiring it to a keydown listener.

## Remarks
This tiny function encapsulates the Escape-to-close behavior, so the closing logic stays decoupled from presentation. By centralizing the handling, the same semantics can be applied consistently wherever the ProjectPicker is used, aiding accessibility and predictable UX. It also simplifies testing, since the close invocation can be verified in isolation.

## Example
```typescript
// Attach Escape-to-close behavior globally
window.addEventListener('keydown', onKey);

// Cleanup should be performed when the component unmounts
window.removeEventListener('keydown', onKey);
```

## Notes
- Ensure you clean up the listener to avoid leaks.
- In some environments, different keys or key values may be reported; prefer e.key === 'Escape' for clarity.
- If the focus is inside a child input, you may want to scope the listener or conditionally enable it to avoid interfering with normal typing.

---

## openSettings
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const openSettings = () =>
```


openSettings navigates to the active project's settings screen. It will no-op if no project is currently active and safely constructs the URL by encoding the project id before using navigate to update the route.

## Remarks
Centralizes the routing logic for the project settings flow, so callers don't need to assemble the path or worry about URL encoding. It ties the navigation action to the presence of an active project, ensuring a consistent user experience when a project is selected. This isolation also makes it easier to test and reuse the navigation behavior from multiple UI triggers within the ProjectPicker component or nearby UI.

## Notes
- Calling openSettings with no active project results in no navigation; this is a silent no-op. Guard callers or provide a user-facing cue if needed.
- It assumes a router/navigation function is in scope; ensure the proper router context is present to avoid runtime errors.

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


Persist the active project ID to localStorage so the user's selection persists across page reloads. Call this when the user selects a project. If a non-null id is provided, it is written under ACTIVE_PROJECT_KEY; if null (or an empty string, due to the falsy check) is provided, the key is removed. Any errors during storage are swallowed to avoid impacting the UI.

## Remarks
This function acts as a tiny persistence adapter for the active project. It centralizes the storage key usage and error handling, so all parts of the UI share a single, predictable means of saving and clearing the active project.

## Example
```typescript
persistActiveProjectId("proj-123"); // stores the active project id
persistActiveProjectId(null); // clears the stored value
```

## Notes
- Silent failures: storage errors are swallowed; if persistence failures matter in your scenario, consider surfacing them or returning a status.
- Empty string or null clears the value; if you need to store an empty string, this function won't distinguish it from removal.
- LocalStorage is browser-specific; in non-browser environments or privacy modes, this may be a no-op or throw, but the try/catch will swallow it.

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


selectProject updates the active project by id and triggers the associated side effects. It persists the chosen id, notifies listeners about the active project change, optionally provides the selected project's metadata to onActiveProjectMetaChange, and then closes the menu.

## Remarks
By funneling all project-selection side effects through a single function, this symbol decouples the UI from the data-layer and event-handling logic. It coordinates persistence, change notification, metadata provisioning, and UI state (menu visibility) in one place, reducing duplication across the component and its consumers.

## Example
```typescript
// Typical usage in a click handler
<button onClick={() => selectProject(project.id)}>
  Use this project
</button>
```

## Notes
- If no matching project is found, onActiveProjectMetaChange receives null.
- onActiveProjectMetaChange is optional; the function checks with ?. before invoking.
- setMenu(null) closes the menu; if you need different behavior, adjust after calling selectProject.


---

## toggleMenu
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const toggleMenu = () =>
```


toggleMenu is a UI helper that toggles the visibility of a contextual menu anchored to a trigger element. When invoked, it closes the menu if it is already open by clearing the menu state; otherwise it reads the current trigger button from triggerRef, and if the button exists, computes the menu’s position from the button’s bounding rectangle and opens the menu just below the button with the same width.

## Remarks
This function centralizes the toggling and positioning logic for a dropdown anchored to its trigger, ensuring consistent placement across invocations and re-renders. It cleanly separates the concerns of showing/hiding the menu from the rendering of the menu itself, enabling the menu component to rely on a simple position object (top, left, width) provided by this helper.

## Example
```typescript
// Typical usage inside a component
<button ref={triggerRef} onClick={toggleMenu}>
  Projects
</button>
```

## Notes
- If triggerRef.current is null, or the button element is not mounted, the function exits gracefully without throwing.
- The menu is positioned using the trigger element’s bounding client rectangle, offset slightly downward (bottom + 4) and stretched to the button’s width to align visually with the trigger.

---