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


MenuAnchor is a compact data contract that describes where a contextual menu should anchor itself within the UI. It exposes three numeric fields—top, left, and width—that together represent the anchor's vertical offset, horizontal offset, and visual width. This simple shape lets the rendering logic align a popover or dropdown with its triggering element without embedding measurements or layout logic in the menu component.

## Remarks
Separating geometry from rendering reduces coupling between the ProjectPicker's measurement logic and the menu's presentation. It provides a stable contract that can be produced by different layout calculations (e.g., from DOM measurements or responsive logic) and consumed by the menu to render in the correct place. This pattern makes testing easier and supports reusability of the menu component across layouts.

## Notes
- Ensure updates reflect current layout; stale values cause misalignment.
- This is a plain data container; there is no behavior.
- Coordinate space must be consistent between producer and consumer (e.g., same container viewport basis).


---

## ProjectPickerProps
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** interface

```typescript
interface ProjectPickerProps
```


ProjectPickerProps defines the props accepted by the ProjectPicker component. It carries the identity of the currently active project (as a string ID or null when none is active), a callback invoked when the user selects or clears the active project, an optional callback that provides the full active-project metadata whenever the picker resolves the active project, and an optional refreshKey that the parent can bump to trigger a refresh.

## Remarks
Acts as a contract between the UI and its container, enabling the parent to coordinate routing and diagnostics behavior around the active project without the picker needing to know those rules. The onActiveProjectMetaChange callback surfaces rich metadata (ProjectResponse) to support higher-level decisions, such as which URL or view to open. The nullable activeProjectId supports both selection and explicit clearing, which is important for initial loads and user-driven resets.

## Example
```typescript
// Example illustrating a props object that conforms to the interface
const exampleProps: ProjectPickerProps = {
  activeProjectId: "proj-42",
  onActiveProjectChange: (id: string | null) => {
    // handle change
  },
  onActiveProjectMetaChange: (meta: ProjectResponse | null) => {
    // handle metadata changes
  },
  refreshKey: 2
}
```

## Notes
- The activeProjectId field is nullable to represent "no selection." Callers should handle null gracefully.
- Bumping refreshKey triggers the picker to refetch its data; avoid unnecessary or frequent changes to prevent excessive refreshes.

---

