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

A small data shape describing the rectangular anchor used to position a floating menu: numeric top and left coordinates and a width. Use this when you need to pass an element's position/size to a menu or popup component instead of a DOMRect.

## Remarks
MenuAnchor is a simple DTO that captures the minimal geometry required to place a menu: the x/y origin (top, left) and the horizontal size (width). It exists to decouple menu positioning logic from DOM-specific types and to make it easy to serialize/store the anchor information.

## Example
```typescript
// create an anchor from a DOMRect and apply it to an absolutely positioned menu
const rect = triggerElement.getBoundingClientRect();
const anchor: MenuAnchor = { top: rect.top, left: rect.left, width: rect.width };

const menuStyle = {
  position: 'absolute',
  top: `${anchor.top}px`,
  left: `${anchor.left}px`,
  width: `${anchor.width}px`,
};
```

## Notes
- Values are plain numbers (unitless); consumers typically append a unit such as "px" when using them in CSS.
- The coordinate space (viewport vs. document) depends on how the measurements were taken; MenuAnchor does not normalize or validate that context.

---

## ProjectPickerProps

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

Props describing how a parent component controls and observes the ProjectPicker UI. Use this interface when the parent owns which project is considered "active" and needs callbacks for when that active project changes or when the picker should refetch its project list.

## Remarks
This interface separates the minimal control surface (activeProjectId + onActiveProjectChange) from optional metadata and refresh behavior. onActiveProjectMetaChange is provided for parents that need the full ProjectResponse whenever the picker determines the active project (initial load, user selection, or an auto-activated newly created project). refreshKey is a simple numeric hint the parent can bump to force the picker to refetch its list; the picker also performs its own refetches after internal mutations, so refreshKey is intended for cross-component coordination (for example, when a project is created elsewhere).

## Example
```typescript
function ParentRoute() {
  const [activeProjectId, setActiveProjectId] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const handleActiveMeta = (proj: ProjectResponse | null) => {
    // update route behavior (e.g. diagnostics URL) based on full project metadata
    console.log('active project metadata:', proj);
  };

  return (
    <ProjectPicker
      activeProjectId={activeProjectId}
      onActiveProjectChange={setActiveProjectId}
      onActiveProjectMetaChange={handleActiveMeta}
      refreshKey={refreshKey}
    />
  );
}
```

## Notes
- activeProjectId is nullable; the picker may report null when no project is selected or when the selection is cleared.
- onActiveProjectMetaChange is optional and will be invoked on initial resolution of the active project, on user-driven changes, and when a newly created project becomes active; parents that only care about the id can omit it.
- Increment refreshKey (e.g. setRefreshKey(k => k + 1)) to request a fresh fetch from the parent; the picker will still refetch on its own internal changes, so this is for external coordination only.


---

