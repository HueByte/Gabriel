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

Represents the props accepted by a Mermaid-rendering component. Use this interface when passing a Mermaid DSL diagram as a plain string to a component that will render it.

## Remarks
This is a minimal DTO-like shape that keeps the component API focused: the single required property, `source`, contains the Mermaid diagram definition (the Mermaid DSL). Keeping the props small makes the rendering component easier to test and reuse.

## Example
```typescript
// Passing a simple left-to-right graph definition to the Mermaid component
<Mermaid source={`graph LR\nA[Start] --> B[End]`} />
```

## Notes
- `source` must be a non-empty string containing a Mermaid DSL diagram; invalid or malformed syntax will not render the expected diagram.
- The interface does not include rendering options (theme, configurations); those, if supported, are provided elsewhere or via a different props shape.

---

## ModalProps

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** interface

Represents the properties passed to a modal used for previewing a Mermaid diagram. Use this interface when rendering a dialog that shows the generated SVG and optionally the original Mermaid source, and needs a callback to close the modal.

## Remarks
This is a small, focused DTO that keeps the modal's contract explicit: svg supplies the rendered markup to display, source provides the textual Mermaid definition (useful for copy/edit actions), and onClose is a consumer-provided callback invoked when the UI should dismiss the modal. The interface assumes the SVG is already generated elsewhere (e.g., by a Mermaid renderer) and that the consumer is responsible for any sanitization or safe insertion into the DOM.

## Example
```typescript
function DiagramModal({ svg, source, onClose }: ModalProps) {
  return (
    <div className="modal">
      <button onClick={onClose}>Close</button>
      <div className="diagram" dangerouslySetInnerHTML={{ __html: svg }} />
      <pre className="source">{source}</pre>
    </div>
  );
}

// Usage
const props: ModalProps = {
  svg: '<svg>...</svg>',
  source: 'graph TD; A-->B;',
  onClose: () => setShowModal(false),
};
```

## Notes
- svg is inserted as markup in the example; ensure the SVG content is trusted or sanitized to avoid XSS risks.
- onClose should be a stable callback if passed through memoized components to avoid unnecessary re-renders.
- Both svg and source are required by this interface — supply empty strings if you need to render an empty state.

---

## MermaidModule

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** type

Represents the compile-time TypeScript type of the 'mermaid' module. Use this alias when typing variables, parameters, or promises that hold the value returned by a dynamic import('mermaid') so callers can refer to a single, descriptive type instead of repeating typeof import('mermaid').

## Remarks
This alias captures the exported shape of the installed 'mermaid' module (as provided by its type declarations) and is primarily useful when lazily loading or otherwise handling the module at runtime. Centralizing the module's type reduces repetition and keeps call sites readable when you need to annotate imported module values or promises.

## Example
```typescript
// annotate a function that lazy-loads the mermaid module
async function loadMermaid(): Promise<MermaidModule> {
  return import('mermaid');
}

// consume it elsewhere
async function useMermaid() {
  const mermaid = await loadMermaid(); // mermaid has type MermaidModule
  // call members on `mermaid` as needed
}
```

## Notes
- This is a type alias only; it has no runtime representation. At runtime you still need to perform dynamic import and any necessary guards (e.g., for SSR or bundlers).
- For meaningful typings, ensure the project has type declarations for 'mermaid' (ambient .d.ts or @types/mermaid). Without those, the imported module type may be inferred as any.

---

## MaximizeIcon

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Renders a small, decorative "maximize" SVG icon built from four stroked paths. Use this component when you need a compact maximize/expand glyph that inherits color from its surrounding text (it uses currentColor) and is intended to be purely visual.

## Remarks
This is a presentational, prop-less React component that returns an inline SVG. It sets aria-hidden="true" so screen readers ignore it (it is not announced), and the stroke uses currentColor so the icon color follows the CSS color of its parent or any applied styles. Because it does not accept props, size and other attributes are controlled via CSS or by wrapping elements.

## Example
```typescript
// Basic usage inside a button; the icon color comes from the button's color
function ToolbarButton() {
  return (
    <button style={{ color: '#0b5fff', padding: 8 }}>
      <MaximizeIcon />
    </button>
  );
}

// Override size via CSS
const smallStyle = { width: 12, height: 12 };
function SmallIcon() {
  return <div style={smallStyle}><MaximizeIcon /></div>;
}
```

## Notes
- The SVG is aria-hidden and therefore not accessible to assistive tech; provide an accessible label on the interactive container (e.g., the button) if the icon conveys meaning.
- Color is controlled via the surrounding element's color (currentColor). To change the icon color, set CSS color on a parent or the SVG wrapper.
- The component does not accept props (size, className, aria-label). To customize attributes you must wrap it or modify the component directly.

---

## Mermaid

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

A named React component that accepts a props object containing a single property, `source`. The implementation was not available in the provided source (only the parameter list `({ source }` was visible), so this documentation describes the externally visible contract rather than internal behaviour. Use this component when you want to pass a Mermaid diagram definition (as a string) into the UI layer via the `source` prop for rendering.

## Remarks
Because the implementation is not present in the checked-out source, details such as how the `source` string is parsed, whether it is sanitized, how rendering is performed (client-side via the mermaid library, server-side rendering, or via an iframe/SVG), and what lifecycle effects occur (initialization, updates, cleanup) are unknown. Consumers should treat this as a small wrapper that centralizes Mermaid-related rendering logic and inspect the actual implementation before assuming specifics about performance, security, or DOM structure.

## Example
```typescript
// Typical usage (based on the visible prop contract)
const diagram = `graph TD\nA-->B`;

<Mermaid source={diagram} />
```

