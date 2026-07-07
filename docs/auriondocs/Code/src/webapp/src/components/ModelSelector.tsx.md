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


ModelSelector is a self-contained React component that fetches the list of available models on mount and renders a dropdown labeled “Active model.” It shows loading and error states, and when a user selects a model it updates the server via setActiveModel and then re-syncs its local state from the server’s response to reflect the canonical state. The dropdown options are derived from data.availableModels and displayed as provider::name with the visible label provided by labelFor.

## Remarks
Architecturally, it encapsulates the data lifecycle for the active model: it fetches models, handles updates, and re-syncs from the server response, reducing the need for parent components to manage this state. It also performs an optimistic-ish update by re-reading the server’s PUT response to ensure the dropdown reflects what the server actually stored.

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


labelFor constructs a compact, display-oriented label for a ModelDto. It concatenates the model’s provider and name, shows the context window in thousands of tokens, and conditionally appends information about a per-model compact threshold and whether the model is the default. This is a UI helper used in dropdowns to give users a quick, at-a-glance understanding of each model’s characteristics without inspecting the raw data. The function deliberately centralizes formatting so the same visual presentation is used consistently across the UI.

## Remarks
This abstraction isolates presentation concerns from the rest of the UI logic. By coding the label composition in one place, the dropdown and any other model-listing components stay aligned in how they convey context (provider/name, context size, and trim/default hints). It also makes it straightforward to adjust how model metadata is shown without touching multiple call sites.

## Example
```typescript
const m: ModelDto = {
  provider: 'OpenAI',
  name: 'grok-4.3',
  isDefault: true,
  contextWindowTokens: 200000,
  compactThreshold: 0.75
};

console.log(labelFor(m)); // "OpenAI / grok-4.3 — 200k ctx, compact @ 75%, (default)"
```

## Notes
- If compactThreshold is null, the trailing ", compact @ ...%" portion is omitted.
- The default indicator is appended only when isDefault is true; otherwise, no extra tag is shown.

---