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


MemoryQuickSaveProps defines the props contract for the MemoryQuickSave UI component. It provides seedBody as the initial draft to pre-fill the memory's body, an optional projectId that scopes the save to a project (null hides the project scope in user-level flows), and an onClose callback invoked when the quick-save UI is dismissed.

## Remarks
This interface is a lightweight data transfer object that encapsulates both UI state and behavior. It enables the MemoryQuickSave component to present a starting point to the user while respecting project-scoping rules, without hard-coding workflow decisions into the component itself.

## Notes
- seedBody is a starting point for the memory body and may be edited by the user before saving.
- If projectId is null, the UI should hide or disable the project-scoping option, reflecting a user-scope context rather than a project-scoped workflow.
- onClose is a callback intended to be invoked when the quick-save UI is closed, allowing callers to perform cleanup or dismissal actions.

---

## MemoryQuickSave
> **File:** `src/webapp/src/components/MemoryQuickSave.tsx`  
> **Kind:** function

```typescript
export function MemoryQuickSave(
```


MemoryQuickSave encapsulates a focused quick-save flow for a seedBody associated with a projectId and offers an onClose callback to notify the caller when the dialog or action completes. Use MemoryQuickSave when you need a consistent, reusable quick-save experience across the UI instead of duplicating save/seeding logic in multiple components.

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


When activated, this function acts as a keyboard event handler that watches for the Escape key and triggers the close action. It provides a compact, reusable way to dismiss the related UI (such as a memory quick-save overlay) via keyboard input, rather than requiring a mouse interaction.

## Remarks
Encapsulating the Escape-to-close behavior in onKey centralizes keyboard dismissal logic and keeps the close behavior decoupled from the specific event source. Attach this handler to a keyboard event (for example, onKeyDown) on the Memory Quick Save UI so that pressing Escape consistently closes the UI by invoking onClose.

## Notes
- Uses e.key === 'Escape' to detect the Escape key; this is the most explicit and cross-browser-friendly check, but older code might use 'Esc' or keyCode. Prefer 'Escape' for clarity.
- Relies on onClose existing in scope; if onClose is undefined or not in scope, calling it will fail at runtime.
- Should be wired to an appropriate keyboard event (such as onKeyDown) on the element representing the UI; using the wrong event or propagating incorrectly could lead to missed dismissals or multiple triggers.

---