## Notes
- The source code for this function was truncated in the repository view; verify the real implementation for details such as sanitization, error handling, and whether the component is synchronous or asynchronous.
- Confirm whether the component requires the mermaid library to be present globally or as an import, and whether additional initialization is necessary (e.g., calling mermaid.initialize).


---

## MermaidModal

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Renders a modal that presents a Mermaid diagram together with its source and provides a close callback. Reach for this component when you need a reusable UI to preview or inspect a Mermaid diagram (SVG markup) and the original Mermaid source in an overlay-style dialog.

## Remarks
This component isolates the display concerns for a Mermaid diagram: it groups the rendered SVG output and the textual source into a single modal UI and exposes an onClose handler so the parent component controls visibility and teardown. Keeping rendering and closing logic here makes it easy to reuse the same presentation across different places in the app while letting callers manage state and accessibility behavior.

## Example
```typescript
// Render a modal for a Mermaid diagram. Parent controls visibility and handles closing.
const [open, setOpen] = useState(true);
const svgMarkup = '<svg ...>...</svg>'; // produced by Mermaid or server-side renderer
const mermaidSource = 'graph TD; A-->B;';

return (
  {open && (
    <MermaidModal
      svg={svgMarkup}
      source={mermaidSource}
      onClose={() => setOpen(false)}
    />
  )}
);
```

## Notes
- The svg prop typically contains raw SVG markup; sanitize or validate it before passing to the component to avoid XSS if the implementation injects markup into the DOM.
- Ensure focus management and keyboard handling (e.g., restoring focus, handling Escape) are implemented somewhere — either inside this component or by the caller — to meet accessibility requirements.
- Very large SVGs or long source text should be displayed in a scrollable container; callers may need to constrain the modal size or the component may provide scrolling internally.

---

## loadMermaid

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Loads and initializes the mermaid library on demand and returns a promise that resolves to the library's default export. Use this when you need to render mermaid diagrams in the browser and want to avoid importing/initializing mermaid until it's actually required (saving startup cost and keeping initialization centralized).

## Remarks
This function memoizes the dynamic import and initialization so subsequent calls reuse the same Promise and do not re-run mermaid.initialize. It also centralizes the runtime configuration (disables automatic start, applies a dark theme, enforces a strict security policy to block inline HTML in SVG output, and sets a specific font stack) so all consumers render diagrams consistently and safely.

## Example
```typescript
// Typical usage in a React component or other browser-only code
async function renderDiagram(code: string) {
  const mermaid = await loadMermaid();
  // use mermaid to render the diagram (API calls depend on mermaid version)
  // e.g. mermaid.render(...) or mermaid.mermaidAPI.render(...)
}
```

## Notes
- This performs a dynamic import; call it only in browser/runtime code (guard with `typeof window !== 'undefined'` if your app also runs server-side).
- The function returns the same Promise on repeated calls. Initialization runs only once per module lifetime unless the module-level memo is reset.
- `securityLevel: 'strict'` intentionally blocks inline HTML inside diagrams. If you rely on inline HTML in diagrams, those elements will be removed or blocked.
- Ensure `mermaid` is available in your build/runtime dependencies so the dynamic import succeeds.

---

## nextId

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Returns a short, unique identifier string of the form `md-mermaid-<n>` by pre-incrementing a module-level counter. Reach for this when you need a simple, consistent DOM id for Mermaid diagram containers or other per-instance elements to avoid id collisions within the same page.

## Remarks
This small helper centralizes id generation so callers don't need to manage numeric counters themselves. It relies on a module-scoped numeric `idCounter` variable that it increments each time it's called, producing monotonically increasing ids within the running process. Because it mutates `idCounter` (pre-increment), the first returned id will reflect the counter after incrementing.

## Example
```typescript
// assign an id to a container for a mermaid diagram
const containerId = nextId(); // e.g. "md-mermaid-1"
return <div id={containerId} className="mermaid">graph TD; A-->B;</div>;
```

## Notes
- `idCounter` must exist in the surrounding module scope and be a number; if it is undefined or non-numeric the result will be unexpected (NaN-based string).
- The function mutates module state (pre-increment), so calls are not idempotent and will return different values each time.
- For server-side rendering or environments where deterministic ids are required across requests/clients, a module-level counter may cause hydration mismatches or non-deterministic ids; prefer externally provided ids in those cases.

---

## onKey

> **File:** `src/webapp/src/components/Mermaid.tsx`  
> **Kind:** function

Calls the surrounding onClose callback when the user presses the Escape key. Intended to be used as a KeyboardEvent handler (for example on window or an element) to let users dismiss a modal, overlay, or similar UI with the keyboard.

## Remarks
This small handler centralizes the Escape-to-close behavior so it can be attached or detached in a single place. It assumes an onClose function is available in the outer scope and does not manage event listener registration or lifecycle itself — the caller is responsible for adding and removing the handler at the appropriate time (for example, during mount/unmount in a React component).

## Example
```typescript
// Attach to the window when a component mounts and remove on unmount
useEffect(() => {
  window.addEventListener('keydown', onKey);
  return () => window.removeEventListener('keydown', onKey);
}, [onKey]);
```

## Notes
- The code checks KeyboardEvent.key for the string 'Escape' (the DOM spec value). Some old browsers used 'Esc' — consider compatibility if you must support legacy environments.
- This handler does not call preventDefault() or stopPropagation(); if you need that behavior add it explicitly inside the handler.
- When attaching to global targets (window/document) ensure onKey has a stable identity (e.g. wrapped in useCallback) so removeEventListener can successfully deregister it.

---