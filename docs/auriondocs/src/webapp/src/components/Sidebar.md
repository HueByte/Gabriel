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

A small data shape that captures which floating/contextual menu is open and where it should be rendered. Use this interface when you need to store or pass the menu's identifier together with numeric offsets used to position the menu.

## Remarks
Centralizes the minimal pieces of information a menu renderer needs: an identifier to distinguish menu instances and two numeric offsets (top and right) used for placing the floating element. Keeping these three values together simplifies state handling for showing, moving, or hiding a context/floating menu.

## Example
```typescript
// Typical React usage inside a Sidebar or similar component
const [menu, setMenu] = useState<MenuState | null>(null);

function openMenuForItem(id: string, anchorRect: DOMRect) {
  setMenu({
    id,
    top: anchorRect.top,    // numeric offset used for positioning
    right: window.innerWidth - anchorRect.right // compute right offset if needed
  });
}

// When rendering the menu:
// <div style={{ position: 'absolute', top: menu?.top, right: menu?.right }}>...</div>
```

## Notes
- top and right are numeric offsets (typically pixel values). If you convert them to CSS string values, append units (e.g. `${top}px`). React inline styles accept numbers as pixels for most layout properties.
- id should uniquely identify the menu instance being opened; do not assume uniqueness is enforced elsewhere.

---

## Sidebar

> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

A stateful React sidebar component that renders and manages the conversations list, project selection state, per-conversation actions (edit, menu), and collapsed/open state. It reads the active conversation id from the URL (avoiding prop drilling), fetches conversations from ConversationsService scoped to the currently selected project, persists the collapsed state to localStorage, and exposes UI behaviors such as in-place renaming, a contextual menu, and local refreshes after sidebar-initiated mutations.

## Remarks
The component centralizes sidebar-specific concerns so the rest of the layout doesn't need to coordinate conversation list fetching, edit state, or menu lifecycle. Using the URL as the single source of truth for the active conversation keeps routing and UI selection consistent across the app; the localRefresh counter lets sidebar actions (create, delete, rename) trigger a refetch without depending on an external/global refresh signal. Project-scoped fetching (passing the active project id or undefined briefly during bootstrap) prevents unwanted cross-project results in normal operation. Menu lifecycle handlers also account for the menu being position:fixed by closing on outside click, Escape, scroll, and resize so the menu never floats detached from its anchor.

## Example
```typescript
// Typical usage inside an app layout with React Router
import { BrowserRouter } from 'react-router-dom';

function AppLayout() {
  return (
    <BrowserRouter>
      <div className="app-shell">
        <Sidebar />
        <main>{/* routed conversation view here */}</main>
      </div>
    </BrowserRouter>
  );
}
```

## Notes
- The component reads and writes localStorage for the collapsed and project selection state; guard or mock localStorage in SSR or test environments to avoid runtime errors.
- The contextual menu logic relies on document-level event listeners and an element class selector ('.conv-action') to avoid immediately closing when a row's action button is clicked — ensure any custom action triggers include that class if they should not close the menu first.
- Because activeProjectId can be null briefly during startup, the initial fetch may receive whatever the server returns for an unspecified project; this is intentional in the component but can produce a transient mixed result set until the picker resolves.


---

## loadSidebarCollapsed

> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

Returns whether the sidebar should start in the collapsed (closed overlay) state by reading a persisted preference from localStorage. Use this when initializing the sidebar's UI state so the component respects the user's last choice; it defaults to collapsed when no preference is stored or when storage access fails.

## Remarks
This helper centralizes the logic for interpreting the value saved under SIDEBAR_STORAGE_KEY. A stored value of '1' is treated as collapsed; any other stored value (including '0' or arbitrary strings) is treated as not-collapsed. The function catches exceptions from localStorage access and falls back to the collapsed default, which makes it safe to call in environments where storage may be unavailable (e.g., private browsing or server-side execution).

## Example
```typescript
// Typical usage in a React component to initialize state from persisted preference
import { useState } from 'react';

function SidebarWrapper() {
  const [collapsed, setCollapsed] = useState(() => loadSidebarCollapsed());

  // ...render sidebar based on `collapsed`
}
```

## Notes
- If localStorage is inaccessible (throws), the function returns true (collapsed) rather than propagating an error.
- Only the literal string '1' is interpreted as collapsed; any other non-null value is treated as not-collapsed.
- Calling this on the server (where localStorage is undefined) is safe because exceptions are caught and the default is returned.

---