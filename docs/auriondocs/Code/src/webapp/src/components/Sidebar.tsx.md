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


MenuState defines the minimal shape of a menu's identity and positioning used inside the Sidebar. It captures an id and positional offsets (top and right) so components can render or reposition a contextual menu deterministically without depending on DOM specifics.

## Remarks

MenuState acts as a lightweight value object that decouples layout concerns from business logic. By centralizing id/top/right into a single contract, components that render menus and those that trigger them can share a consistent representation, reducing duplication and mistakes when computing positioning.

## Example

```typescript
const currentMenu: MenuState = { id: 'settings', top: 110, right: 20 };
```

## Notes

- Mutating MenuState instances can cause subtle UI drift; prefer creating new object instances to reflect updates.
- Keep top/right as pixel values in the same coordinate system used by your styling (e.g., px).

---

## Sidebar
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
export function Sidebar()
```


Renders a collapsible left-hand navigation panel that lists conversations scoped to the active project and exposes actions to switch between conversations, rename items, and access per-conversation controls. It derives the active conversation from the URL, fetches the project-scoped conversations via ConversationsService, and manages local UI state (collapse state, editing mode, quick-action menus) to drive navigation and interaction without leaking UI concerns into parents.

## Remarks
Encapsulates the UI concerns around listing and selecting conversations for diagnostics. It coordinates data fetching, derived from the active project, with URL-driven navigation to keep the view in sync with the browser state. It also handles global interactions (Escape-to-close, outside-click to dismiss menus) and UI preferences (collapsed state) to deliver a consistent, accessible experience without leaking layout or data retrieval concerns elsewhere.

## Notes
- The component relies on ConversationsService for its data; changes to the API surface or data shape may require corresponding updates here.
- It persists the collapsed state to localStorage and gracefully handles environments where localStorage is unavailable or disabled (errors are caught and ignored).

---

## loadSidebarCollapsed
> **File:** `src/webapp/src/components/Sidebar.tsx`  
> **Kind:** function

```typescript
function loadSidebarCollapsed(): boolean
```

**Returns:** `boolean`


Returns a boolean that indicates whether the app's sidebar should start in a collapsed state by reading a saved flag from localStorage (SIDEBAR_STORAGE_KEY). If the key is missing, the value is treated as collapsed by default; a value of '1' maps to true, and any other value maps to false. If accessing storage fails, it gracefully falls back to true to align with the EchoHub-style collapsed default. Use this helper to decide the initial rendering state of the sidebar without duplicating storage logic.

## Remarks

This function centralizes the persistence rule for the sidebar's open/closed state, decoupling rendering from storage concerns and ensuring a consistent default of collapsed to match the EchoHub pattern. It guards against runtime storage exceptions via a try/catch, so UI initialization remains stable even when localStorage is unavailable or blocked. Placing this logic here makes it easy to adjust the default or the storage key in one place without touching rendering code.

## Example

```typescript
// Example: apply a collapsed state class based on the persisted preference
<div className={loadSidebarCollapsed() ? 'sidebar collapsed' : 'sidebar'}>
  {/* sidebar contents */}
</div>
```

## Notes

- This function relies on the browser's localStorage; in non-browser environments (e.g., server-side rendering) or when storage is disabled, it will return true by default. If you need a different persistence mechanism, consider injecting a storage provider or gating this call behind client-only rendering.


---