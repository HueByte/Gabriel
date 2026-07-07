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


MenuState defines the placement contract for a contextual menu anchored in the Sidebar. It carries an identifier and two numeric coordinates (top and right) that describe where the menu should appear; use MenuState when rendering or repositioning menus rather than scattering raw coordinates or mutating DOM state directly.

## Remarks
MenuState is a lightweight data contract that decouples positioning from rendering. It enables the Sidebar to manage multiple menus by id and pass precise placement information to a Menu component or popover without coupling to DOM APIs. This separation supports easier testing and re-use of positioning logic across different menus.

## Example
```typescript
const menuState: MenuState = { id: 'user-menu', top: 120, right: 16 };
```

---

## Sidebar
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
export function Sidebar()
```


Renders and controls the application's left-hand sidebar, coordinating data fetching, user interactions, and routing for conversations within the currently selected project. It derives the active conversation from the URL to avoid prop drilling, fetches project-scoped conversations via ConversationsService, and manages UI concerns such as collapsing the sidebar, editing conversation titles, and showing a per-row action menu. It refreshes its list in response to internal mutation signals and preserves user preferences in localStorage, while gracefully handling errors and integrating with the authentication layer for user context.

## Remarks
The Sidebar serves as the UI boundary that ties routing, data loading, and per-conversation actions together. It encapsulates project-scoped logic so the surrounding layout remains agnostic to whether the user is viewing a single project’s diagnostics or the global conversation list. The localRefresh tick is the sanctioned hook mutations can trigger to refetch the list without going through a global refresh signal.

## Notes
- Mutations in the list (create/edit/delete) should bump localRefresh to trigger a re-fetch; otherwise the list might not reflect changes immediately.
- The outside-click and Escape handling depend on the menu’s DOM shape (menuRef and elements with the conv-action class); ensure action triggers carry the expected class to cooperate with the close-on-outside-click logic.

---

## loadSidebarCollapsed
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
function loadSidebarCollapsed(): boolean
```

**Returns:** `boolean`


Determines the initial collapsed state of the sidebar by reading a flag from localStorage using SIDEBAR_STORAGE_KEY. It returns true when nothing is stored or when the stored value is '1' (collapsed); any other non-null value yields false (expanded). If localStorage access fails, it safely falls back to collapsed.

## Remarks
Centralizes the logic for deriving the sidebar state, so components don't have to read localStorage directly. The default-to-collapsed behavior ensures a predictable and safe initial UI, matching the EchoHub burger pattern. Because it relies on SIDEBAR_STORAGE_KEY, the value's meaning is tightly coupled to how the app persists the state across reloads. This abstraction also makes future changes to the persistence strategy easier to adopt in one place.

## Notes
- LocalStorage access can throw (e.g., in private browsing or certain security contexts); the function guards against this by returning the collapsed default. 
- The value '1' maps to collapsed; any other non-null value is interpreted as expanded, so ensure write paths use '1' to indicate collapsed.

---