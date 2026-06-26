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

Represents a simple rectangular anchor used to position a floating menu or popover. It carries the numeric top/left coordinates and the anchor element's width so the consumer can compute placement and sizing for a menu without needing the DOM node itself.

## Remarks
This interface decouples menu placement logic from DOM lookups: callers can pass a plain data object describing where the anchor is located (and how wide it is) to any rendering or positioning utility. That makes it easy to calculate styles or transform coordinates in environments where you already computed bounding information (for example via getBoundingClientRect) or when the DOM node is not directly available.

## Example
```typescript
const anchor: MenuAnchor = { top: 120, left: 240, width: 200 };
// Use to build inline styles for a floating menu
const menuStyle = {
  position: 'absolute',
  top: `${anchor.top}px`,
  left: `${anchor.left}px`,
  minWidth: `${anchor.width}px`,
};
```

## Notes
- The interface only enforces numeric values; units are not encoded here (numbers are typically treated as pixels when applied to style values).
- Consumers should recalculate these numbers on layout changes (resize/scroll) to avoid stale positioning.
- No validation is performed by the type; negative or out-of-viewport values are possible and will affect rendering accordingly.

---

## ProjectPickerProps

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

Props for a ProjectPicker component that lets a parent component control which project is active and react to changes. Use this interface when embedding the picker in a route or view that needs to track the active project by id (and optionally its full metadata), and to request a refresh of the picker's project list from the parent.

## Remarks
This surface separates the lightweight identity (activeProjectId) from the heavier full metadata (onActiveProjectMetaChange). That lets callers update and persist just an id in simple flows while optionally receiving the resolved ProjectResponse when more context is required (for routing decisions, diagnostics links, or UI summaries). The refreshKey is an explicit parent-driven trigger for refetching the project list; the picker also performs internal refreshes for its own mutations.

## Example
```typescript
// Typical embedding in a parent component
<ProjectPicker
  activeProjectId={activeProjectId}
  onActiveProjectChange={(id) => setActiveProjectId(id)}
  onActiveProjectMetaChange={(meta) => setActiveProjectMeta(meta)}
  refreshKey={projectsRefreshKey}
/>
```

## Notes
- activeProjectId may be null to indicate no selection; onActiveProjectChange will be called with string | null accordingly.
- onActiveProjectMetaChange is optional and may be invoked with null when there is no active project or the picker clears its selection.
- The picker calls onActiveProjectMetaChange whenever it "resolves" the active project (initial load, user selection, or auto-activation after creation) — do not assume it only fires on explicit user interaction.
- refreshKey is a parent-controlled hint to force a fresh fetch (e.g., after creating a project elsewhere). The picker also refreshes itself on internal mutations, so this key is for cross-component coordination.


---

## ProjectPicker

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Renders a reusable project-selection UI and notifies the parent when the selected project or its metadata changes. Reach for this component when your page or layout needs to display/choose an active project and you want a single controlled component that reports selection and metadata updates to its parent.

## Remarks
Encapsulates the selection UI and change-notification contract: the parent supplies the current activeProjectId and receives changes through the provided callbacks. The refreshKey prop is intended to allow the parent to force the picker to refresh its internal project list or re-run any effect that depends on that key.

## Example
```typescript
// Common controlled usage
const [activeProjectId, setActiveProjectId] = useState<string | undefined>();
const [projectMeta, setProjectMeta] = useState<any>();

<ProjectPicker
  activeProjectId={activeProjectId}
  onActiveProjectChange={(id) => setActiveProjectId(id)}
  onActiveProjectMetaChange={(meta) => setProjectMeta(meta)}
  refreshKey={someReloadCounter}
/>
```

## Notes
- Keep activeProjectId controlled (pass a stable value from state) to avoid mixing controlled/uncontrolled behavior.
- Provide stable callback references (useCallback) if the parent re-renders frequently to avoid unnecessary re-renders of the picker.

---

## close

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Clears the menu state by calling setMenu(null). Use this small convenience callback whenever you need to close or hide the menu from the surrounding component (for example as an onClick or onClose handler).

## Remarks
This function is a tiny wrapper around the surrounding scope's setMenu function so callers can pass a named handler instead of repeating setMenu(null). It exists to improve readability and to provide a single place to adjust close behaviour if needed.

## Example
```typescript
// Typical usage in a React component's JSX
<button onClick={close}>Close</button>

// Or pass it to a child that expects an onClose callback
<Menu onClose={close} />
```

## Notes
- The function assumes `setMenu` is available in the containing scope (usually a state setter). It simply invokes `setMenu(null)` and returns undefined.
- Calling this repeatedly is safe; it unconditionally sets the menu state to null.
- If the close action needs to prevent default event behavior or perform cleanup, handle those before calling `close` or extend this function accordingly.

