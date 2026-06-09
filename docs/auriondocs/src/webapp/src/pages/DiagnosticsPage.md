# DiagnosticsPage.tsx

> **Source:** `src/webapp/src/pages/DiagnosticsPage.tsx`

## Contents

- [DiagnosticsFrameProps](#diagnosticsframeprops)
- [DiagnosticsPageProps](#diagnosticspageprops)
- [DiagnosticsFrame](#diagnosticsframe)
- [DiagnosticsPage](#diagnosticspage)
- [DiagnosticsPageInner](#diagnosticspageinner)
- [ProjectDiagnosticsPage](#projectdiagnosticspage)
- [onBack](#onback)

---

## DiagnosticsFrameProps

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** interface

A typed shape for props passed to a diagnostics frame renderer. Use this interface when a component or function needs a flat numeric frame buffer, an associated palette (array of numeric entries), and a numeric index identifying which frame is being represented.

## Remarks
This interface exists purely for type safety and clear intent when rendering or inspecting diagnostic frames. It keeps the frame data (a one-dimensional numeric buffer), the palette (a two-dimensional numeric array describing palette entries), and a simple numeric index bundled together so callers and consumers have a single, self-describing value.

## Example
```typescript
const props: DiagnosticsFrameProps = {
  frame: new Array(160 * 144).fill(0).map((_, i) => i % 4), // flat pixel/entry indices
  palette: [[0,0,0], [85,85,85], [170,170,170], [255,255,255]], // per-entry numeric color arrays
  index: 3,
};

// Passed into a React component:
// <DiagnosticsFrame {...props} />
```

## Notes
- The type system enforces array shapes only at the top level: it does not guarantee inner arrays have a specific length or that numeric values represent colors vs. indices. Validate contents as needed at runtime.
- These arrays may be large; avoid mutating them in-place if the same data is shared elsewhere (prefer copying or using immutable updates to prevent surprising UI updates).
- Because frame is a flat number[] the consumer is responsible for interpreting its layout (width/height or scanline ordering).

---

## DiagnosticsPageProps

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** interface

Props for the DiagnosticsPage component — indicates which sequence to inspect and optional UI hints. Use this interface when rendering the diagnostics view so the page knows which sequence (via SequenceSource) to show; page wrapper components typically populate `source` from the URL and only override `backTo`/`scopeLabel` when the defaults need changing.

## Remarks
This prop bag separates routing/URL concerns from the presentational diagnostics page. Wrappers extract a SequenceSource from the route and pass it here, allowing the DiagnosticsPage to remain focused on rendering/debugging UI for that sequence while callers control navigation (back target) and a short heading suffix (scopeLabel).

## Example
```typescript
// Typical usage when rendering the page directly (wrappers usually do this for you):
import { DiagnosticsPage } from './DiagnosticsPage';
import type { SequenceSource } from '../types';

const source: SequenceSource = getSequenceSourceFromUrl(); // provided by your routing layer

<DiagnosticsPage
  source={source}
  backTo={`/items/${source.id}`} // optional: overrides default back target
  scopeLabel="Project"         // optional: short suffix shown in the heading
/>
```

## Notes
- `source` is required; if you mount DiagnosticsPage outside of the usual wrapper, compute and pass a valid SequenceSource. 
- Omitting `backTo` uses the component's default (the matching detail page); do not rely on the default if you need a custom back-target.
- `scopeLabel` is a short, user-facing suffix (e.g. "Project", "Conversation") used only for headings/labels.

---

## DiagnosticsFrame

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

```typescript
function DiagnosticsFrame(
```


Renders a single diagnostics frame entry for the DiagnosticsPage. Use this presentational React function component when you need to display one frame together with its color palette and its position in a list; it accepts props { frame, palette, index } and returns the UI for that item.

## Remarks
This component is intended to be the unit renderer for a list of diagnostic frames so page-level code can map over a collection and delegate visual representation and layout to this component. That separation keeps data selection and composition at a higher level while this component focuses on how a single frame is displayed.

## Example
```typescript
// Typical usage inside DiagnosticsPage or any list renderer
frames.map((f, i) => (
  <DiagnosticsFrame key={f.id ?? i} frame={f} palette={palette} index={i} />
));
```

## Notes
- The exact shapes of `frame` and `palette` are not present in the provided source; check the surrounding type definitions to learn required properties and optional fields.
- Treat `index` as an ordering/display hint (likely zero-based); do not use it as a stable key when rendering lists — prefer a stable `id` from `frame` if available.
- If the component expects a non-null `palette` or other invariants, callers must ensure those guarantees; verify props before passing if values can be missing.

---

## DiagnosticsPage

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

A small route-aware React component that reads the conversationId route parameter and renders DiagnosticsPageInner scoped to that conversation. Use this component as the page element for a diagnostics route that includes a :conversationId parameter; it converts the URL parameter into the props DiagnosticsPageInner expects and constructs a back link to the conversation view.

## Remarks
This component is a thin wrapper whose sole responsibilities are extracting the conversationId from the URL, returning null when no id is present, and passing a standardized source object, backTo path, and scopeLabel to DiagnosticsPageInner. It keeps routing concerns (URL decoding/encoding and parameter presence checks) out of the inner diagnostics UI.

## Example
```typescript
// Typical React Router usage
import { Routes, Route } from 'react-router-dom';
import { DiagnosticsPage } from './pages/DiagnosticsPage';

<Routes>
  <Route path="/diagnostics/:conversationId" element={<DiagnosticsPage />} />
</Routes>
```

## Notes
- If the route does not provide a conversationId, the component returns null (renders nothing). Ensure your route includes the :conversationId param.
- The backTo path uses encodeURIComponent(conversationId) to safely embed the id into a URL segment.


---

## DiagnosticsPageInner

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Renders the inner content of a diagnostics page using the props provided by its parent. Reach for this component when you need the presentational/core content of the diagnostics screen separated from outer page chrome or routing logic — it accepts a data `source`, a `backTo` value to control back navigation, and a `scopeLabel` describing the diagnostics scope.

## Remarks
This function is the page's inner/presentational piece: it focuses on rendering diagnostics content for a given scope and relies on its parent to supply the data source and navigation behavior. That separation keeps layout and routing concerns outside this component so the inner UI can be reused or tested in isolation.

## Notes
- The snippet does not include prop types or implementation details; check the source for the exact shapes and expected types of `source`, `backTo`, and `scopeLabel` before using the component.
- This component likely does not include page-level chrome (headers/footers) or perform top-level routing — the parent is expected to handle navigation and context.
- If `source` is an observable or callback that changes identity frequently, memoize or stabilize it at the parent level to avoid unnecessary re-renders.

---

## ProjectDiagnosticsPage

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Renders the diagnostics UI for a single project by reading the route parameter named `projectId` and delegating the actual UI to DiagnosticsPageInner. Use this component when you need a route-backed diagnostics view that targets a specific project (it extracts the ID from the router params instead of receiving it via props).

## Remarks
This component is a thin wrapper whose sole responsibilities are extracting the `projectId` from React Router params and invoking DiagnosticsPageInner with a `source` object identifying the project, a `backTo` location of `/`, and a `scopeLabel` of `project`. It keeps routing concerns out of the inner diagnostics implementation so DiagnosticsPageInner can remain focused on rendering diagnostics for whatever source it's given.

## Example
```typescript
// react-router v6 example
import { Route, Routes } from 'react-router-dom';
import { ProjectDiagnosticsPage } from './pages/DiagnosticsPage';

<Routes>
  <Route path="/projects/:projectId/diagnostics" element={<ProjectDiagnosticsPage />} />
</Routes>
```

## Notes
- This component requires a Router context (useParams) — rendering it outside a Router will throw.
- If the `projectId` route param is missing or empty the component returns null (renders nothing); ensure your route includes `:projectId`.
- It does not validate the shape or existence of the project ID; DiagnosticsPageInner should handle invalid IDs or missing resources.
- The `backTo` prop is hard-coded to `/`; change the wrapper if a different return location is desired.


---

## onBack

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Navigates the user back: if the surrounding scope provides a truthy `backTo` value it navigates to that path, otherwise it goes back one entry in browser history by calling `navigate(-1)`. Use this as a click/handler for a "Back" control when a component may either return to a specific route or fall back to the previous history entry.

## Remarks
This small helper centralizes the conditional back-navigation decision so callers don't need to duplicate the same `if (backTo) ... else ...` logic at each back button. It expects `navigate` and `backTo` to be available in the enclosing scope (typically provided by React Router's `useNavigate` and a prop or state for `backTo`).

## Example
```typescript
import { useNavigate } from 'react-router-dom';

function MyPage({ backTo }: { backTo?: string }) {
  const navigate = useNavigate();
  const onBack = () => {
    if (backTo) navigate(backTo);
    else navigate(-1);
  };

  return <button onClick={onBack}>Back</button>;
}
```

## Notes
- If `backTo` is an empty string or another falsy value, the function will call `navigate(-1)` instead of navigating to that value; ensure `backTo` is explicitly set when you intend to navigate to a falsy route value.
- `navigate(-1)` relies on the browser history; if the history stack has no previous entry the result depends on the router/environment and may not change the current page.

---