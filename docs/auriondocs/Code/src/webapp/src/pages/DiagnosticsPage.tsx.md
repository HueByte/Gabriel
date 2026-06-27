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

Represents the set of properties needed to describe a single diagnostics frame: a flat numeric frame buffer, an associated palette (array of numeric color entries), and the numeric frame index. Use this interface when passing per-frame pixel and palette data into a renderer, inspector, or any component that operates on an individual diagnostics frame.

## Remarks
This interface groups the three related pieces of data that are commonly required together when rendering or analyzing a diagnostics frame. By bundling the frame buffer, palette, and index into a single props type, components and functions can accept a single parameter and remain clear about the expected inputs.

## Example
```typescript
// Pass frame data and palette into a React component
function FrameViewer(props: DiagnosticsFrameProps) {
  const { frame, palette, index } = props;
  // render or inspect the frame using the palette
  return <div>{/* rendering logic here */}</div>;
}

// Usage
<FrameViewer frame={frameBuffer} palette={paletteTable} index={currentFrameIndex} />
```

## Notes
- The interface only enforces that these values are numeric arrays; it does not enforce a specific encoding (for example whether palette entries are RGB tuples or some other format). Ensure producers and consumers agree on the array shapes and encoding.
- Arrays are passed by reference — mutating the provided arrays after passing them as props may affect other consumers.

---

## DiagnosticsPageProps

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** interface

A small props bag for the Diagnostics page component that identifies which sequence to inspect and optional UI context (where the back button should go and an appended scope label for headings). Use this interface when rendering or testing the Diagnostics page so the component receives the required sequence source and optional navigation/labeling hints.

## Remarks
This interface exists to separate required routing/state input (the SequenceSource) from optional presentation and navigation hints. In the app the page wrapper normally derives and supplies source from the URL; callers only need to provide backTo or scopeLabel when they want to override navigation or the heading suffix.

## Example
```typescript
const props: DiagnosticsPageProps = {
  source: { type: 'conversation', id: 'conv-123' },
  backTo: '/conversations',
  scopeLabel: 'Conversation',
};

// Render (pseudo-code, actual component may vary)
// <DiagnosticsPage {...props} />
```

## Notes
- source is required and typically comes from the URL via page wrappers; don’t assume it will be populated automatically if you instantiate the page component directly.
- backTo is optional — when omitted the page falls back to its default "matching detail page" destination, so pass it only to override that behavior.

---

## DiagnosticsFrame

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Renders a single diagnostics frame for the diagnostics page UI. Use this React function component when you need to display one frame (the frame data), supply a color palette for rendering, and provide its position/ordinal via index; typically invoked by DiagnosticsPage or another parent that iterates over a frames collection.

## Remarks
Encapsulates the rendering logic for an individual diagnostic frame so page-level code can remain focused on collection-level concerns (iteration, selection, layout). Keeping frame rendering in a dedicated component makes it easier to test, style, and replace rendering strategies (for example swapping canvases, SVGs, or pixel buffers) without changing the parent.

## Example
```typescript
// Typical usage inside a page that has an array of frames:
frames.map((frame, i) => (
  <DiagnosticsFrame key={i} frame={frame} palette={palette} index={i} />
))
```

## Notes
- The concrete shapes and types of `frame` and `palette` are not present in the available source; check the component's implementation or surrounding prop/type definitions to know the expected structure and any performance/mutation expectations.
- `index` is the frame's ordinal passed by the caller; callers should assume it is zero-based unless the implementation documents otherwise.
- If the component performs expensive rendering (canvas updates, image decoding), ensure callers avoid unnecessary re-renders by memoizing props or the component itself.

---

## DiagnosticsPage

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Renders the diagnostics UI for a single conversation identified by the route parameter `conversationId`. This component reads `conversationId` from the URL (using `useParams`) and, when present, forwards it as the `source` prop to `DiagnosticsPageInner` along with a `backTo` URL and a `scopeLabel`. If the route parameter is missing, the component renders nothing.

