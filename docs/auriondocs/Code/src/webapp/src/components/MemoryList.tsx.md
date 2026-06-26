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

Props for a Memory editor component — used when rendering a form to create or edit a memory within a (possibly) selected project. Reach for this interface when you need to mount the editor UI and wire up save/cancel behaviour, or when typing components that consume the editor's props.

## Remarks
This interface encapsulates the minimal context the editor needs: the project scope (projectId), optional initial data for edit mode (initial), and two callbacks for lifecycle actions (onSaved and onCancel). A null projectId represents the absence of a selected project (the editor or caller should handle that case). The optional initial property distinguishes create (undefined) from edit (provided) flows.

## Example
```typescript
// Create new memory (no initial data)
<MemoryEditor
  projectId={currentProjectId}
  onSaved={() => { refreshList(); closeModal(); }}
  onCancel={() => closeModal()}
/>

// Edit existing memory
<MemoryEditor
  projectId={currentProjectId}
  initial={existingMemory}
  onSaved={() => { refreshList(); closeModal(); }}
  onCancel={() => closeModal()}
/>
```

## Notes
- projectId may be null to signal "no project selected"; callers should not assume a non-null value.
- initial is optional; presence means the editor should prefill fields for editing, absence indicates creation mode.
- onSaved and onCancel are required callbacks and should be used to update caller state (e.g., refresh lists, close dialogs).

---

## MemoryListProps

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

Describes the props accepted by the MemoryList component; primarily used to tell the component which scope of memories to display and — importantly — which scope newly created entries should be saved into. Use this prop when rendering a MemoryList for either the current user or a specific project so the component knows where to read and persist memories.

## Remarks
The scope property is a MemoryScope discriminated union (e.g. { kind: 'user' } or { kind: 'project', projectId }). MemoryList uses this to both filter the list it shows and to determine the target for any newly-added memories. Because the scope determines the destination for new entries, keep it stable when you expect subsequent saves to go to the same place; switching the scope changes where future saves are written.

## Example
```typescript
// Show and save memories for the current user
<MemoryList scope={{ kind: 'user' }} />

// Show and save memories for a specific project
<MemoryList scope={{ kind: 'project', projectId: 'proj-123' }} />
```

## Notes
- When using { kind: 'project' }, ensure the projectId refers to an existing project and the current user has permission to read/write its memories.
- The scope controls where newly-added entries are saved; it does not retroactively move existing memories between scopes.
- MemoryScope is defined separately — follow its shape exactly (project scopes must include projectId).

---

## MemoryEditor

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryEditor(
```


Renders an editor UI for creating or updating a "memory" (a single item) associated with a project. Use this React component when you need a reusable edit/save/cancel interface that receives an optional initial value and reports user actions through callbacks.

## Remarks
This component encapsulates the common edit lifecycle (prefill from an initial value, persist changes, and cancel). It is intended to be used where memories are listed or managed per-project so the surrounding UI can delegate edit behavior and persistence to the component while handling high-level list updates via the provided callbacks.

## Example
```typescript
// Typical usage inside a parent component or list view
<MemoryEditor
  projectId="project-abc"
  initial={existingMemory} // or undefined/null when creating a new memory
  onSaved={(savedMemory) => {
    // update parent state, refresh list, or navigate
  }}
  onCancel={() => {
    // close editor or revert UI state
  }}
/>
```

## Notes
- The implementation body was not provided with the signature; confirm the exact prop types (shapes of `initial` and the callback argument) before wiring complex logic.
- Ensure callbacks are stable (memoized) when rendering inside lists to avoid unnecessary re-renders.
- Passing a falsy `initial` value typically indicates creation mode; verify how the component differentiates create vs. edit in the source.


---

## MemoryList

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

A small React/TSX component exported as MemoryList that accepts a single props object which is destructured to read a `scope` property. The provided source snippet does not include the implementation or type information for `scope`, but the symbol is intended to be used where a component needs to be given a scope value (for example, to render or filter items related to that scope).

## Remarks
This symbol is declared in a .tsx file and exported as a top-level function, so it is intended to be used as a React function component. Keeping the `scope` prop at the component boundary centralizes whatever scoping logic or rendering is required so callers only need to supply the appropriate scope value.

## Example
```typescript
import { MemoryList } from './components/MemoryList';

function Page() {
  const currentScope = 'project:123';
  return <MemoryList scope={currentScope} />;
}
```

## Notes
- The implementation and the type (or shape) of the `scope` prop are not present in the provided snippet; check the full source to confirm expected types and behavior.
- Because this is a .tsx function export, it should be used inside a React render tree and will return JSX (implementation-dependent).


---

## MemoryRow

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
function MemoryRow(
```


Renders a single row for a "memory" entry and forwards user actions to the parent via callbacks. Reach for this component when you want each memory in a list to encapsulate its own display and to delegate edit/delete behavior back to the list container or parent.

## Remarks
MemoryRow is a small presentational wrapper that keeps per-item markup and interaction logic encapsulated so the parent (for example, MemoryList) can focus on collection-level concerns such as ordering, filtering, and data loading. The component surfaces edit and delete intent through the onEdit and onDelete callbacks rather than performing those operations itself.

## Example
```typescript
// Parent component rendering a list of memories
function handleEdit(memory: Memory) {
  // open editor or navigate
}

function handleDelete(memoryId: string) {
  // confirm and remove
}

return (
  <table>
    <tbody>
      {memories.map(m => (
        <MemoryRow
          key={m.id}
          memory={m}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      ))}
    </tbody>
  </table>
);
```

## Notes
- Verify the exact shapes/signatures of onEdit and onDelete in the implementation — they may receive the full memory object, its id, or another payload.
- MemoryRow is expected to be a controlled/presentational component; manage state and side effects (persistence, navigation, confirmations) in the parent handlers.


---

## stopToggle

> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

Returns a React mouse-event handler that prevents the event's default action, stops propagation, and then invokes the provided no-argument function. Use this when an inner clickable control (for example a button or link inside a list item) should perform its own action without triggering parent click handlers or the browser's default behavior.

## Remarks
This small helper centralizes the common pattern of calling e.preventDefault() and e.stopPropagation() before running a callback, avoiding repetition in JSX event attributes. It is intended for mouse event handlers (React.MouseEvent) and keeps the wrapped callback free of event parameters — useful when the inner action only needs to run code and not inspect the event.

## Example
```typescript
// Prevent the outer list item click from firing when the inner button is clicked
<li onClick={() => setExpanded(!expanded)}>
  <button onClick={stopToggle(() => handleDelete(itemId))}>Delete</button>
</li>

// Close a menu without letting the parent toggle handle the click
<button onClick={stopToggle(() => setOpen(false))}>Close</button>
```

## Notes
- It calls preventDefault(), so using this on anchors or form controls will suppress their native behavior (links won't navigate, forms won't submit).
- The wrapped function receives no event; if you need the event inside fn, wrap it yourself: onClick={(e) => { e.preventDefault(); e.stopPropagation(); myHandler(e); }}.
- This helper only affects mouse events; keyboard events (e.g., onKeyDown) are not handled by stopToggle.

---