---

## handleNew

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const handleNew = async () =>
```


Prompts the user for a new project name and, if provided, creates the project via ProjectsService. On successful creation it triggers a local refresh, persists and activates the newly created project, and invokes the provided callbacks (onActiveProjectChange and the optional onActiveProjectMetaChange). Errors during creation are reported through notifyError.

## Remarks
This handler centralizes the common flow of creating a project and making it the active selection so callers (typically UI buttons or menu items in ProjectPicker) don't need to duplicate network, persistence, or callback logic. It performs several side-effects: a network POST, a local refresh counter increment, persistence of the active project id, and callback invocations — all to keep UI state and persisted state consistent after creation.

## Example
```typescript
// Typical usage as a click handler in a React component
<button onClick={handleNew}>New Project</button>
```

## Notes
- The function uses window.prompt for input; it may be unsuitable in non-browser or headless/SSR environments where prompt is unavailable or blocked.
- Input is trimmed and empty/whitespace names are ignored (no request is made).
- Side-effects include persisting the active project id and calling callbacks; tests that exercise this function should account for those external interactions.

---

## loadActiveProjectId

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Reads the active project identifier from window.localStorage using the ACTIVE_PROJECT_KEY and returns it as a string, or null if the key is missing or access to localStorage fails. Use this when you need a safe, non-throwing way to retrieve the persisted active project id (for example when initializing UI state), especially in environments where localStorage may be unavailable or restricted.

## Remarks
This small wrapper centralizes the try/catch around localStorage access so callers don't need to handle storage-related exceptions repeatedly. It intentionally converts any access error into a null result so the caller can treat missing or inaccessible stored values uniformly.

## Example
```typescript
// Initialize a React state with the persisted active project id, falling back to an empty string
const initialProjectId = loadActiveProjectId() ?? '';
const [activeProjectId, setActiveProjectId] = useState<string>(initialProjectId);