## Remarks
A thin routing-aware wrapper that adapts URL state into the shape expected by `DiagnosticsPageInner`. It centralizes the logic for extracting the conversation identifier and building the return link (`/c/<conversationId>`), keeping the inner diagnostics component focused on presentation and behavior for a given source.

## Example
```typescript
// route registration example
<Route path="/diagnostics/:conversationId" element={<DiagnosticsPage />} />

// produced back link when conversationId is "abc 123":
// backTo === `/c/${encodeURIComponent('abc 123')}` // -> "/c/abc%20123"
```

## Notes
- The component returns `null` if `conversationId` is not present; mounting this component without the expected route param yields no UI.  
- `backTo` uses `encodeURIComponent` to ensure the conversation id is safely embedded in the URL.  
- `useParams` may provide `undefined`, which is normalized here to an empty string via the destructuring default; the early return prevents passing an empty id into the inner component.

---

## DiagnosticsPageInner

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

A presentational React component that renders the internal content of the Diagnostics page. It is intended to be used wherever the Diagnostics UI needs to be embedded; the function signature destructures the props { source, backTo, scopeLabel } which are supplied by the parent or routing wrapper.

## Remarks
The "Inner" suffix implies this component is the page's rendering-focused part, separated from higher-level concerns such as routing, data fetching, or layout. Use this component when you want the visual/interactive portion of the Diagnostics page without the surrounding route/container logic (for example in storybook or unit tests).

## Example
```typescript
// Render DiagnosticsPageInner inside a test or a parent page
<DiagnosticsPageInner
  source={myDiagnosticsSource}
  backTo="/overview"
  scopeLabel="Project: Alpha"
/>
```

## Notes
- The source snippet provided only includes the prop destructuring; inspect the implementation to learn exact prop types, whether props are optional, and any side effects.
- Because this appears to be an "inner"/presentational component, prefer passing already-resolved data and handlers from a higher-level container rather than performing data fetching inside this component.

---

## ProjectDiagnosticsPage

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Renders the diagnostics UI for a single project by reading the projectId route parameter and delegating to DiagnosticsPageInner. If the projectId param is missing the component renders nothing; use this component on routes that include a :projectId segment.

## Remarks
This is a thin, stateless wrapper whose sole responsibility is to extract the projectId from React Router params and provide a consistent set of props to DiagnosticsPageInner (source with kind 'project', backTo '/', and scopeLabel 'project'). It keeps routing concerns separate from the diagnostics view and lets DiagnosticsPageInner remain reusable for other scopes.

## Example
```typescript
// react-router v6 example
import { Route, Routes } from 'react-router-dom';

<Routes>
  <Route path="/projects/:projectId/diagnostics" element={<ProjectDiagnosticsPage />} />
</Routes>
```

## Notes
- The component returns null when projectId is falsy; ensure the route includes the :projectId param or handle redirection elsewhere.
- Must be rendered inside a Router context (useParams is used).

---

## onBack

> **File:** `src/webapp/src/pages/DiagnosticsPage.tsx`  
> **Kind:** function

Navigates to an explicit back target when one is provided; otherwise falls back to going back one entry in the browser history. Use as a reusable back-button handler in pages that sometimes need to return to a specific route but should otherwise behave like a normal "back" action.

## Remarks
Centralizes the back-navigation decision so callers (e.g. a UI back button) do not need to duplicate conditional logic. The function expects a surrounding scope to provide a truthy `backTo` value (a route or location) and a `navigate` function; when `backTo` is present it is used as the destination, otherwise `navigate(-1)` is used to step the history stack.

## Example
```typescript
// Typical usage inside a React component using react-router
import { useNavigate } from 'react-router-dom';

function Example({ backTo }: { backTo?: string }) {
  const navigate = useNavigate();

  const onBack = () => {
    if (backTo) navigate(backTo);
    else navigate(-1);
  };

  return <button onClick={onBack}>Back</button>;
}
```

## Notes
- If `backTo` is falsy (undefined, null, empty string, 0, etc.) the function will call `navigate(-1)`; ensure an empty string is not intended to be a valid route.
- Calling `navigate(-1)` has no effect if there is no previous history entry (for example, when the page was opened directly); handle that scenario if a fallback destination is required.

---