# Mermaid.tsx

> **Source:** `src/webapp/src/components/Mermaid.tsx`

## Contents

- [MermaidProps](#mermaidprops)
- [ModalProps](#modalprops)
- [MermaidModule](#mermaidmodule)
- [MaximizeIcon](#maximizeicon)
- [Mermaid](#mermaid)
- [MermaidModal](#mermaidmodal)
- [loadMermaid](#loadmermaid)
- [nextId](#nextid)
- [onKey](#onkey)

---

## MermaidProps
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

```typescript
interface MermaidProps
```


MermaidProps specifies the props for the Mermaid diagram renderer component used in the React UI. It requires a single string field named source that contains the Mermaid diagram DSL to be rendered. Use this interface when wiring Mermaid.tsx into a UI and providing the diagram text from state, props, or data sources.

## Remarks
By isolating the diagram text in a dedicated prop type, the rendering component remains focused on visualization while the rest of the application supplies the content. This design enables easy reuse of the Mermaid renderer for different diagrams and makes prop-validation straightforward at compile time.

---

## ModalProps
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

```typescript
interface ModalProps
```


ModalProps describes the properties consumed by a modal component that displays a Mermaid diagram. It requires an SVG string to render the diagram, a textual source describing the diagram (Mermaid source), and an onClose callback that is invoked to dismiss the modal.

## Remarks
By grouping these three concerns, ModalProps externalizes the modal’s contract: the parent controls what is shown (svg and source) and how to close it (onClose) without needing to know internal rendering details. This promotes reuse: different components can render identical modals by supplying the same prop shape.

## Example
```typescript
const demoProps: ModalProps = {
  svg: '<svg width="100" height="100" xmlns="http://www.w3.org/2000/svg"></svg>',
  source: 'graph TD; A-->B;',
  onClose: () => { /* close the modal */ }
};
```

## Notes
- Sanitize or trust the svg content before injecting it into the DOM if it will be rendered as raw HTML to prevent XSS.
- Prefer a stable onClose reference to avoid unnecessary re-renders in consuming components (e.g., wrap in useCallback in React components).

---

## MermaidModule
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** type alias

```typescript
type MermaidModule = typeof import('mermaid')
```


MermaidModule is a type alias for the module object produced by a dynamic import of the mermaid package. It allows you to type values or parameters that will hold the Mermaid library at runtime without performing a top-level import.

## Remarks
"MermaidModule" isolates type information from the actual runtime import, enabling lazy loading and easier testing/mocking. It helps keep consumer code flexible against changes in the mermaid package, since the type reflects the module's shape rather than a concrete import site.

## Example
```typescript
async function loadMermaidModule(): Promise<MermaidModule> {
  const mod = await import('mermaid');
  return mod as MermaidModule;
}
```

## Notes
- MermaidModule is a type, not a value. At runtime there is no "MermaidModule" object; you must dynamic-import the module to obtain a usable value.

---

## MaximizeIcon
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MaximizeIcon()
```


MaximizeIcon is a small, stateless React component that renders an inline SVG representing a window-maximize action. It serves as a reusable presentational icon for UI controls that trigger expanding content to fill available space, typically used in toolbars or panels where a maximize action is available.

## Remarks
This icon component centralizes the exact SVG used to depict the maximize action, promoting visual consistency across the UI and enabling easy color and size adjustments via CSS since the stroke uses currentColor. It is decorative (aria-hidden) and does not convey information on its own, so consumers should pair it with an accessible label (for example on the surrounding button) to communicate the action to assistive technologies.

## Notes
- The SVG is marked aria-hidden to indicate it is purely decorative. If accessibility requires a spoken label, ensure the actionable container (e.g., a button) provides an explicit accessible name (aria-label or visible text).


---

## Mermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
export function Mermaid(
```


Mermaid renders a diagram from a Mermaid syntax string supplied through the source prop, acting as a lightweight adapter that embeds Mermaid charts into a React UI. You would reach for Mermaid when you want diagrams defined in Mermaid syntax to be rendered inline in your app rather than using static images.

## Remarks
By encapsulating Mermaid rendering, this component decouples diagram logic from business UI, making theming and styling consistent across diagrams. It provides a single surface for diagram input (a Mermaid text block), so changes to how diagrams are rendered can be made here without touching all call sites. This isolation also makes it easier to swap or upgrade the underlying diagram-rendering strategy in one place.

## Example
```tsx
<Mermaid source="graph TD; A-->B; B-->C" />
```

## Notes
- Invalid Mermaid syntax may fail to render or show an error depending on the environment.

---

## MermaidModal
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MermaidModal(
```


MermaidModal is a React functional component that renders a dismissible modal showing a Mermaid diagram. It accepts three props: svg (the rendered diagram content), source (the Mermaid source text), and onClose (callback invoked when the modal should close). Use MermaidModal when you want to present Mermaid diagrams in a focused overlay rather than inline, enabling users to view the diagram and its source in a dedicated view with an explicit close action.

## Remarks
MermaidModal encapsulates the diagram presentation and the modal chrome so you can reuse this pattern across different parts of the app without duplicating layout or styling. By taking the diagram content (svg) and its source as props, it remains decoupled from the page-level layout and state, making it easy to render any Mermaid diagram in a consistent, accessible overlay.

## Example
```tsx
// Typical usage within a React component
<MermaidModal svg={svg} source={mermaidSource} onClose={handleClose} />
```

## Notes
- Ensure onClose is provided and wired to dismiss the modal; without it, the dialog may be hard to close.
- If the diagram or source is large, consider proper overflow handling and responsive sizing to maintain usability.
- For accessibility, ensure the modal provides appropriate focus management and ARIA attributes, and that the close control is keyboard accessible.


---

## loadMermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function loadMermaid(): Promise<MermaidModule['default']>
```

**Returns:** `Promise<MermaidModule['default']>`


loadMermaid lazily loads the Mermaid diagram library and returns a promise that resolves to the library's default export, initializing Mermaid with a consistent, security-conscious configuration. It caches the import so subsequent calls reuse the same instance instead of re-importing Mermaid on every use.

## Remarks

This abstraction centralizes Mermaid integration into the web UI, ensuring a single initialization and consistent styling across components. By caching the loading promise, components can await loadMermaid() and render diagrams without worrying about repeated imports or multiple initializations, while the strict security settings help prevent unintended SVG script execution.

## Notes

- The function returns a Promise; callers must await or chain a .then to access the Mermaid API.
- If the dynamic import fails, the promise rejects; you may retry by invoking loadMermaid() again.
- The Mermaid module is configured only once per page load; subsequent calls reuse the same instance through the cached promise.

---

## nextId
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
const nextId = () => `md-mermaid-$
```


nextId is a small, stateful helper that returns a unique string identifier for Mermaid-related elements by incrementing a shared counter and prefixing the result with 'md-mermaid-'. Use it whenever you need to attach a stable, collision-free ID to a new Mermaid diagram block or node in the DOM, instead of crafting IDs manually or relying on randomness.

## Remarks
By centralizing ID generation in nextId, the code ensures consistent prefixes and avoids collisions when rendering multiple Mermaid blocks within a page. It relies on a mutable idCounter defined in the same module (or shared scope), so the sequence is predictable and fast. This approach is efficient for client-side rendering but means IDs are not guaranteed to be unique across separate runtimes (for example, across separate tabs or workers).

## Notes
- It mutates a shared, module-scoped counter; IDs are sequential within a runtime, not globally across contexts.
- It requires idCounter to be initialized in the surrounding scope; otherwise the first call may yield NaN or throw.

---

## onKey
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
const onKey = (e: KeyboardEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `KeyboardEvent` | — |


This is a compact keyboard event handler that triggers a close action whenever the Escape key is pressed. It calls onClose() in response to the Escape key, so developers typically attach it to a modal, dialog, or other focusable container to provide a standard Escape-to-close behavior without duplicating the check across components.

## Remarks
By isolating the Escape-key logic into onKey, the code reuses a single, testable closure for dismissal behavior, improving consistency across surfaces that support closing via Escape. It sits at the boundary between input handling and UI lifecycle, delegating the actual close action to onClose while handling the key comparison here.

## Notes
- Ensure the listener is attached to a context that can receive keyboard events (document level or a focused element).
- Remember to unregister when appropriate to avoid memory leaks or multiple bindings.
- Some environments may not provide onClose in scope; ensure onClose is defined and accessible where onKey is used.

---