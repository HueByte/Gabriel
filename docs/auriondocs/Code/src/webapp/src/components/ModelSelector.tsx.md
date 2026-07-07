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


ModelSelector renders a dropdown to pick the active model. It fetches the list of available models from the server when mounted, shows a loading state while waiting, surfaces non-abort errors, and updates the server-side active model when a user selects a different option. The component uses the server's response as the canonical state to reflect what was actually stored.

## Remarks
By encapsulating the fetch and update logic, ModelSelector provides a single, reusable UI primitive for model selection. It delegates data fetching to fetchModels and state changes to setActiveModel, keeping concerns separated. After a successful update, it refreshes its data with the response, avoiding drift between client and server. The hint text in the UI communicates that changes apply on the next message and ties the selection to the default configured in appsettings.json.

## Notes
- Encoding and parsing rely on the delimiter :: in the composite value provider::name; if either field contains ::, the selection value would be parsed incorrectly.
- The code path allows an empty value ('') to represent a reset, but there is no explicit empty option in the dropdown, which can be confusing.
- The component uses AbortController to cancel the initial fetch on unmount; if the fetch implementation ignores the AbortSignal, there could be a brief, unsafe state transition.

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


Computes a concise, human-friendly label for a ModelDto to display in UI dropdowns. It composes the provider and model name, the model's context window size expressed in thousands of tokens, and optional extras: a per-model compact threshold override and a marker when the model is the default choice. This function centralizes formatting logic so all dropdowns show consistent labels, and developers would call it rather than duplicating string assembly.

## Remarks
Labeling is kept in one place to ensure consistent presentation across the model selector UI. It encapsulates display concerns: whether a model is the default choice, how much context it can consume, and any compact threshold override that affects perceived cost. By taking a ModelDto and returning a single string, it decouples UI chrome from the underlying data shape, making future tweaks to the label format less risky.

## Example
```typescript
const m: ModelDto = {
  provider: 'gpt',
  name: 'grok-4.3',
  isDefault: true,
  contextWindowTokens: 200000,
  compactThreshold: 0.75
};
labelFor(m); // -> "gpt / grok-4.3 — 200k ctx, compact @ 75% (default)"
```

## Notes
- If compactThreshold is null/undefined, the suffix ", compact @ ..." is omitted.
- The label uses an em dash and specific spacing to produce consistent alignment in the UI.
- The context window is displayed as a rounded number of thousands of tokens (e.g., 200k), derived from dividing tokens by 1000.


---