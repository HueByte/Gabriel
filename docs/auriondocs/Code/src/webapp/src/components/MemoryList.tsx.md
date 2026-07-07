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

```typescript
interface MemoryEditorProps
```


MemoryEditorProps defines the props accepted by the Memory Editor UI in MemoryList.tsx. Use it when wiring the editor to either create a new memory (omit initial) or edit an existing one (supply initial memory) and to connect save/cancel behavior to the parent component, while scoping changes to a specific project via projectId.

## Remarks
By isolating the editor's inputs into MemoryEditorProps, the editor becomes reusable across create and edit flows without embedding data-fetching concerns. The optional initial prop drives mode (create vs edit) and preloads the form when editing. The projectId ensures all operations are associated with a particular project context, preventing cross-project data mixing. The onSaved callback is the hook the parent uses to refresh lists or navigate after a successful save.

## Example
```typescript
// Create mode
const createProps: MemoryEditorProps = {
  projectId: 'proj-42',
  onSaved: () => refreshList(),
  onCancel: () => setEditing(false)
};

// Edit mode
const editProps: MemoryEditorProps = {
  projectId: 'proj-42',
  initial: existingMemoryDto,
  onSaved: () => refreshList(),
  onCancel: () => setEditing(false)
};
```

## Notes
- Pass a string for projectId when the memory belongs to a project; pass null if there is no project association.
- The initial prop is optional; omit it to render a blank form for creation. Ensure MemoryDto shape matches the editor expectations.


---

## MemoryListProps
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

```typescript
interface MemoryListProps
```


MemoryListProps defines the props for a MemoryList component and exposes a single field, scope, that determines which memories are shown and where new memories are saved. The scope encodes either a user-wide view (kind: 'user') or a per-project view (kind: 'project', projectId).

## Remarks
MemoryListProps uses the MemoryScope abstraction to decouple the data-source boundary from UI concerns. This enables the MemoryList component to be reused in both personal and project contexts without duplicating logic, since the actual data retrieval and persistence can share the same memory access layer keyed by the scope.

## Notes
- When using project scope, ensure a valid projectId is provided in the MemoryScope.
- Switching scope at runtime may trigger a rebind or reload of the list's data; callers should manage scope stability to avoid unnecessary data fetches.

---

## MemoryEditor
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryEditor(
```


MemoryEditor is a React function component that presents an editing UI for a memory item tied to a specific project. It takes a projectId, an optional initial value, and onSaved/onCancel callbacks, and should be used when you need to create or modify a memory entry from the MemoryList view rather than editing items inline.

## Remarks
Remains focused on input handling, validation, and user actions separate from the list rendering. By encapsulating the edit flow, MemoryEditor centralizes memory-entry semantics (e.g., field validation and save/cancel behavior) and makes MemoryList simpler and more testable. It also makes reuse easier if memory editing is needed in other contexts beyond MemoryList.

---

## MemoryList
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryList(
```


MemoryList is a React function component that accepts a props object and destructures a scope property. It serves as a focused, reusable UI piece to render a list whose content is determined by the given scope, rather than hard-coding a single dataset. Use MemoryList to render a scoped memory-related collection within the web UI, keeping page components lean and avoiding duplication of list-rendering logic.

## Remarks
By isolating scope-driven rendering, MemoryList provides a clear boundary between layout and item presentation, enabling consistent styling and behavior across different scopes. It helps promote reuse and testability by centralizing the rendering decisions for memory-scoped lists.

## Notes
- Ensure scope prop is properly typed to avoid runtime errors when MemoryList is used without a value.
- If MemoryList interacts with external data (fetches based on scope), consider caching or memoization to avoid unnecessary fetches on prop changes.
- Be mindful of re-renders: keep MemoryList as a light wrapper that delegates actual item rendering to child components to minimize updates.

---

## MemoryRow
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
function MemoryRow(
```


MemoryRow is a small React function component used inside MemoryList to render a single memory entry as a row. It receives the memory object to display and two callbacks, onEdit and onDelete, which are invoked when the user triggers editing or deletion of that memory. This abstraction keeps the list rendering focused on layout while isolating per-item interactions in a dedicated component, enabling reuse and simpler testing.

## Remarks
Isolating per-row logic helps keep MemoryList lean and makes it easy to swap different row presentations without altering the list wiring. It also provides a stable surface for editing or removing a memory, which can be wired to modals or in-line editors without affecting the overall list structure.

## Example
```typescript
// Example usage within a MemoryList
{memories.map(memory => (
  <MemoryRow
    key={memory.id}
    memory={memory}
    onEdit={() => editMemory(memory)}
    onDelete={() => deleteMemory(memory)}
  />
))}
```

## Notes
- Ensure memory objects have a stable, unique identifier used as the list key (commonly memory.id).
- Callbacks should be side-effect free within MemoryRow; prefer passing the memory through to the parent rather than mutating it inside the row.


---

## stopToggle
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
const stopToggle = (fn: () => void) => (e: React.MouseEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `fn` | `() => void` | — |


stopToggle is a higher-order function that turns a simple callback into a React mouse-event handler which first prevents the default action and stops propagation, then invokes the callback. Use it for controls inside clickable rows or cards to perform an action without triggering the row’s click behavior (for example, toggling a item without navigating).

## Remarks
This utility centralizes the common pattern of suppressing event flow when a user interacts with a nested control inside a larger clickable region. By wrapping the action in stopToggle, the MemoryList UI can attach robust, predictable interactions without duplicating boilerplate across multiple controls, improving maintainability and readability.

## Example
```typescript
const handleOpen = () => openItem(itemId);
<button onClick={stopToggle(handleOpen)}>Open</button>
```

## Notes
- The wrapped function fn does not receive the event; if you need event data, capture it in a closure and still call fn.
- The wrapper creates a new handler on each call; consider memoizing if used inside tight render loops.
- This is specific to mouse events; ensure the element uses a mouse-related event (e.g., onClick) when applying this wrapper.

---