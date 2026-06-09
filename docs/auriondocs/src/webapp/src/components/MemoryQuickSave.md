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

A small props bag for the MemoryQuickSave component describing the initial state and lifecycle hook used when presenting a quick-save UI for a memory. Use this interface when you need to pre-fill a memory's text, optionally associate the save with a project, and react when the quick-save UI is closed.

## Remarks
This interface exists to keep the quick-save UI independent from higher-level form or persistence logic: seedBody provides an editable starting value for the memory's body, projectId signals whether a project-scoped save option should be offered, and onClose lets callers know the UI was dismissed so they can refresh state or proceed with navigation.

## Example
```typescript
// Typical JSX usage when rendering the quick-save dialog
<MemoryQuickSave
  seedBody={conversationDraft}
  projectId={currentProject ? currentProject.id : null}
  onClose={() => setShowQuickSave(false)}
/>
```

## Notes
- seedBody is only a starting point: the user may edit the body before actually saving.
- When projectId is null the component should hide any project-scope save option and offer only user-scoped saving.
- onClose is a UI lifecycle callback; do not assume it implies a successful save — use a separate success callback or state if you need confirmation of persistence.

---

## MemoryQuickSave

> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

Provides a compact UI for saving an in-memory "seed" into a project and notifies the host when the UI should be closed. Reach for this component when you need an embedded quick-save control for a seedBody associated with a specific projectId instead of launching a full editor or flow.

## Remarks
This component centralizes the quick-save UX so callers can open it (for example in a modal or drawer) and react to completion via onClose without duplicating save UI logic. It is designed as a presentational/interaction piece — persistence details (API calls, storage) are likely encapsulated inside the component or delegated to hooks/services it consumes.

## Example
```typescript
// Rendered inside JSX (typical usage)
<MemoryQuickSave
  seedBody={currentSeed}
  projectId={project.id}
  onClose={() => setShowQuickSave(false)}
/>
```

## Notes
- The provided source was truncated; confirm whether onClose receives arguments (e.g., saved item id or status) or is called without parameters.
- Treat seedBody as immutable or serializable when passing it in; the component may serialize/clone it before persisting.
- Use inside a React application context (modal/panel); it likely does not manage its own visibility and expects the parent to mount/unmount or hide it.

---

## onKey

> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

```typescript
const onKey = (e: KeyboardEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `KeyboardEvent` | — |


Handler that listens for keyboard events and calls the surrounding scope's onClose() when the Escape key is pressed. Use this function when you want a component (for example a modal or quick-save UI) to close in response to the user pressing Escape; attach it as a key event listener (e.g. onKeyDown or a window listener).

## Remarks
This is a minimal, local keyboard handler intended to be used inside the MemoryQuickSave component (or similar). It deliberately only reacts to the 'Escape' key and does not prevent default behavior or stop propagation — responsibility for those concerns is left to the caller. The function references onClose from its outer scope, so its behavior depends on that value.

## Example
```typescript
// Inside a React component
useEffect(() => {
  window.addEventListener('keydown', onKey);
  return () => window.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- Remember to remove the listener when the component unmounts to avoid leaks.
- If onClose is re-created on each render, prefer stabilizing it with useCallback or include it (or onKey) properly in the effect dependencies to avoid repeatedly adding/removing the listener.
- Some very old browsers used 'Esc' instead of 'Escape'; this handler checks for the standard 'Escape' string.

---