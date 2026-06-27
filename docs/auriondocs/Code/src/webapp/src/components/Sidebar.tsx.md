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

Represents the state of a contextual menu or popup: a unique identifier plus pixel coordinates used for layout (top and right). Reach for this interface when you need to store or pass a menu's identity together with its positioned offset on the screen (for example, to render or re-open the menu at the correct location).

---

## Sidebar

> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

Renders the application's left-hand sidebar: a project-aware list of conversations, controls for collapsing the panel, per-conversation actions (context menu, rename/edit), and project selection state. The active conversation id is read from the URL (useParams) and used as the single source of truth; the component fetches the conversation list from ConversationsService scoped to the currently selected project, persists the collapsed state to localStorage, and exposes localRefresh for sidebar-initiated updates that should re-fetch the list.

## Remarks
The component centralizes sidebar-specific UI and state so parent layout code does not need to prop-drill active conversation or editing state. Project selection is stored in localStorage and kept in sync with a ProjectPicker (commented in code); when activeProjectId is null during initial boot the fetch intentionally passes undefined to the service to avoid returning cross-project results. Interaction helpers (editInputRef, menuRef) are used so the sidebar can focus/select the edit field and close floating menus correctly.

## Notes
- Collapsed state is persisted to localStorage; writes are wrapped in try/catch so failures are silently ignored (e.g., private browsing restrictions).
- When editingId is set the component selects the edit input (editInputRef.current?.select()).
- The conversation list is re-fetched whenever localRefresh or activeProjectId changes; bump localRefresh to trigger a sidebar-local refresh after mutations.
- Floating menus are closed on outside clicks, Escape, sidebar scroll and window resize because the menu is position:fixed and would otherwise become detached from its anchor.

---

## loadSidebarCollapsed

> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

Returns whether the app should initialize the sidebar in its collapsed (overlay/closed) state by reading the boolean-ish value stored under SIDEBAR_STORAGE_KEY in localStorage. Use this when initializing UI state so the sidebar starts collapsed by default and follows the user's persisted preference when available.

## Remarks
This helper centralizes the logic and safe access to localStorage for the sidebar state. It defaults to collapsed to match the EchoHub-style burger pattern (overlay closed) and guards against exceptions from localStorage (for example, in some private modes or restricted environments) by falling back to the default collapsed state.

## Example
```typescript
// Typical use in a React component to set initial state
const [collapsed, setCollapsed] = useState<boolean>(() => loadSidebarCollapsed());

// Later, persist a change (assumes SIDEBAR_STORAGE_KEY and a save function exist)
function toggleSidebar() {
  const next = !collapsed;
  setCollapsed(next);
  localStorage.setItem(SIDEBAR_STORAGE_KEY, next ? '1' : '0');
}
```

## Notes
- Only the exact string '1' is interpreted as collapsed; any other stored value (including '0') is treated as not collapsed.
- If the key is missing (null) the function returns true (collapsed) — missing value falls back to the default.
- Access to localStorage is wrapped in a try/catch; any exception (e.g., storage disabled) causes the function to return the collapsed default.
- The check is strict and does not trim or parse the stored string; values must match '1' exactly.

---