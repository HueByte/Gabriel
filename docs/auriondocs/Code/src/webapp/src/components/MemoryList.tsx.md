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


MemoryEditorProps defines the props contract for the Memory Editor UI used within the Memory List view. It provides a projectId to scope the editor, an optional initial MemoryDto to prepopulate fields when editing, and two callbacks—onSaved and onCancel—for communicating user actions back to the parent component. Use this interface to keep the editor decoupled from persistence and navigation concerns, enabling a single editor component to support both creation and editing workflows.

## Remarks
MemoryEditorProps acts as a lightweight boundary between the UI and the data model. By accepting an optional initial MemoryDto, it supports both create and edit modes without the editor needing to know where data comes from. The onSaved callback lets the parent react to a successful save (for example, by refreshing a list or closing a dialog), while onCancel provides a clean way to exit editing without side effects. The projectId allows scoping or associating the memory with a particular project, and it may be null to indicate the absence of a specific project context.

## Notes
- Do not mutate the initial MemoryDto object passed via initial; derive local editor state from it instead, to avoid mutating props.

---

## MemoryListProps
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

```typescript
interface MemoryListProps
```


MemoryListProps defines the props for a MemoryList component by specifying the scope of memories that should be displayed and persisted when new entries are added. The scope is expressed as MemoryScope and determines whether memories live in a user-wide space or within a specific project (e.g., { kind: 'user' } or { kind: 'project', projectId }).

## Remarks

By centralizing the memory scope in MemoryListProps, the same MemoryList component can be reused in both personal and project contexts without changing its implementation. MemoryScope provides a discriminated union that enforces correct shapes at compile time, helping prevent accidental mixing of scopes. This abstraction also makes it straightforward to introduce additional scopes in the future (e.g., group or organization) without touching the component's internal logic.

## Example

```typescript
// Example usage of MemoryListProps with user scope
const userMemoryListProps: MemoryListProps = {
  scope: { kind: 'user' }
};

// Example usage with project scope
const projectMemoryListProps: MemoryListProps = {
  scope: { kind: 'project', projectId: 'proj-123' }
};
```

## Notes

- Do not mutate the scope object; MemoryListProps.scope is a value that should be treated as immutable from the component's perspective.
- Switching scopes typically requires a new prop object to trigger data reload; rely on the parent to provide a new MemoryList instance or key when changing scope.

---

## MemoryEditor
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryEditor(
```


MemoryEditor is a React functional component that renders a form to edit a memory item for a specific project. Use it when adding or editing a memory inside MemoryList.tsx; it reports changes via onSaved and lets users cancel via onCancel.

## Remarks
By encapsulating the editing UI, MemoryEditor keeps the MemoryList component focused on listing and orchestration rather than per-item editing details. It accepts an initial value to prefill the form, a projectId to scope changes, and callbacks to let the parent control persistence and navigation. This separation improves reusability: the same editor can be invoked for both creating a new memory and updating an existing one, with the parent providing the actual persistence strategy.

## Notes
- Do not mutate the incoming initial prop; always work with a local state copy.
- Ensure onSaved is called after validation and with the correct memory shape; handle errors gracefully.
- If the editor is used within a modal/dialog, ensure onCancel reliably closes it and does not trigger navigation.

---

## MemoryList
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryList(
```


MemoryList is a React functional component that takes a scope prop and renders a list associated with that scope. Use MemoryList when you want a reusable UI unit to present items tied to a particular context, rather than inlining the list rendering in multiple pages.

## Remarks
MemoryList serves as a presentation boundary between data provisioning and UI rendering. By encapsulating the list rendering in a dedicated component, it becomes reusable across parts of the UI that share the notion of a scope and its related memory items, and it can be styled independently from surrounding layout.

## Example
```tsx
<MemoryList scope={currentScope} />
```


---

## MemoryRow
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
function MemoryRow(
```


MemoryRow is a small, stateless React function component that renders a single memory item within the MemoryList UI. It accepts a memory object along with onEdit and onDelete callbacks and delegates user-initiated actions to these callbacks rather than mutating state directly. This keeps the row presentation separate from the list’s data management, allowing the parent to decide how edits and deletions are handled (e.g., opening editors, triggering API calls). Use MemoryRow when you want a reusable, bottom-level row renderer that can be composed across the memory list without pulling in list-wide logic.

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


stopToggle is a tiny higher-order utility that converts a plain void callback into a React mouse-event handler. It guarantees that when the handler is invoked, the default action is prevented and the event does not bubble upward before the callback runs. Use it for UI controls inside MemoryList.tsx when a click should not trigger parent click handlers or navigation.

## Remarks
By encapsulating preventDefault and stopPropagation, this function keeps event-management concerns out of business logic and promotes reuse across similar controls. It is a simple, dependency-light pattern that relies only on React's event type. It expects a callback with no event parameter; the wrapped function receives no arguments, and the event is consumed entirely by the wrapper.

## Notes
- The wrapper always calls preventDefault and stopPropagation, so be mindful that it will suppress default actions and any ancestor click handlers for the event.
- If you need access to event data inside the callback, capture it outside or pass it through a differently designed wrapper; stopToggle does not pass the event to the inner callback.


---