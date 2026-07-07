# Sidebar.tsx

> **Source:** `src/webapp/src/components/Sidebar.tsx`

## Contents

- [MenuState](#menustate)
- [Sidebar](#sidebar)
- [loadSidebarCollapsed](#loadsidebarcollapsed)

---

## MenuState
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** interface

```typescript
interface MenuState
```


MenuState captures the positional data for a contextual popover in the sidebar: the menu's identifier and its absolute screen coordinates (top and right). Use this interface when you need to pass around placement data separately from rendering logic, rather than computing or duplicating coordinates in multiple places.

## Remarks
MenuState acts as a lightweight value object that couples a specific menu instance with its placement information. It enables the Sidebar to coordinate showing a menu for the active item while keeping the popover rendering concerns isolated from the trigger logic. By presenting a stable contract for identity and position, it simplifies sharing this data across components.

## Notes
- The numeric top and right values represent CSS pixel coordinates; when applying to a style object, numeric values are interpreted as pixels in React.
- The id field identifies which menu this state refers to; ensure it remains stable for the lifetime of the menu so updates don't accidentally swap state between menus.

---

## Sidebar
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
export function Sidebar()
```


Renders a collapsible left sidebar that displays conversations for the currently selected project and exposes quick navigation to the active conversation via the URL. It coordinates routing, server data, and user interactions so you can browse per-project threads without prop-drilling state across components.

It fetches conversations scoped to the active project, sorts them by last update, and supports inline title editing, keyboard shortcuts, and a contextual action menu. The component also preserves collapsed/expanded state in localStorage and listens for global input events to enhance accessibility and UX when the panel is open.

## Remarks
This component acts as the local orchestrator for the per-project conversation list and incidental UI concerns (collapse state, focus management, and a mutation-driven refresh). It relies on the router-provided conversationId as the truth source for the active item, and on ConversationsService for data retrieval; changes to the active project or a local refresh trigger re-fetches automatically, keeping the UI in sync with project context.

## Notes
- Cancellation safety: fetch-side cancellation avoids setting state on unmounted components.
- Initialization nuance: when activeProjectId is null during boot, the API is called with undefined to fetch a broad or server-default set; once the project picker resolves, results are scoped to the selected project.
- Escape handling: the global Escape key binding is attached only while the panel is open to close it gracefully.
- Outside-click handling: the external-click/menu logic uses a specific anchor and container references to avoid closing when interacting with the action menu (elements marked with conv-action).

---

## loadSidebarCollapsed
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
function loadSidebarCollapsed(): boolean
```

**Returns:** `boolean`


Determines whether the app’s left sidebar should start collapsed by reading a persisted user preference from localStorage. It fetches the value under SIDEBAR_STORAGE_KEY and returns true when the key is missing or when the stored value is '1'; otherwise it returns false. If localStorage access throws (e.g., in restricted environments or during SSR), it falls back to true to preserve a compact UI consistent with the EchoHub-style burger pattern.

## Remarks

Centralizes the persistence of the sidebar's collapsed state, decoupling rendering from storage details. It provides a single, testable source of truth for the initial UI chrome state and keeps the EchoHub-style burger behavior consistent across sessions.

## Example

```typescript
// Most common usage
const isCollapsed = loadSidebarCollapsed();

// Typical React initialization
const [collapsed, setCollapsed] = useState<boolean>(loadSidebarCollapsed());
```

## Notes

- The literal string '1' means collapsed; any other value (e.g. '0', 'true', 'false') means expanded.
- If SIDEBAR_STORAGE_KEY is not defined or localStorage is unavailable, the function will default to true (collapsed). Ensure the storage key is defined in the correct scope to avoid runtime errors.


---