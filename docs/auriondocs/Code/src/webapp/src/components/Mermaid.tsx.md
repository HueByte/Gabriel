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


MermaidProps is the prop contract for the Mermaid component. It defines the shape of props that the component expects and provides a single mandatory field: source, which holds the Mermaid diagram specification as a string. Callers pass this text to the Mermaid component to render diagrams, rather than passing ad-hoc values, giving strong typing and clear intent at the component boundary.

## Remarks
This interface isolates the diagram text from the rendering logic, enabling straightforward testing and future evolution (e.g., adding configuration options) without altering existing call sites. It also makes the dependency on Mermaid syntax explicit, improving readability and maintenance.

## Notes
- source is non-nullable in TypeScript; in strict mode, missing values are caught at compile time.
- Runtime rendering depends on Mermaid parsing; invalid syntax will surface as render errors in the diagram area.
- If you plan to extend the component with additional props, add them here to preserve backward compatibility and minimize churn.

---

## ModalProps
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

```typescript
interface ModalProps
```


ModalProps defines the props required by a modal UI that presents a Mermaid diagram: an SVG markup for rendering, the original diagram source, and a callback to close the modal. This interface is used whenever a component needs to render the diagram in a modal window, providing a stable contract instead of passing separate values through nested components.

## Remarks
ModalProps encapsulates concerns that belong to the presentation layer: content (svg), provenance (source), and lifecycle (onClose). By abstracting these into a single type, components rendering a diagram modal can remain focused on layout and behavior, while consumers rely on a single prop object. The interface also makes it easier to swap in alternative content renderers or modal implementations without changing call sites.

## Notes
- The svg content will be injected into the DOM as SVG; sanitize or validate to prevent XSS when rendering user-provided content.
- Ensure onClose is a stable function and not re-created on every render to avoid unnecessary re-renders or effect re-triggers.

---

## MermaidModule
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** type alias

```typescript
type MermaidModule = typeof import('mermaid')
```


MermaidModule is a type alias for the module object produced by a dynamic import of 'mermaid'. It is used when you want a type-safe reference to the Mermaid API surface after loading the library at runtime, without importing the module upfront.

## Remarks
MermaidModule captures the module namespace exposed by the dynamically imported Mermaid library, preserving the API surface for type-checking without eagerly importing the package. It enables you to write functions and variables that will hold the loaded module in a type-safe way, supporting lazy or conditional loading patterns.

## Example
```typescript
async function loadMermaid(): Promise<MermaidModule> {
  const mermaid = await import('mermaid');
  return mermaid;
}
```

## Notes
- This type is a type-level alias; there is no runtime value named MermaidModule.
- Dynamic import can fail at runtime; always handle errors accordingly.
- If you need the default export specifically, you may need to reference the .default member in your code or adjust the type accordingly.


---

## MaximizeIcon
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MaximizeIcon()
```


MaximizeIcon is a small React function component that renders a compact SVG glyph representing the maximize action. It’s a pure presentational element intended for reuse wherever a consistent maximize icon is needed, allowing you to drop in a single component rather than duplicating SVG markup and to inherit color from its context via stroke=\"currentColor\".

## Remarks
MaximizeIcon serves as a pure presentational abstraction. It centralizes the glyph so styling updates (such as stroke width or color) apply consistently across all usages, and it aligns with standard icon grids by returning a fixed 14×14 SVG canvas with a simple four-corner motif.

## Notes
- The icon is decorative by default (aria-hidden=\"true\"); if you need screen-reader accessible text, wrap it in a control that provides an accessible label.
- Because it uses a fixed viewBox and dimensions, adjust its size via CSS on the container rather than resizing the SVG directly to avoid distortion.

---

## Mermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
export function Mermaid(
```


Mermaid is a React component that takes Mermaid syntax from its source prop and renders a diagram in the UI. Use this component when you want to embed Mermaid diagrams (flowcharts, sequence diagrams, etc.) in a React page without writing custom Mermaid wiring or lifecycle code.

## Remarks
Mermaid serves as a small wrapper around Mermaid diagram rendering. By consolidating the rendering logic into a single component, it makes styling and theming consistent across the app and shields callers from the library's lifecycle details. It fits alongside other presentational components that translate diagram text into visual diagrams.

## Example
```typescript
<Mermaid source={`graph TD; A-->B; B-->C;`} />
```

## Notes
- Mermaid runtime must be available in the bundle for diagrams to render.
- If using server-side rendering, render on the client only (dynamic import / client-side only).
- Changing the source prop re-renders the diagram to reflect updated Mermaid syntax.


---

