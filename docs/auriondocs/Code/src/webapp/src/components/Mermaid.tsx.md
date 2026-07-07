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


MermaidProps defines the shape of props expected by the Mermaid diagram component. It declares a single required field, source, which carries the Mermaid DSL string used to render the diagram.

## Remarks
MermaidProps crystallizes the contract between the Mermaid diagram component and its callers. It keeps the responsibility for rendering strictly within the component by requiring a source string, simplifying testing and mocking since any object conforming to MermaidProps is a valid prop container. The interface is intentionally minimal, delegating all rendering logic to the component that consumes these props.

## Notes
- The interface does not validate the Mermaid syntax; runtime validation is the responsibility of the rendering component.
- Treat MermaidProps as immutable input and avoid mutating its properties in downstream code.

---

## ModalProps
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

```typescript
interface ModalProps
```


ModalProps defines the props contract for a modal component that presents a Mermaid diagram; it requires an SVG string to render, the original Mermaid source for reference, and an onClose callback to dismiss the modal. Use this interface to strongly type the props of the Mermaid diagram modal so the rendering and interaction contract is explicit.

## Remarks
ModalProps encapsulates the modal's external interface, decoupling rendering concerns from the modal's consumer. By bundling svg, source, and onClose together, it ensures the modal has what it needs to render the diagram and respond to user dismissal, while allowing the modal implementation to remain agnostic about how the data is produced.

## Example
```typescript
// Example usage of ModalProps
const exampleProps: ModalProps = {
  svg: '<svg />',
  source: 'graph TD; A-->B; B-->C;',
  onClose: () => console.log('Modal closed')
};
```

---

## MermaidModule
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** type alias

```typescript
type MermaidModule = typeof import('mermaid')
```


MermaidModule is a type alias for the module object exported by the Mermaid library. It uses TypeScript's typeof import('mermaid') to capture the shape of the module without forcing a runtime import. Use MermaidModule when you need to annotate a value or parameter that will hold the Mermaid module (for example after a dynamic import) or when building a wrapper around Mermaid that should remain decoupled from a concrete import.

## Remarks
By representing the module as a type, this symbol enables type-safe interaction with Mermaid without bundling the library into every consuming module. It supports lazy-loading patterns, where the actual dynamic import of 'mermaid' can be assigned to a variable typed as MermaidModule, preserving strong typing while keeping the initial bundle small. The abstraction also makes it easier to swap implementations or to mock Mermaid in tests since the type boundary is explicit.

## Notes
- This is a type-only alias; no runtime value is produced by this declaration.
- If your runtime usage expects different import styles (default vs named exports), remember that MermaidModule reflects the module's exports as defined by the Mermaid package and your runtime imports should align with that shape.

---

## MaximizeIcon
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MaximizeIcon()
```


MaximizeIcon is a small, stateless React functional component that renders a decorative inline SVG resembling a four-corner maximize glyph. Use it anywhere you need a consistent maximize/expand affordance, rather than duplicating SVG markup in multiple components, so sizing, stroke, and theming stay uniform across the UI.

## Remarks
MaximizeIcon centralizes the glyph so its appearance remains consistent across toolbars, panels, and dialogs. It uses stroke="currentColor" to adapt to surrounding text color, making it easy to theme with CSS. The SVG is marked aria-hidden since it is purely decorative; if you rely on the icon to convey action, wrap it in a label-bearing element (for example, a button with an explicit aria-label) to provide accessibility context.

## Example
```tsx
import React from 'react';
import { MaximizeIcon } from './Mermaid';

function ToolbarButton() {
  return (
    <button aria-label="Maximize">
      <MaximizeIcon />
    </button>
  );
}
```

## Notes
- The icon has fixed dimensions (width="14", height="14") and a 16 units viewBox; scale it by its container when needed rather than altering the SVG directly.
- Because aria-hidden is set, ensure you provide an accessible label at the control level when the icon is used as an actionable button (as shown in the example).

---

## Mermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
export function Mermaid(
```


Renders a Mermaid diagram from a Mermaid DSL string supplied via the source prop. This React component lets you embed diagrams anywhere in the UI by passing the diagram text, avoiding ad-hoc Mermaid initialization and direct DOM manipulation scattered across the codebase.

## Remarks
By encapsulating Mermaid rendering behind a dedicated component, you centralize options and styling for diagrams (such as theming and rendering behavior), making diagrams consistent across the app and simplifying future changes. It acts as a thin bridge between React lifecycle and the Mermaid library, reducing boilerplate at call sites and enabling easier testing and maintenance.

