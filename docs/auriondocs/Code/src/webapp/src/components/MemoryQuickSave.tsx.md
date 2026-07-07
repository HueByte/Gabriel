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

```typescript
interface MemoryQuickSaveProps
```


MemoryQuickSaveProps is a TypeScript interface that defines the shape of the props passed to the MemoryQuickSave UI. It carries three pieces of data: seedBody, projectId, and onClose. seedBody provides the prefilled text inserted into the memory's body field; users can edit it before saving. projectId controls the scope of the save; a non-null value enables a project-scoped save, while null hides the project option and switches the UI to user-scope. onClose is a callback invoked to dismiss the quick-save dialog.

## Remarks
MemoryQuickSaveProps acts as a small boundary between the MemoryQuickSave component and its surroundings. By isolating seed content, scope, and close behavior, it makes the component easier to test and reuse in different contexts (with project-scoped or user-scoped saving). The seedBody is intentionally editable, signaling that the initial value is a suggestion rather than the final content.

## Example
```typescript
const props: MemoryQuickSaveProps = {
  seedBody: "Idea: summarize the user's goal here...",
  projectId: "proj-123",
  onClose: () => {
    // close handler implementation
  }
};
```

## Notes
- seedBody is a starting point; the actual body is edited by the user before saving.
- If projectId is null, the project-scoped option is hidden and the UI operates in user-scope.
- onClose should be stable across renders to avoid unnecessary re-mounts.

---

## MemoryQuickSave
> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

```typescript
export function MemoryQuickSave(
```


MemoryQuickSave is a lightweight React function component that provides a concise, in-place save action for a memory seed associated with a specific project. It accepts seedBody (the content to persist), projectId (target project), and onClose (callback invoked to dismiss its UI after the save completes). Use MemoryQuickSave when you want a focused, inline saving interaction—such as inside a modal or panel—without routing through a broader save flow.

## Remarks

By encapsulating the save trigger in MemoryQuickSave, the UI stays cohesive and reusable across different parts of the app. It separates the responsibilities of collecting a seed and persisting it from higher-level layout or navigation concerns. The component signature suggests it triggers a save and then immediately signals completion by invoking onClose, allowing it to be composed with modals, drawers, or inline panels.

## Notes

- The exact persistence mechanism is abstracted away; expect that the component wires through a shared memory/save service or context to persist seedBody for projectId.
- If the save operation can fail, coordinate error handling with the caller; onClose is typically invoked on success, not on failure, so error feedback should be surfaced by the parent when needed.

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


Handles keyboard input by checking for the Escape key and invoking onClose when detected. This small handler is intended to be attached to a UI component (such as the memory quick-save panel) to allow users to dismiss it with the Escape key without clicking a button. It relies on an outer onClose callback provided by the surrounding scope.

## Remarks
It encapsulates a common UX pattern (dismissal with Escape) to keep the UI logic focused and reusable. It relies on onClose from the surrounding scope and should be attached to a focusable element or container that receives keyboard events; without focus, Escape presses won't trigger the close.

---