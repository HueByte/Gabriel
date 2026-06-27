# ModelSelector.tsx

> **Source:** `src/webapp/src/components/ModelSelector.tsx`

## Contents

- [ModelSelector](#modelselector)
- [labelFor](#labelfor)

---

## ModelSelector

> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

Renders a controlled dropdown that displays available models and lets the user change the application's active model. The component fetches the list of available models and the currently selected model on mount, shows loading and error states, and calls setActiveModel when the user picks a different model; it disables the picker while the change is being saved.

## Remarks
ModelSelector coordinates three responsibilities: loading the canonical model list and current selection (via fetchModels), sending updates to the server (via setActiveModel), and reflecting the server's authoritative state by replacing local data with the PUT response. It uses an AbortController to cancel the initial fetch on unmount and encodes model identifiers as a "provider::name" string when communicating with the select element.

## Notes
- The code assumes the fetched ModelsResponse always contains a non-null `selected`. If `data.selected` can be null/undefined, constructing `"${data.selected.provider}::${data.selected.name}"` will throw at render time.
- onPick treats an empty string value (`""`) as a request to clear the active model (provider and name set to null). The rendered options do not include an explicit empty option, so clearing can only occur if an empty value is provided programmatically or added to the options list.
- AbortError thrown by the fetch is intentionally ignored; other errors are surfaced to the user via the component's error state.

---

## labelFor

> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

Returns a compact, human-readable label for a ModelDto suitable for showing in a model-selection dropdown. It combines provider and model name, rounds the context window (in thousands of tokens) to an integer with a "k ctx" suffix, optionally appends a compact-threshold percentage when present, and marks the label with " (default)" if the model is the default selection.

## Remarks
This helper centralizes the UI text formatting for model choices so the dropdown can surface at-a-glance differences between models (for example, which models have a compact threshold that may cause earlier trimming and therefore affect cost or behavior). It keeps presentation logic out of the JSX markup and ensures consistency across lists of models.

## Example
```typescript
const model: ModelDto = {
  provider: 'grok',
  name: 'grok-4.3',
  contextWindowTokens: 200000,
  compactThreshold: 0.8, // 80%
  isDefault: false,
};

console.log(labelFor(model));
// Output: "grok / grok-4.3 — 200k ctx, compact @ 80%"

const defaultModel: ModelDto = {
  provider: 'openai',
  name: 'gpt-4o',
  contextWindowTokens: 32768,
  compactThreshold: null,
  isDefault: true,
};

console.log(labelFor(defaultModel));
// Output: "openai / gpt-4o — 33k ctx (default)"
```

## Notes
- compactThreshold is checked with != null, so both null and undefined are treated as "not present."
- The context window is divided by 1,000 and formatted with toFixed(0) (an integer string) followed by "k ctx"; small windows (<1000 tokens) will render as "0k ctx."
- The compact percentage uses Math.round(m.compactThreshold * 100), so it shows integer percentages (no fractional precision) and may round up/down compared to the raw value.
- The output is not localized (hard-coded English suffixes and punctuation); adjust if localization is required.


---