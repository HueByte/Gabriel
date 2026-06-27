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

A minimal props bag for a Mermaid-rendering component containing the raw Mermaid diagram definition as a string. Use this interface when passing the diagram text to a <Mermaid /> component so it can parse and render the diagram.

## Remarks
This interface exists to keep the component's public API explicit and focused: a single property provides the full Mermaid DSL source needed for rendering. It keeps the component reusable and makes it clear that the consumer is responsible for supplying the diagram definition.

## Example
```typescript
const diagram = `
  graph TD
    A-->B
    B-->C
`;

// In a React/TSX file
<Mermaid source={diagram} />
```

## Notes
- The source must be valid Mermaid DSL; invalid or malformed text may not render or may produce errors depending on the renderer.
- Multi-line diagrams are expected and should typically be provided using template literals to preserve line breaks.

---

## ModalProps

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

Props for a modal that presents a rendered SVG and the textual source that produced it, along with a close callback. Use this interface to type the props passed into the Mermaid modal component (or any component that needs to display SVG markup and expose its original source) so consumers know an SVG string, the original source string, and a required close handler are expected.

## Remarks
This interface groups three related concerns: the rendered output (svg), the canonical input that produced that output (source), and the lifecycle control (onClose). Keeping these together makes it explicit which data a diagram/modal component needs to render and how a parent should dismiss it.

## Example
```typescript
// Typical usage when rendering the modal from a parent component
const svgMarkup = '<svg>...</svg>';
const mermaidSource = 'graph LR\nA-->B';

function Parent() {
  const [open, setOpen] = useState(true);

  return (
    {open && (
      <MermaidModal
        svg={svgMarkup}
        source={mermaidSource}
        onClose={() => setOpen(false)}
      />
    )}
  );
}
```

## Notes
- svg is expected to be raw SVG markup; when rendering it into the DOM the component should handle sanitization or use React's dangerouslySetInnerHTML intentionally and carefully to avoid XSS.
- All fields are required: provide both strings and a functioning onClose handler to ensure correct behavior when the modal is dismissed.

---

## MermaidModule

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** type

A type alias that represents the runtime module shape of the installed `mermaid` package (the same type you get from `typeof import('mermaid')`). Reach for this when you need to type a variable, parameter, or field that will hold the result of a dynamic `import('mermaid')` or any runtime-loaded reference to the mermaid module — it avoids repeating `typeof import('mermaid')` and makes intent explicit.

## Remarks
This alias captures whatever the `mermaid` package exports at runtime (functions, objects, and any default export carried on the module namespace). It is purely a compile-time convenience so callers can declare and reuse a single, descriptive type for lazily-loaded or injected mermaid instances across the codebase.

## Example
```typescript
// lazy-load mermaid in an async function
let mermaidModule: MermaidModule | null = null;

async function ensureMermaid(): Promise<MermaidModule> {
  if (!mermaidModule) {
    mermaidModule = await import('mermaid');
  }
  return mermaidModule;
}

// usage
async function renderDiagram(definition: string) {
  const m = await ensureMermaid();
  // call into the module's exported API
  // e.g. m.initialize(...); m.render(...);
}
```

## Notes
- The alias has no runtime effect; it exists only for TypeScript's type system.
- For useful typings, TypeScript must be able to resolve `mermaid`'s declarations (either the package ships its own types or you have `@types/mermaid` installed); otherwise the type may be `any` or produce resolution errors.
- Depending on how the package is authored and your TS config (esModuleInterop / allowSyntheticDefaultImports), the runtime value you call may be on the module namespace or on `module.default`; verify how you import/consume `mermaid` at runtime.

---

## MaximizeIcon

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Renders a small, decorative "maximize/expand" SVG icon as a React functional component. Intended for use wherever a compact expand/maximize glyph is needed in the UI; the icon inherits color from its surrounding text (uses currentColor) and is marked aria-hidden so it is treated as purely decorative.

## Remarks
This is a stateless, prop-less component that returns an inline SVG with a 16x16 viewBox and explicit width/height of 14. Because it uses stroke="currentColor", the icon adopts the color of its parent element allowing easy theming via CSS. It is marked aria-hidden="true", so it should only be used when the icon is decorative or the meaning is already conveyed by other accessible text.

## Example
```typescript
// Typical usage inside JSX
import React from 'react';
import { MaximizeIcon } from './components/Mermaid';

function ExpandButton() {
  return (
    <button style={{ color: '#0b5fff' }} aria-label="Expand">
      <MaximizeIcon />
      <span className="sr-only">Expand</span>
    </button>
  );
}
```

## Notes
- The component does not accept props; to change size, color, or other SVG attributes, either wrap it and apply CSS (the color can be changed via the parent element) or modify the component directly.
- aria-hidden="true" means screen readers will ignore the icon — provide an accessible label on the surrounding control when the icon conveys interactive meaning.
- The SVG uses strokeWidth="1.5" and rounded line caps/joins; at very small sizes the stroke thickness may affect visual clarity.

---

