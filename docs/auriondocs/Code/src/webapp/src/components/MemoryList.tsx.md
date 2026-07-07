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


MemoryEditorProps defines the shape of the props consumed by the Memory Editor component in the Memory List UI. It provides the project context via projectId, supports preloading an existing memory through initial, and exposes onSaved and onCancel callbacks for persistence and dismissal.

## Remarks
By decoupling the editor's wiring from its implementation, this interface lets MemoryList.tsx render the editor in create or edit mode without embedding data-loading logic. The optional initial enables reuse for both creating new memories and editing existing ones, while onSaved and onCancel funnel lifecycle events back to the parent so it can persist data and close the editor in a single place. The projectId field ensures edits stay associated with the correct project context or can be intentionally detached by passing null.

## Example
```typescript
// Example usage
<MemoryEditor
  projectId={null}
  initial={undefined}
  onSaved={() => { /* handle after save */ }}
  onCancel={() => { /* handle cancel */ }}
/>
```

## Notes
- Callbacks are required; the editor communicates save and cancel intent back to the parent so persistence and navigation can be controlled from a single place.
- Passing null for projectId is valid; ensure the parent handles the absence of a project context appropriately.


---

## MemoryListProps
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** interface

```typescript
interface MemoryListProps
```


MemoryListProps is the props contract for the MemoryList component, specifying the memory scope the list displays and the scope used for newly added entries. Use this interface whenever a MemoryList must operate within either user-scoped memories or a specific project's memories, so the scope is centralized and consistent across usage sites rather than embedded ad-hoc.

## Remarks
MemoryScope decouples the list's behavior from the details of how memories are stored or retrieved. This abstraction lets MemoryList remain agnostic about the memory source while still enforcing a consistent scope policy. By anchoring the component to MemoryScope, you can reason about filtering, permissions, and persistence at the integration points where the scope is produced.

## Notes
- When kind is 'project', a projectId must be supplied; otherwise downstream logic may misbehave.

---

## MemoryEditor
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryEditor(
```


MemoryEditor is a React function component that renders an editing interface for a memory item tied to a specific project. By accepting a projectId, an optional initial memory payload, and two callbacks — onSaved and onCancel — it encapsulates the common editing workflow so callers can present a consistent editor without embedding persistence logic.

## Remarks
Its purpose is to isolate the editing concerns from the surrounding list UI, enabling reuse of the editor across different contexts (e.g., inline editing in a memory list or a dedicated editor screen). It centralizes the editing experience for memory data, ensuring consistent submission and cancellation semantics via the onSaved and onCancel callbacks. The component's contract—projectId, optional initial data, and explicit callbacks—provides a predictable integration point for parent components.

## Notes
- Ensure to pass stable onSaved/onCancel callbacks; changing them frequently can trigger unnecessary re-renders.
- If initial is undefined, MemoryEditor should operate in "create" mode and initialize a new memory payload.

---

## MemoryList
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
export function MemoryList(
```


MemoryList is a React functional component that renders a list of memory entries for a supplied scope. Use it whenever you need a reusable, scoped memory display in the UI instead of rendering the list inline in a parent component.

## Remarks
MemoryList encapsulates the rendering of memory data associated with a given scope, providing a single, reusable UI surface for memory inventories. Centralizing this logic helps ensure consistent styling, interaction patterns (like selection or item expansion), and easier future enhancements (for example, adding filtering or pagination) across memory-related views. It acts as a stable boundary between the data shape exposed by scope and the presentation layer, making it easier to swap the data source or presentation details without touching callers.

## Notes
- Do not mutate the scope prop or memory data; MemoryList should render purely from inputs.
- If the underlying data set is large, consider virtualization or pagination to avoid long render times.
- Ensure stable list item keys to prevent unnecessary DOM churn during scope changes.

---

## MemoryRow
> **File:** `src/webapp/src/components/MemoryList.tsx`  
> **Kind:** function

```typescript
function MemoryRow(
```


MemoryRow is a React function component that renders a single memory item within MemoryList.tsx. It accepts a memory object and two callbacks, onEdit and onDelete, and wires those callbacks to user interactions in the row. This lightweight item renderer is used by MemoryList to display each memory in the collection while delegating data management to its parent.

## Remarks
MemoryRow encapsulates the presentation of a single memory entry and exposes a minimal interaction surface via onEdit and onDelete. The parent MemoryList is responsible for the data store and the actual side effects, while MemoryRow simply translates user actions into those callbacks. This separation improves reusability and testability, and makes it easy to apply consistent styling to every row.

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


stopToggle is a tiny higher-order helper that accepts a void-returning callback and returns a React mouse event handler. When the returned function runs (for example as an onClick handler), it calls e.preventDefault(), stops the event from bubbling, and then executes the provided callback. Use stopToggle whenever you want a click to trigger a side effect without allowing default actions or parent handlers to run.

## Remarks
StopToggle centralizes the boilerplate required to suppress default actions and stop propagation. It keeps onClick logic focused on the side effect, and is especially handy inside composite controls where inner elements should not trigger the container’s click logic. Note that the callback does not receive the event argument because the signature is () => void; if you need event data you must capture it outside and close over what you need.

## Notes
- The inner callback cannot access the click event since stopToggle's fn signature is () => void.
- Because it calls preventDefault and stopPropagation, ensure this behavior aligns with your UX; it will prevent navigation or parent handlers from firing.
- If you need to access event data or pass through to your callback, use another wrapper that captures the event or consider not using stopToggle in that case.

---