## ProjectPicker

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function ProjectPicker(
```


A controlled React component that presents a project-selection UI and reports changes back to its parent. Use this when the parent needs to drive which project is selected (via activeProjectId) and respond to selection or metadata changes through the provided callbacks.

## Remarks
This component is intended to be used as a controlled input: the activeProjectId prop is the authoritative source of the currently selected project and should be updated by the parent in response to onActiveProjectChange. The onActiveProjectMetaChange callback is provided for cases where additional project metadata (beyond the ID) needs to be surfaced to the parent. The refreshKey prop allows the parent to force the component to refresh its internal list or state when external data changes.

## Example
```typescript
<ProjectPicker
  activeProjectId={currentProjectId}
  onActiveProjectChange={(newId) => setCurrentProjectId(newId)}
  onActiveProjectMetaChange={(meta) => setProjectMeta(meta)}
  refreshKey={projectsVersion}
/>
```

## Notes
- The component is controlled: failing to update activeProjectId in response to onActiveProjectChange will leave the visible selection out of sync with user intent.
- Treat onActiveProjectMetaChange as a notification hook; the parent should not assume it is called on every render but only when relevant metadata changes.

---

## close

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const close = () => setMenu(null)
```


Sets the component's menu state to null to close or hide the menu. Use this as a simple event handler or callback when you want to explicitly close the project picker menu.

## Remarks
This is a tiny convenience wrapper around setMenu(null) that makes call sites more readable (close vs. setMenu(null)). It captures the setMenu updater from the component scope and is intended to be used as an onClick/onClose-style handler inside the ProjectPicker component.

## Example
```typescript
// Common usages inside the component
<button onClick={close} aria-label="Close menu">Close</button>
// or passing as a callback
<Menu onClose={close} />
```

## Notes
- The function is recreated on each render; use React.useCallback if a stable reference is required when passing to deep children.
- setMenu must exist in the enclosing scope (typically a state updater from useState).
- React state updates are asynchronous — reading the menu state immediately after calling close may still show the old value.

---

## handleNew

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Prompts the user for a project name, creates the project via ProjectsService, and — on success — increments a local refresh counter, persists and activates the new project's id, and notifies parent callbacks about the active project and its metadata. On failure it reports the error through notifyError.

## Remarks
This helper centralizes the user interaction and the sequence of side effects required when creating a new project: collecting a name, calling the API, updating local UI refresh state, making the new project the active one (both persisted and via callbacks), and surfacing errors. Keeping these steps together ensures consistent behavior whenever the UI needs to create-and-activate a project.

## Example
```typescript
// Typical usage inside a React component's JSX
<button type="button" onClick={handleNew}>New project</button>
```

## Notes
- The function uses window.prompt and returns early if the user cancels or provides an empty name (after trim).
- It performs several side effects (setLocalRefresh, persistActiveProjectId, onActiveProjectChange, onActiveProjectMetaChange) — callers should be aware these will change app state and persist the active project id.
- No loading state or cancellation is provided here; long-running requests or concurrent creations are not throttled by this function and should be handled by the caller or surrounding UI if needed.

---

## loadActiveProjectId

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Returns the active project identifier previously stored under ACTIVE_PROJECT_KEY in localStorage, or null when the key is missing or access to localStorage fails. Use this helper instead of accessing localStorage directly when initializing UI state or restoring a persisted project selection, as it safely handles environments where localStorage is unavailable or throws.

## Remarks
This small abstraction centralizes the single responsibility of reading the persisted active project id and swallowing any exceptions that arise from localStorage access (for example: private browsing restrictions, disabled storage, or server-side rendering where window.localStorage is absent). It depends on the ACTIVE_PROJECT_KEY constant being defined elsewhere and intentionally returns the raw string value exactly as stored.

## Example
```typescript
import { loadActiveProjectId } from './ProjectPicker';

const activeProjectId = loadActiveProjectId();
if (activeProjectId) {
  // restore previously selected project
  selectProject(activeProjectId);
} else {
  // fall back to default behavior
  showProjectSelectionUI();
}
```

## Notes
- Returns null both when the key is not present and when any exception occurs while accessing localStorage.
- The function returns the raw string value; parse it if you stored JSON or another structured format.
- Errors from localStorage are swallowed (no logging). If you need diagnostics, wrap calls at the call site or modify this function to surface/log errors.

---

## onDown

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Closes the picker when the user presses the mouse button (or otherwise "downs") outside of the picker menu or its trigger element. Attach this as a document-level down/mouse listener to dismiss the menu whenever interaction happens outside the menu or trigger.

## Remarks
This function relies on menuRef and triggerRef (and a close() function) from the surrounding scope: it checks whether the event target is contained inside either element and only calls close() when the interaction happened outside both. It's intended as a simple outside-click detector used to hide a dropdown/overlay without interfering with interactions inside the control or its activator.

## Example
```typescript
// Typical usage inside a React component — ensure `onDown` is stable (useCallback) so the listener can be removed.
useEffect(() => {
  document.addEventListener('mousedown', onDown);
  return () => document.removeEventListener('mousedown', onDown);
}, [onDown]);
```

## Notes
- The handler casts event.target to Element; EventTarget can be other node types but Element.contains accepts a Node, so the runtime behavior is generally safe — the TypeScript cast is just a narrow typing choice.
- Prefer making `onDown` a stable reference (e.g. useCallback) when you add/remove it as a DOM listener; otherwise removeEventListener may not work as expected.
- For touch devices you may want to use `pointerdown` instead of `mousedown` to cover touch interactions.
- If menuRef.current or triggerRef.current are removed from the DOM before the event runs, contains will return false and the handler will call close(); design accordingly if elements are unmounted during interaction.


---

## onKey

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

A small keyboard event handler that closes the enclosing UI when the user presses the Escape key. Use this when you want a single, focused function to detect Escape and invoke a previously defined close() action (for example to dismiss a modal or picker).

## Remarks
This handler is intentionally minimal: it checks the event.key string for the Escape key and calls close() from the surrounding scope. It is intended to be attached directly to a keyboard event target (window or a specific element) and removed when no longer needed, rather than performing additional event management like preventing default behavior or stopping propagation.

## Example
```typescript
// Attach on mount and clean up on unmount in a React component
useEffect(() => {
  window.addEventListener('keydown', onKey);
  return () => window.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- onKey closes over a close() symbol from its outer scope — ensure close is stable (or memoized) if you pass onKey to addEventListener inside effects.
- The handler does not call e.preventDefault() or e.stopPropagation(); if you need to suppress default Escape behavior you must do so explicitly.
- Some older environments used the key value 'Esc' instead of 'Escape'. If you need to support such legacy cases, check both values.

---

## openSettings

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Navigates the application to the settings page for the currently selected project. Call this when you need to programmatically send the user to the active project's settings view (for example from a button click in a project picker).

## Remarks
This helper expects activeProject and a navigate function to be available in the enclosing scope; it returns early if no project is selected. The project id is passed through encodeURIComponent before being appended to the route, ensuring the URL remains valid for ids that contain reserved characters.

## Example
```typescript
// Typical usage as a click handler inside a React component
<button onClick={openSettings}>Open Project Settings</button>
```

## Notes
- If activeProject is falsy, the function does nothing (no navigation occurs).
- The function performs no checks for unsaved changes or confirmation prompts before navigating.
- It assumes `navigate` behaves like a react-router navigation function (or similar) that accepts a path string.

---

## persistActiveProjectId

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Persist the currently active project id in browser localStorage (under the ACTIVE_PROJECT_KEY). Call this from UI code when the selected project changes so the choice is remembered across page reloads or future visits; pass null to remove the stored value.

## Remarks
This small helper centralizes the read/write semantics for the "active project" preference and defensively guards against environments where localStorage is unavailable or throws (e.g., server-side rendering, private browsing with storage disabled, or quota errors). It deliberately swallows exceptions to avoid bubbling storage errors into the UI flow.

## Example
```typescript
// Save an active project id
persistActiveProjectId('project-123');

// Clear the stored active project
persistActiveProjectId(null);
```

## Notes
- The function treats any falsy id (including empty string) as a signal to remove the key — an empty string will remove the stored value rather than store an empty string.
- Errors from localStorage (quota, disabled storage, lack of window/localStorage in non-browser environments) are caught and ignored; consider adding logging if you need visibility into failures.
- This is synchronous and has no return value.

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


Selects a project by its id and performs the side effects required by the ProjectPicker: it persists the active project id, notifies the active-project change handler, updates the active project metadata via an optional callback, and closes the project menu.

## Remarks
This helper centralizes the sequence of actions that must happen when the user picks a project: persistence, notification, metadata propagation, and UI state change. Keeping those steps together ensures a consistent order (persist -> notify -> update meta -> close UI) so consumers of the callbacks see a stable state. The function relies on surrounding scope values and callbacks (persistActiveProjectId, onActiveProjectChange, onActiveProjectMetaChange, projects, setMenu).

## Example
```typescript
// typical usage inside a click handler for a project item
function onProjectClick(projectId: string) {
  selectProject(projectId);
}
```

## Notes
- onActiveProjectMetaChange is optional; if not provided, the meta update step is skipped.
- The metadata callback receives the matching project object from the local projects array, or null if no match is found.
- This function performs multiple side effects (persistence, callbacks, UI mutation). Review the implementations of the provided callbacks if you need different ordering or asynchronous behavior.

---

## toggleMenu

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Toggles a popup/menu anchored to a trigger element: if a menu is open it closes it (by calling setMenu(null)), otherwise it measures the trigger button and opens the menu positioned just below the button with the same left offset and width (top = button.bottom + 4, left = button.left, width = button.width).

## Remarks
This function is a small UI helper meant to be used as a click handler for a trigger (for example a button) that should open or close a floating menu or popover. It relies on a triggerRef pointing to the DOM element and setMenu/menu state in the same component scope; position values are computed from getBoundingClientRect so the menu is anchored to the element in viewport coordinates.

## Example
```typescript
// inside a React component
const [menu, setMenu] = useState<{ top: number; left: number; width: number } | null>(null);
const triggerRef = useRef<HTMLButtonElement | null>(null);

const toggleMenu = () => {
  if (menu) { setMenu(null); return; }
  const btn = triggerRef.current;
  if (!btn) return;
  const rect = btn.getBoundingClientRect();
  setMenu({ top: rect.bottom + 4, left: rect.left, width: rect.width });
};

return (
  <>
    <button ref={triggerRef} onClick={toggleMenu}>Open</button>
    {menu && (
      <div style={{ position: 'absolute', top: menu.top, left: menu.left, width: menu.width }}>
        {/* menu contents */}
      </div>
    )}
  </>
);
```

## Notes
- toggleMenu depends on triggerRef.current being a mounted DOM element; it does nothing if the ref is null.
- This runs only on the client (uses getBoundingClientRect); avoid calling during server-side rendering.
- If you memoize toggleMenu (useCallback), include menu and setMenu in the dependency list to prevent stale closures.

---