## Mermaid

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
export function Mermaid(
```


Renders a Mermaid diagram from the provided `source` prop. Use this component in JSX when you have Mermaid-formatted text that should be displayed as a diagram rather than as plain text.

## Remarks
This component serves as a small UI abstraction that accepts Mermaid syntax and produces the diagram markup needed for display, keeping parsing and rendering concerns out of the caller code. It is intended to simplify usage so consumers only provide diagram source and do not need to manage Mermaid initialization, lifecycle, or DOM updates themselves.

## Example
```typescript
// Inline usage in a React/TSX file
<Mermaid source={`graph LR\nA --> B\nB --> C`} />
```

## Notes
- The `source` value must be valid Mermaid syntax; invalid input will not render a meaningful diagram.
- If your app uses server-side rendering, ensure any Mermaid runtime the implementation requires runs on the client; rendering may depend on client-side script execution.
- Rendering very large or frequently-updated diagrams can be expensive — avoid re-parsing on every render when possible (memoize source or control updates).

---

## MermaidModal

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
function MermaidModal(
```


Renders a modal dialog that presents a Mermaid diagram's rendered SVG and the original Mermaid source, and exposes a callback to close the dialog. Use this component when you want to let users inspect or export a generated diagram in a focused overlay rather than inline in the page.

## Remarks
This component isolates diagram previewing into a reusable UI primitive: it accepts pre-rendered SVG markup and the textual Mermaid source so the caller is responsible for generating the diagram. Keeping rendering and modal behavior together simplifies showing a full-size, inspectable view of a diagram while leaving generation and sanitization responsibilities to the producer of the svg/source props.

## Example
```typescript
// Typical usage inside a React component
const [open, setOpen] = useState(false);
const svgMarkup = getMermaidSvg(someDefinition); // caller provides rendered SVG
const source = someDefinition;

return (
  <>
    <button onClick={() => setOpen(true)}>Open diagram</button>
    {open && (
      <MermaidModal
        svg={svgMarkup}
        source={source}
        onClose={() => setOpen(false)}
      />
    )}
  </>
);
```

## Notes
- The component receives raw SVG markup; ensure the SVG is sanitized or produced from a trusted source to avoid XSS risks.
- Provide a stable onClose handler; the modal should call it when the user dismisses the overlay (e.g., close button, backdrop click, or Escape key) so the caller can update UI state.
- Large or complex diagrams may require styling or container sizing to avoid overflow — the caller may need to constrain width/height or allow scrolling for very big SVGs.

---

## loadMermaid

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Lazily imports the Mermaid library and initializes it with the application's default settings, returning a promise for the library's default export. Call this before attempting to render Mermaid diagrams so the library is loaded and configured (dark theme, no automatic start, strict security, and a custom font).

## Remarks
Centralizes Mermaid initialization in one place so the same configuration is applied consistently and the heavy dynamic import only happens once. The function memoizes the import promise (via an outer-scope mermaidPromise) to ensure concurrent callers share the same load/initialization sequence.

## Example
```typescript
// Ensure mermaid is loaded and initialized before rendering
const mermaid = await loadMermaid();
// Use the returned module according to the Mermaid API to render diagrams.
// Example usage depends on the Mermaid version in your project; call-site should
// use the library's rendering functions after awaiting loadMermaid().
```

## Notes
- The function returns the same promise for repeated calls; initialization runs only once.
- If the dynamic import fails the returned promise will reject — callers should handle errors.
- securityLevel is set to "strict", which prevents inline HTML from appearing in generated SVGs (important when rendering user/chat content).

---

## nextId

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

```typescript
const nextId = () => `md-mermaid-$
```


Returns a new identifier string in the format `md-mermaid-<n>`, where `<n>` is produced by pre-incrementing a module-scoped counter. Reach for this helper when you need short, readable unique IDs for Mermaid diagram containers or other DOM elements within the current page runtime.

## Remarks
Centralizes the id format and sequencing so callers don't duplicate the `md-mermaid-` prefix logic. Uniqueness is only guaranteed within the module's runtime (it relies on a mutable `idCounter` in the same module).

## Example
```typescript
let idCounter = 0;
const nextId = () => `md-mermaid-${++idCounter}`;

console.log(nextId()); // "md-mermaid-1"
console.log(nextId()); // "md-mermaid-2"
// Typical usage: assign to an element id when rendering a diagram
const elementId = nextId();
// <div id={elementId}>...</div>
```

## Notes
- The provided source is missing the closing backtick in the template literal; it will not compile until the backtick is added.
- Uniqueness depends on a module-scoped mutable `idCounter` (not shown here); resetting the counter or sharing the module across different request contexts (e.g., some SSR setups) can cause id collisions.
- The function uses pre-increment (`++idCounter`), so if `idCounter` starts at `0` the first returned id will be `md-mermaid-1`.

---

## onKey

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Handler that listens for keyboard events and invokes the surrounding onClose callback when the user presses the Escape key. Use this when you want a component (for example a modal, popover, or drawer) to close in response to the Escape key instead of wiring the logic inline.

## Remarks
This is a minimal keyboard shortcut handler intended to provide a common accessibility pattern: closing transient UI with Escape. The function checks the KeyboardEvent.key string for the exact value 'Escape' and then calls the onClose function captured from the surrounding scope. It does not prevent default behavior or stop event propagation — it merely observes the key and triggers the close action.

## Example
```typescript
// Attach / detach in a React component
useEffect(() => {
  document.addEventListener('keydown', onKey);
  return () => document.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- The code checks event.key === 'Escape' (exact string). Older browsers or some environments may use different values (e.g. 'Esc'); consider normalizing if you need broader compatibility.
- Ensure the listener is removed during cleanup to avoid memory leaks or unexpected behavior (remove the same function reference that was added).
- Because onKey calls onClose from its closure, keep onClose stable (useCallback) if you add/remove the handler in an effect that depends on the function reference.

---