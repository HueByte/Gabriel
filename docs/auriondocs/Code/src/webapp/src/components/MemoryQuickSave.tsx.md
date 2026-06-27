# MemoryQuickSave.tsx

> **Source:** `src/webapp/src/components/MemoryQuickSave.tsx`

## Contents

- [MemoryQuickSaveProps](#memoryquicksaveprops)
- [MemoryQuickSave](#memoryquicksave)
- [onKey](#onkey)

---

## MemoryQuickSaveProps

> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** interface

Props for the MemoryQuickSave component that supply an initial memory body, the optional project context, and a callback to close the UI. Use this interface when rendering the quick-save UI so the parent can provide a pre-filled message, indicate whether the save should offer a project scope, and react when the user dismisses the flow.

## Remarks
This is a small, presentation-focused props contract that keeps the quick-save component decoupled from application state. The parent is responsible for providing the starting content (seedBody) and for handling closure behavior; the component itself handles editing and saving. projectId is nullable so the same component can be used both inside a project (showing project-scoped save options) and outside it (hiding those options).

## Example
```typescript
// Parent component: show a quick-save dialog with a prefilled message
function Parent() {
  const [open, setOpen] = React.useState(true);
  const currentProjectId: string | null = getActiveProjectId();

  return (
    open && (
      <MemoryQuickSave
        seedBody={"Notes from today's meeting: ..."}
        projectId={currentProjectId}
        onClose={() => setOpen(false)}
      />
    )
  );
}
```

## Notes
- seedBody is only a starting value — the user can edit it before saving.
- If projectId is null the component should hide any project-scoped save options and only offer user-scope saving.
- onClose is required; it should close/dismiss the quick-save UI when invoked (either after save or when the user cancels).

---

## MemoryQuickSave

> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

A small React UI component that exposes a lightweight "quick save" flow for persisting an initial memory (seedBody) into a given project (projectId) and notifying the caller when the flow closes (onClose). Use this when you need an in-place component to capture or save a memory tied to a project without implementing the full save UI yourself.

## Remarks
The symbol is defined in a .tsx file and follows React component naming conventions, so callers should treat it as a functional component. Its purpose is to isolate the quick-save UX: the parent provides the initial content (seedBody) and the target project identifier (projectId), and receives an onClose callback when the component finishes or is dismissed. Keeping this interaction small and self-contained makes it easy to reuse wherever a compact memory-save affordance is needed.

## Example
```typescript
// Render the quick-save component inside JSX
<MemoryQuickSave
  seedBody={{ title: 'Note', content: 'Check experiment results' }}
  projectId="proj-42"
  onClose={() => { /* refresh list or close modal */ }}
/>
```

## Notes
- The implementation source was not available; concrete types for seedBody and the exact signature of onClose are unknown. Treat seedBody as the initial payload and onClose as a notification callback (it may be called with or without arguments).
- The component likely performs asynchronous persistence. Provide stable callbacks and handle loading/error states in the parent if needed.
- Verify accessibility and focus management in the real implementation if embedding inside dialogs or modals.

---

## onKey

> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

Calls the surrounding onClose callback when the user presses the Escape key. Use this function as a keyboard event handler (for example registered with window.addEventListener or attached to a DOM element) to provide an Escape-to-close shortcut.

## Remarks
This small helper centralizes the Escape-key-to-close behaviour so the component can attach a single handler rather than duplicating the check in multiple places. It expects an accessible onClose closure in scope and performs a simple string check against event.key; it does not stop propagation or prevent the default action.

## Example
```typescript
// attach on mount and remove on unmount in a React component
useEffect(() => {
  window.addEventListener('keydown', onKey);
  return () => window.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- The handler checks event.key strictly for 'Escape'; older browsers or unusual keyboards may report different values (e.g. 'Esc'), so account for compatibility if necessary.
- This expects a DOM KeyboardEvent (as used with addEventListener). If you pass it directly as a React synthetic event handler, ensure the types and event object align.
- The function does not call preventDefault() or stopPropagation(); if those behaviours are required, handle them where the function is attached or extend the handler.
- Ensure the onClose closure is stable (memoized or referenced) if you attach/detach this handler in an effect to avoid stale references or unnecessary re-registrations.

---