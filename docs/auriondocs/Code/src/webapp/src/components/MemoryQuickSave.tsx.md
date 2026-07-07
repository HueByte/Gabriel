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


MemoryQuickSaveProps defines the props contract for the Memory Quick Save UI. It carries the initial seed for the memory body, an optional project context, and a close callback: seedBody supplies prefilled text for the memory's body (editable by the user before saving), projectId designates the project scope (non-null enables project-scoped saving and reveals the project control; null hides that option, leaving the user-scoped path), and onClose is invoked to dismiss the UI.

## Remarks
This interface centralizes the data required by the Memory Quick Save component, enabling it to present a meaningful default body, conditionally expose project-scoped saving, and signal when the UI should close. By encoding seedBody and projectId as inputs rather than performing work itself, the symbol supports reuse of the MemoryQuickSave UI across contexts (project-scoped or user-scoped) without embedding presentation logic in callers.

## Notes
- seedBody is a starting point that the user can edit; it is not the final saved content.
- When projectId is null, the project scope control should be hidden, reflecting a user-scoped scenario.
- onClose should be wired to the UI's dismissal path to avoid leaving the dialog open.

---

## MemoryQuickSave
> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

```typescript
export function MemoryQuickSave(
```


MemoryQuickSave is a React functional component that wires a quick-save action for a given seedBody within a specific project. It takes three props: seedBody, projectId, and onClose. The component's behavior is inferred from its name and props; it likely triggers a save operation scoped to the provided project and then invokes onClose to signal completion or dismissal.

## Remarks
MemoryQuickSave encapsulates a minimal, reusable save interaction that can be dropped into modal dialogs or inline UIs without pulling in broader save flows. By binding the action to projectId, it ensures the save context remains explicit and predictable, while onClose gives the parent component control over lifecycle after the action completes.

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


onKey is a small keyboard event handler that receives a KeyboardEvent and triggers a close action when the Escape key is pressed. It is intended to be attached to a focusable element or a keydown listener to provide a concise, keyboard-accessible way to dismiss a panel, modal, or overlay, delegating the actual close operation to onClose.

## Remarks
This function centralizes Escape-to-close behavior for the MemoryQuickSave UI, keeping dismissal logic in one place and enabling reuse across different UI regions. It is intentionally narrow: it only reacts to the Escape key and delegates the actual dismissal to onClose, making it easy to test or replace in isolation.

## Notes
- onClose must be defined in the surrounding scope; onKey calls it directly when Escape is detected, so an undefined onClose will cause a runtime error.
- It does not call preventDefault or stopPropagation; if you need to intercept Escape in more complex keyboard handling, attach it at the appropriate level and consider adding explicit event control as needed.

---