## Example
```tsx
<Mermaid source="graph TD; A-->B; B-->C" />
```

## Notes
- Diagram rendering typically relies on the Mermaid library being available in the client environment; ensure the host page loads Mermaid when this component is used.
- The diagram will render based on the provided Mermaid DSL; invalid syntax may render incorrectly or emit errors.
- For very large diagrams, consider splitting into smaller components or lazy-loading to avoid long render times.

---

## MermaidModal
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MermaidModal(
```


MermaidModal is a small React component that renders a modal dialog containing a Mermaid-generated diagram. It accepts an SVG representation (svg), the original Mermaid source (source), and a callback (onClose) to be invoked when the user dismisses the dialog. Developers reach for this symbol when they want a focused, dismissible diagram preview/export experience without embedding diagram markup directly in page content.

## Remarks
MermaidModal isolates modal behavior from the rest of the UI, allowing diagrams to be presented on demand while keeping the page layout clean. By accepting both the rendered SVG and the Mermaid source, it supports both visual inspection and source reference in a single, reusable component. This abstraction makes it easy to reuse the same modal experience across multiple diagrams in the web app.

## Notes
- The onClose prop must be connected to a handler that updates visibility state; otherwise the modal cannot be closed.
- The svg prop should be a renderable SVG element; passing plain string Mermaid code requires pre-processing to turn it into an SVG before rendering within MermaidModal.
- Consider accessibility: ensure the modal traps focus and provides an accessible label for screen readers.

---

## loadMermaid
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function loadMermaid(): Promise<MermaidModule['default']>
```

**Returns:** `Promise<MermaidModule['default']>`


loadMermaid lazily imports the Mermaid library and returns its default export as a Promise, reusing a cached module on subsequent calls. It also initializes Mermaid with startOnLoad: false, theme: 'dark', securityLevel: 'strict', and a custom fontFamily to prepare diagrams rendered from chat content in a safe, on-demand manner.

## Remarks
This abstraction centralizes the library bootstrap and security posture, ensuring consistent behavior across call sites. It decouples UI code from the details of dynamic import, cache management, and initialization, so components can simply await the Mermaid module before rendering diagrams.

## Notes
- If the dynamic import fails, the returned Promise is rejected and there is no automatic retry; callers should handle errors or trigger a retry at a higher level.
- startOnLoad: false means Mermaid won't render diagrams automatically on load; you should invoke the rendering API after obtaining the module.
- The securityLevel: 'strict' blocks inline HTML within the generated SVG, which may affect diagrams that rely on HTML content.

---

## nextId
> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
const nextId = () => `md-mermaid-$
```


Generates a unique, deterministic ID for Mermaid elements by incrementing a module-scoped counter and prefixing the result with 'md-mermaid-'. Call nextId whenever you need a fresh identifier for rendering Mermaid diagrams in the web app, instead of constructing IDs by hand or relying on random values.

## Remarks

This function centralizes ID generation for Mermaid diagrams, ensuring every call yields a distinct string that can be used as a DOM id or data attribute. It relies on a mutable, module-scoped counter (idCounter); the sequence is predictable within a single runtime. If your environment involves multiple bundles, server-side rendering, or hot reloading, be aware that IDs may not be globally unique across boundaries without additional scoping.

## Example

```typescript
// Generate two fresh IDs
const id1 = nextId();
const id2 = nextId();
// id1 and id2 are distinct strings like "md-mermaid-1" and "md-mermaid-2"
```

## Notes

- nextId depends on a mutable shared state (idCounter). In development setups with hot-reloading or multiple render roots, the lifecycle and scope of this counter can affect uniqueness across bundles.

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


onKey is a small keyboard event handler that closes the UI when Escape is pressed. It is designed for modal-like surfaces (dialogs, overlays, panels) where Escape should dismiss the content without duplicating close logic across components.

## Remarks
It centralizes Escape-to-close semantics, allowing different dialogs or overlays to share consistent behavior by invoking their onClose callback. By keeping the key-check logic separate from rendering code, it improves reuse and testability across the UI that relies on this handler.

## Notes
- If this function is used as a React onKeyDown prop, the event type should be React.KeyboardEvent, or attach a native listener to guarantee correct typing with KeyboardEvent.
- Ensure the listening element can receive focus, or attach the listener at a global level (e.g., document) to capture Escape regardless of focus.
- Make onClose idempotent to avoid issues if Escape fires multiple times after the UI is already closed.

---