## ProjectPicker
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function ProjectPicker(
```


ProjectPicker is a React function component that renders a user interface for selecting or switching the active project within the web app. It takes the current activeProjectId and exposes callbacks for when the user changes the active project or when the active project's metadata changes, plus a refreshKey to trigger a data refresh.

## Remarks
Design-wise, ProjectPicker acts as a boundary between page-level state and the domain concept of a project. By delegating the selection and metadata change events to the provided callbacks, it keeps the UI decoupled from how projects are loaded or what metadata is associated with a project. Additionally, refreshKey gives the parent a simple mechanism to force the picker to re-evaluate its data and UI state, which is useful when the available projects or their metadata may change outside the component's usual lifecycle.

## Notes
- If refreshKey changes too frequently, the component may re-fetch data frequently; consider debouncing or batching updates.
- Ensure onActiveProjectChange remains a stable function (memoized) to prevent unnecessary re-renders of the picker.

---

## close
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const close = () => setMenu(null)
```


Closes the ProjectPicker menu by resetting the component's menu state to null. This tiny helper centralizes the common action of dismissing the menu, so developers can attach it as an event handler (for example, to a Close button) without duplicating setMenu(null) in multiple places.

## Remarks
Close acts as a single point of truth for how the ProjectPicker is dismissed. It decouples the open/close semantic from the exact UI markup, enabling reuse wherever the menu should be hidden and simplifying future changes to the close behavior. It also helps keep unit tests focused on the close behavior rather than the surrounding rendering code.

---

## handleNew
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const handleNew = async () =>
```


Prompts the user for a new project name and, if the input is non-empty after trimming, creates the project via ProjectsService.postApiProjects.

On success, it increments the local refresh counter, persists the new project's id as the active project, and calls onActiveProjectChange(project.id) and onActiveProjectMetaChange?.(project); on failure, it surfaces the error via notifyError with the message 'Failed to create project.'

---

## loadActiveProjectId
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
export function loadActiveProjectId(): string | null
```

**Returns:** `string | null`


Reads the active project identifier from localStorage under the ACTIVE_PROJECT_KEY and returns it as a string. If the key is missing or localStorage access throws, it returns null instead of raising an error.

## Remarks

By centralizing the retrieval and guarding against environments where localStorage is unavailable, this function allows the UI to gracefully handle the absence of an active project without crashing. It returns a string when a value is present and non-null; otherwise, callers should treat null as indicating that no active project is currently selected.

## Notes

- Returns null when the item is absent or when localStorage access fails; callers must handle null.
- Safe for environments without a DOM (e.g., during server-side rendering); the catch block prevents exceptions from propagating.

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


OnDown is a mouse-down event handler that closes the project picker when the user clicks outside of the dropdown UI. It resolves the event target and, if the click did not occur inside the menu (menuRef) or the trigger element (triggerRef), it calls close() to hide the panel. This centralizes outside-click dismissal logic and prevents accidental closures when interacting with the dropdown itself.

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


The onKey function is a focused keyboard event handler that detects when the Escape key is pressed and invokes close(). Use this to implement Escape-to-close behavior for UI elements like a modal or picker, ensuring a consistent user experience without requiring mouse interaction.

## Remarks
This abstraction centralizes Escape-key handling and delegates the actual closing action to close(), which keeps the component's input handling separate from its dismissal logic. It also helps keep the UX consistent across components that should be dismissible via Escape. If you integrate this into different environments (e.g., React synthetic events), ensure the event type aligns with the host framework or adjust the signature accordingly.

## Notes
- This relies on the standard KeyboardEvent.key value "Escape"; in very old browsers, you might also encounter "Esc".
- The handler assumes close() is in scope and callable; ensure it is defined in the consumer's context.
- Attach this handler to the appropriate key event stream (commonly keydown) so Escape is captured reliably and before focus shifts.

---

## openSettings
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const openSettings = () =>
```


Opens the settings view for the currently active project by navigating to the project-specific settings route. It first checks for an active project and returns early if none is present, ensuring no navigation occurs in that state. When a project is active, it routes to /p/{id}/settings using a URL-encoded project id to guarantee a valid path regardless of special characters in the id.

## Remarks
This function centralizes the navigation logic from the Project Picker to the per-project settings screen, so callers don’t need to know the exact URL structure. Encoding the project id with encodeURIComponent prevents malformed URLs when IDs contain special characters and keeps the route pattern consistent. The early guard against a missing active project protects against unexpected navigations in edge UI states.

## Notes
- If activeProject exists but its id is absent or not a string, the resulting URL may be malformed; ensure activeProject.id is a string and truthy before calling openSettings.
- The function relies on the external activeProject and navigate collaborators; changes to those shapes or imports may require updates to this helper.

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


Persist the active project identifier to the browser's localStorage. This small helper encapsulates the storage interaction so the rest of the UI can save or clear the user's last-selected project without duplicating localStorage logic. When called with a non-null string, it writes the id under ACTIVE_PROJECT_KEY; when called with a falsy value (such as null or an empty string), it removes that key. All operations are wrapped in a try/catch that quietly ignores errors to avoid impacting the UI if storage is unavailable.

## Remarks
Centralizes persistence concerns for the UI's project selection, ensuring a single, consistent storage key is used and isolating storage details from callers. The abstraction makes it easy to swap storage strategies later and guarantees that a failed storage attempt won't crash the UI.

## Notes
- An empty string will be treated as falsy and cause removal of the key; callers should pass null or a non-empty string to persist a value.
- Silent failures mean persisted state may be missing when localStorage is unavailable or blocked; callers should not rely on persistence for correctness.

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


selectProject updates the active project context when a user selects a project from the ProjectPicker. It persists the chosen id, notifies listeners about the change via onActiveProjectChange, optionally passes the selected project's metadata to onActiveProjectMetaChange, and then closes the picker by clearing the menu.

## Remarks
By centralizing the sequence of side effects triggered by a project selection, this symbol acts as a coordination point between persistence, event dispatch, and UI state. It relies on the presence of onActiveProjectMetaChange and the projects collection to fetch and relay metadata; if no matching project is found, it passes null to the meta callback to signal the absence.

## Notes
- If the selected id is not found in the projects array, onActiveProjectMetaChange will be invoked with null.
- The function ends by closing the ProjectPicker with setMenu(null); calling from non-UI contexts may produce unexpected UI behavior.

---

## toggleMenu
> **File:** `src/webapp/src/components/ProjectPicker.tsx`  
> **Kind:** function

```typescript
const toggleMenu = () =>
```


toggleMenu toggles the ProjectPicker's dropdown: if the menu is currently visible, it closes it by clearing the menu state; otherwise it computes the trigger button's position via triggerRef.current.getBoundingClientRect and opens the menu anchored beneath the trigger, using the trigger's width to size the menu. This keeps the popup aligned with its trigger and reuses the same function for both opening and closing.

## Remarks
By centralizing open/close and placement logic, this function keeps the popup behavior cohesive and easy to reuse in the ProjectPicker. It relies on a ref to the trigger DOM element and a menu state to express visibility and geometry, illustrating a straightforward imperatively-driven popup pattern in React. The positioning formula top: rect.bottom + 4, left: rect.left, width: rect.width ties the menu to the trigger's current size and position, preserving alignment when the trigger moves or resizes.

## Notes
- Guard clause ensures no action if the trigger button isn't mounted.
- No handling for viewport clipping or automatic reflows; if the menu would overflow the viewport, it isn't adjusted here.
- The function assumes the surrounding container allows absolute positioning based on viewport coordinates.

---