// Or in an effect: restore if available
useEffect(() => {
  const id = loadActiveProjectId();
  if (id) setActiveProjectId(id);
}, []);
```

## Notes
- localStorage.getItem already returns null when the key is absent; this function also returns null when any error occurs, so callers cannot distinguish "not set" from "storage unavailable".
- The function returns the raw string value; if you stored JSON, you must parse it yourself and handle parse errors.
- Errors are swallowed (no logging). If you need diagnostics for storage failures (e.g., in development), add explicit logging around calls.
- Access is synchronous and not coordinated with other tabs/windows; consider listening for the storage event if you need cross-tab updates.

---

## onDown

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Closes the picker/menu when a mouse down occurs outside both the menu element and the trigger element. Intended to be used as a global mousedown (or pointerdown) handler so the UI can dismiss the menu when the user clicks anywhere else on the page.

## Remarks
Used by a dropdown/project picker component that exposes refs for the menu (`menuRef`) and the control that toggles it (`triggerRef`), and a `close()` action in the same closure. The handler returns early when the event target is missing or when the click occurred inside either the menu or the trigger, and only calls `close()` for true outside clicks.

## Example
```typescript
useEffect(() => {
  document.addEventListener('mousedown', onDown);
  return () => document.removeEventListener('mousedown', onDown);
}, [onDown]);
```

## Notes
- event.target may be a non-Element (e.g. a Text node); the implementation casts to `Element` which is an assumption — in environments where non-Element targets occur you may prefer to inspect `event.composedPath()` or check `instanceof Node` before using `contains`.
- For Shadow DOM or portal-based menus, `contains` on the stored refs may not detect clicks inside the visual menu; using `event.composedPath()` is more robust in those scenarios.
- Ensure the listener is removed when the component unmounts to avoid leaks or stale closures that call `close()` unexpectedly.

---

## onKey

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

A simple keyboard event handler that calls the surrounding scope's close() function when the user presses the Escape key. Use this as the key event callback for dismissible UI (modals, popovers, pickers) to allow users to cancel/close the component with the Escape key.

## Remarks
This function is intentionally minimal: it performs a single check on event.key and delegates the actual closing behaviour to a close() function captured from the component's outer scope. That keeps the handler reusable and easy to attach/detach from global listeners (for example in a useEffect) while keeping the component-specific closing logic where it belongs.

## Example
```typescript
useEffect(() => {
  // Attach the handler to keydown so Escape is detected as soon as the key is pressed
  window.addEventListener('keydown', onKey);
  return () => window.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- event.key uses the character name (e.g. 'Escape'); ensure this is the intended check for all target browsers or consider using event.code for physical key checks.
- Prefer attaching to 'keydown' to avoid platform differences where 'keyup' may not fire in some situations; remove the listener on cleanup to avoid leaks.
- The handler calls close() from its closure — ensure that close is stable (memoized or defined in scope consistently) or update the effect dependency list to avoid stale references.

---

## openSettings

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Navigates the app to the currently selected project's settings page. Use this helper when you want a consistent way to open the settings view for whatever project is currently active (for example, from a button or menu inside ProjectPicker).

## Remarks
This small utility lives inside the ProjectPicker component and abstracts the navigation logic for opening a project's settings. It first checks that an active project exists and then navigates to /p/{projectId}/settings, using encodeURIComponent to ensure the project id is safe for inclusion in the URL.

## Example
```typescript
// Typical usage inside the ProjectPicker component's JSX
<button onClick={openSettings}>Open project settings</button>
```

## Notes
- The function returns immediately (does nothing) if activeProject is falsy — ensure a project is selected before relying on navigation happening.
- openSettings closes over external values (activeProject and navigate); those must be available in the containing scope (e.g. from component state/props and a router hook).
- The project id is URL-encoded with encodeURIComponent; avoid double-encoding ids that are already encoded.
- This is synchronous and does not return a promise or indicate success/failure of navigation.

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


Persists or removes the currently active project identifier in browser localStorage. Call this when the user changes the active project (or clears it) to remember that choice across page reloads or future visits; prefer this helper over raw localStorage access because it centralizes the key and silently handles environments where storage is unavailable.

## Remarks
This small utility centralizes where the active project id is stored (the ACTIVE_PROJECT_KEY) and deliberately swallows errors from localStorage access. That makes it safe to call in environments where localStorage is absent or throws (private browsing, strict storage settings, server-side execution), but it also means callers receive no confirmation whether the write/remove actually succeeded.

## Example
```typescript
// save an active project id
persistActiveProjectId('project-123');

// clear the saved active project
persistActiveProjectId(null);
```

## Notes
- The function catches and ignores all exceptions: failures (quota exceeded, disabled storage, undefined localStorage in non-browser contexts) are suppressed and the call becomes a no-op.
- The id is stored as-provided (a string). Do not pass objects — stringify them first if needed.
- The function is synchronous and returns no value; it should not be relied on for confirmation that persistence succeeded.

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


Sets the active project to the provided id: it persists the selection, notifies the primary change handler, optionally supplies the project's metadata to a secondary handler, and closes the project picker menu. Call this from UI handlers when the user chooses a project so application state, persisted storage, and UI all stay consistent.

## Remarks
selectProject centralizes the side effects required when changing the active project: persisting the chosen id (persistActiveProjectId), invoking the required application-level change handler (onActiveProjectChange), and invoking an optional metadata callback (onActiveProjectMetaChange) with the corresponding project object or null if not found. It also closes the project picker by calling setMenu(null), so callers do not need to repeat these steps.

## Example
```typescript
// Typical usage from a menu item click handler
function onMenuItemClick(projectId: string) {
  selectProject(projectId);
}
```

## Notes
- onActiveProjectChange is invoked directly and must be provided by the surrounding scope — if it's undefined this function will throw.
- onActiveProjectMetaChange is optional; when present it receives the matched project object or null if no project with the given id exists.
- This function performs synchronous side effects (persistence and callbacks) and returns void; it also closes the picker via setMenu(null).

---

## toggleMenu

> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

Toggles the visibility and position of the project picker menu. When the menu is open this closes it by calling setMenu(null); when closed it computes the trigger element's bounding rectangle and opens the menu by setting its top (4px below the trigger), left, and width based on that rectangle.

## Remarks
Used inside the ProjectPicker component as the single handler to open or close the dropdown/popup anchored to a trigger element (triggerRef). It captures layout from getBoundingClientRect so the menu can be positioned to align with the trigger and maintain a small gap (4px) between the trigger and the menu.

## Example
```typescript
// inside a React component
const triggerRef = useRef<HTMLButtonElement | null>(null);
const [menu, setMenu] = useState<{ top: number; left: number; width: number } | null>(null);

// toggleMenu is defined as in the source and closes/opens the menu

return (
  <>
    <button ref={triggerRef} onClick={toggleMenu}>Toggle</button>
    {menu && (
      <div
        style={{
          position: 'absolute',
          top: menu.top,
          left: menu.left,
          width: menu.width,
        }}
      >
        {/* menu contents */}
      </div>
    )}
  </>
);
```

## Notes
- If triggerRef.current is null the function returns early and does nothing.
- Position values come from getBoundingClientRect (viewport coordinates); ensure the menu is positioned in a coordinate space that matches (e.g., appended to body or using fixed positioning) or convert coordinates if needed for nested/offset containers.
- The function sets a 4px gap between trigger and menu (rect.bottom + 4).

---