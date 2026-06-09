# MemoryList.tsx

> **Source:** `src/webapp/src/components/MemoryList.tsx`

## Contents

- [MemoryEditorProps](#memoryeditorprops)
- [MemoryListProps](#memorylistprops)
- [MemoryEditor](#memoryeditor)
- [MemoryList](#memorylist)
- [MemoryRow](#memoryrow)
- [stopToggle](#stoptoggle)

---

## MemoryEditorProps

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

Props for a Memory editor component that ties the editor to a project (if any), optionally seeds the form with an existing MemoryDto for editing, and notifies the parent when the user saves or cancels. Use this interface when rendering a memory creation/editing UI so the parent can provide context and react to completion.

## Remarks
This interface distinguishes between creation and edit flows: provide `initial` when editing an existing memory, leave it undefined for a new memory. `projectId` being `null` indicates the editor is not currently associated with a specific project; consumers should handle that case if project context is required. Callbacks are parameterless—parents are expected to refresh state or re-query data when `onSaved` is invoked.

## Example
```typescript
// Example usage inside a parent component's render/return
<MemoryEditor
  projectId={selectedProject ? selectedProject.id : null}
  initial={editingMemory} // undefined for creating a new memory
  onSaved={() => {
    // refresh list or close editor
    loadMemories();
    setEditing(false);
  }}
  onCancel={() => setEditing(false)}
/>
```

## Notes
- projectId may be `null` — components should not assume a project is always available.  
- `initial` is optional; treat its absence as "create new" and initialize form fields accordingly.  
- `onSaved` and `onCancel` are parameterless callbacks: the parent is responsible for any further actions (re-fetching data, closing UI, etc.).

---

## MemoryListProps

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

Specifies the scope used by a MemoryList component — i.e., which set of memories the list displays and where newly-created memories should be saved. Use this prop to switch the component between a per-user view and a project-scoped view.

## Remarks
This interface is intentionally small: the single scope property is a discriminated MemoryScope that tells the component both how to filter the memories it shows and the target (storage) for any new entries the user adds. The MemoryScope value typically has the shape { kind: 'user' } for a user-global list or { kind: 'project', projectId } for a project-specific list.

## Example
```typescript
// Show the current user's memories
<MemoryList scope={{ kind: 'user' }} />

// Show and save memories for a specific project
<MemoryList scope={{ kind: 'project', projectId: 'proj_123' }} />
```

## Notes
- When using { kind: 'project' }, provide a valid projectId; otherwise the component won't be able to target a project-specific store.
- Changing the scope switches both what is displayed and where subsequent new entries are saved — treat the prop as authoritative for display and persistence behavior.

---

## MemoryEditor

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

A React function component that renders an editor UI for a single "memory" entry and is intended for use inside a memory list or other project-scoped editor flows. Use this component when you need a reusable editor that accepts an optional initial value and reports save/cancel actions back to the caller.

## Remarks
This component is a presentational/editor abstraction: it accepts the projectId to scope the edited item and uses callbacks to communicate user actions (saved or cancelled) to its parent. That keeps saving and navigation concerns out of the component and lets callers decide how to persist changes or update surrounding UI.

## Example
```typescript
// Render MemoryEditor to create a new memory
<MemoryEditor
  projectId="project-123"
  onSaved={(saved) => { /* update list, close modal */ }}
  onCancel={() => { /* close modal */ }}
/>

// Render MemoryEditor to edit an existing memory
<MemoryEditor
  projectId="project-123"
  initial={existingMemory}
  onSaved={(updated) => { /* replace item in list */ }}
  onCancel={() => { /* dismiss editor */ }}
/>
```

## Notes
- initial is typically the existing memory object to edit; omit it (or pass undefined) to present a blank/new entry.  
- Callers are responsible for persisting changes in onSaved; the component reports user intent but does not prescribe storage or navigation behavior.  
- Keep callbacks stable (avoid recreating functions on every render) to prevent unnecessary re-renders of the editor wrapper.

---

## MemoryList

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

MemoryList is an exported React/TSX function component declared to accept a destructured prop ({ scope }). The available source is truncated and contains only the start of the function declaration, so the component's rendered output, behaviors, side effects, and the expected type or shape of the scope prop cannot be determined from the snippet.

## Remarks
This symbol is located in a .tsx file and therefore intended to be a UI component. Because the implementation is missing, this documentation intentionally avoids speculating about responsibilities (for example, whether it renders items, fetches data, or composes other components). Consult the full src/webapp/src/components/MemoryList.tsx file for the complete implementation and prop typings.

## Notes
- The implementation body is not present in the provided source; open the full file to inspect rendering logic and dependencies.
- The type and expected structure of the scope prop are not available here — check nearby type declarations or the complete component signature.
- Confirm how MemoryList is exported and imported in the codebase before using it (named export vs. re-export patterns).

---

## MemoryRow

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
function MemoryRow(
```


Renders a single memory entry (row) and exposes callbacks for editing or deleting that entry. Reach for this component when building a list or table of memories where each item needs the same UI and behaviors wired to parent-managed handlers.

## Remarks
MemoryRow is a lightweight, presentational React component that delegates all side effects to its parent via the onEdit and onDelete callbacks. Keeping mutation and navigation logic out of the row makes it reusable in different contexts (lists, grids, detail panes) and keeps the component easy to test.

## Example
```typescript
// Typical usage inside a parent list component
function MemoryList({ memories }) {
  const handleEdit = (memory) => { /* open editor, navigate, etc. */ };
  const handleDelete = (memory) => { /* confirm, call API, update state */ };

  return (
    <ul>
      {memories.map(m => (
        <li key={m.id}>
          <MemoryRow memory={m} onEdit={() => handleEdit(m)} onDelete={() => handleDelete(m)} />
        </li>
      ))}
    </ul>
  );
}
```

## Notes
- Ensure the memory prop is a valid object (non-null) before rendering to avoid runtime errors in the row's markup.
- Perform confirmation, optimistic UI updates, and error handling in the parent callbacks; MemoryRow should not assume persistence or navigation responsibilities.
- Prefer passing stable callback references (or memoized handlers) to avoid unnecessary re-renders when many rows are rendered.

---

## stopToggle

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

Returns an event handler factory that prevents the mouse event's default action and stops propagation before invoking the provided callback. Use this when you need a simple onClick (or other mouse-event) handler that must block the event from bubbling or performing its default behavior and then run some action (for example, toggling state from a button inside a clickable row).

## Remarks
stopToggle is a small curried helper: give it a zero-argument function and it returns a React.MouseEvent handler. The wrapper calls e.preventDefault() and e.stopPropagation() immediately, then calls your callback. This keeps event-handling logic consistent for places where clicks should not bubble to parent handlers.

## Example
```typescript
// inside a React component
<button onClick={stopToggle(() => setOpen(open => !open))}>Toggle</button>

// common pattern: prevent a button inside a list item from triggering the list's click
<li onClick={() => selectItem(id)}>
  <span>{name}</span>
  <button onClick={stopToggle(() => removeItem(id))}>Remove</button>
</li>
```

## Notes
- The supplied callback is called with no arguments; if you need the event inside your callback, pass a wrapper that accepts the event or change the helper.
- This specifically handles mouse events (React.MouseEvent). For other event types adjust the signature accordingly.
- Because the event is prevented and propagation stopped synchronously, parent handlers will not run for that event.

---