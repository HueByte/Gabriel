# ModelSelector.tsx

> **Source:** `src/webapp/src/components/ModelSelector.tsx`

## Contents

- [ModelSelector](#modelselector)
- [labelFor](#labelfor)

---

## ModelSelector
> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

```typescript
export function ModelSelector()
```


ModelSelector is a React functional component that renders an labeled dropdown to choose the active model from a server-provided list. On mount, it fetches the available models via fetchModels and stores them locally, showing a loading state until data arrives and surfacing non-Abort errors if the load fails. When a user selects a model, it updates the server with setActiveModel and then refreshes its local state with the server’s canonical response so the dropdown reflects exactly what the server stored. The currently selected option is derived from data.selected as provider::name, and the control is disabled while a save is in progress. A brief hint explains that changes apply on the next message and that the default option comes from appsettings.json. It also uses an AbortController to cancel the fetch if the component unmounts, avoiding potential leaks or state updates after unmount.

## Remarks
ModelSelector encapsulates the UI and synchronization logic for choosing the active model behind a simple dropdown. It hides the asynchronous fetch and server-update details, coordinating with fetchModels, setActiveModel, and appsettings-backed defaults to present a coherent, server-backed selection experience. The component employs an optimistic-ish pattern by re-reading the server’s canonical state after a successful update, ensuring the UI mirrors the authoritative server data and reducing drift between client and server.

## Notes
- AbortController is used to cancel the in-flight fetch on unmount, preventing memory leaks or state updates after the component is removed.
- If the initial load fails, the component renders an error state; if an update fails, the error is surfaced below the picker.
- The select is disabled during saving to prevent concurrent updates; the displayed value switches to the server-confirmed provider::name only after the update resolves.

---

## labelFor
> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

```typescript
function labelFor(m: ModelDto): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `m` | `ModelDto` | — |

**Returns:** `string`


Computes a concise display label for a ModelDto used in the model selector UI. It assembles the model’s provider and name, reports the context window length in thousands of tokens, and conditionally appends annotations for a default model and for a compact threshold override. Use this function when rendering a list of models to present a stable, human-friendly summary rather than raw object properties.

## Remarks

Centralizes label formatting so the UI stays consistent across components. It encodes decisions about default status and compact-override visibility; when compactThreshold is present, the label includes an explicit ", compact @ XX%" hint. By deriving the final string in one place, changes to how models are presented require updating only this function. It also documents the intended semantics: "default" marks the preset default model, and the "compact" annotation signals that the model trims context to fit pricing or performance constraints.

## Example

```typescript
// Example: a non-default model with a 200k context and a compact threshold
const m: ModelDto = {
  provider: 'grok',
  name: 'grok-4.3',
  isDefault: false,
  contextWindowTokens: 200000,
  compactThreshold: 0.75
};

const label = labelFor(m);
// label is: "grok / grok-4.3 — 200k ctx, compact @ 75%"
```

## Notes

- compactThreshold is optional; if null/undefined, the label omits the 'compact' annotation.
- contextWindowTokens is displayed as an integer number of thousands of tokens (e.g., 200000 => 200k).
- If isDefault is true, the label includes a trailing " (default)" tag.


---