## MermaidModal
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MermaidModal(
```


MermaidModal renders a modal dialog that presents a Mermaid diagram. It accepts three props: svg, the rendered SVG element for the diagram; source, the Mermaid source text that produced the diagram; and onClose, a callback invoked when the user dismisses the modal. Use MermaidModal when you want to preview or review a Mermaid diagram without navigating away from the current view, keeping the diagram context visible while you inspect or edit other details on the page.

## Remarks
This abstraction keeps UI concerns separate: the diagram rendering logic lives elsewhere, and MermaidModal only handles presentation and dismissal. By wiring the close action through onClose and consuming a ready-made SVG, it simplifies composition in pages that offer diagram previews while ensuring consistent modal behavior. It also centralizes accessibility and focus-handling considerations around diagram previews.

## Example
```tsx
<MermaidModal
  svg={myMermaidSvg}
  source={myMermaidSource}
  onClose={() => setShowMermaidModal(false)}
/>
```

## Notes
- Ensure svg is a valid React node and is not mutated after render. 
- For very large SVG diagrams, ensure the container provides responsive sizing and proper constraints to avoid layout issues.

---

## loadMermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function loadMermaid(): Promise<MermaidModule['default']>
```

**Returns:** `Promise<MermaidModule['default']>`


loadMermaid lazily loads the Mermaid library via a dynamic import, caches the Promise for reuse, and initializes Mermaid with a centralized set of defaults before returning the API. It returns a Promise that resolves to Mermaid's default export, enabling on-demand access to Mermaid in the application.

## Remarks
By centralizing loading and configuration, this symbol decouples consumers from Mermaid's availability and specifics, ensuring consistent theming (dark theme) and safety across all renderings. The promise cache guarantees a single load/init cycle even when multiple components request Mermaid concurrently, improving runtime performance and avoiding duplicated work. The strict securityLevel prevents inline HTML from being executed in SVG output, aligning with safe rendering of diagrams sourced from chat content.

## Notes
- Dynamic import enables code-splitting; in server-side rendering contexts, ensure this runs only in a browser environment. 
- startOnLoad: false means diagrams won’t render automatically on page load; consumers should trigger rendering explicitly after obtaining the API. 
- If the import fails, all callers awaiting loadMermaid will receive a rejected promise, so consider handling errors at call sites.

---

## nextId
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
const nextId = () => `md-mermaid-$
```


Generates a unique, deterministic identifier for each Mermaid diagram rendered by the web app. It increments a module-scoped counter and returns a string that starts with the prefix `md-mermaid-`. Developers call nextId() to obtain a new ID for diagram containers or related elements, ensuring no collisions when rendering multiple Mermaid instances on the same page. Use this helper instead of assembling IDs manually or relying on random values.

## Remarks
By centralizing ID generation, this helper prevents DOM collisions and keeps Mermaid diagram blocks easily addressable for styling, testing, or programmatic access. It relies on a mutable counter shared within the module; IDs are stable within a single runtime instance but may reset on module reload, such as during hot reload or SSR transitions. This small abstraction cleanly separates concerns: the rendering code can request a fresh ID without worrying about tracking state.

## Example
```typescript
// Most common usage: assign unique IDs to diagram containers
const id1 = nextId(); // "md-mermaid-1" (assuming starting from 0)
const id2 = nextId(); // "md-mermaid-2"
```

## Notes
- The first call yields "md-mermaid-1" when idCounter starts at 0. 
- In development workflows with hot module replacement or environments where the module is reloaded, the counter may reset, potentially producing duplicated IDs if previous diagrams persist.

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


onKey is a small keyboard event handler that listens for key presses and closes the associated UI when the Escape key is pressed. It delegates the actual closing action to onClose(), keeping the listener focused on input handling and enabling reuse across dismissible UI such as modals or drawers.

## Remarks
This abstraction centralizes Escape-based dismissal to avoid duplicating close logic across components. It remains intentionally minimal: it only reacts to the Escape key and relies on the provided onClose callback to perform the actual dismissal. Attach it to the relevant container or element that should respond to Escape to ensure the expected dismiss behavior.

## Example
```typescript
// Example usage: wire Escape-to-close on a DOM dialog
const dialog = document.getElementById('my-dialog');
dialog?.addEventListener('keydown', onKey);
```

## Notes
- When bound globally, Escape may close the UI even while typing in inputs; scope the listener to the active dialog or check the event target to restrict behavior.
- Remember to clean up the listener when the dialog is closed to avoid memory leaks.
- If onClose performs asynchronous work, consider ensuring the user flow remains responsive during dismissal.

---