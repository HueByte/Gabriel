# ModelSelector.tsx

> **Source:** `src/webapp/src/components/ModelSelector.tsx`

## Contents

- [ModelSelector](#modelselector)
- [labelFor](#labelfor)

---

## ModelSelector

> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

Renders a small model-selection control that loads available models from the server on mount and lets the user change the active model. It shows loading and error states, disables the picker while an update is in progress, and updates local state with the canonical server response after a change.

## Remarks
This component performs an initial fetch of model data when mounted and uses an AbortController so the request is cancelled if the component unmounts. When the user picks a model the component calls the update API and replaces its local state with the response from the server (the code’s "optimistic-ish" behavior ensures the dropdown reflects whatever the server actually stored). The select's option values are encoded as "provider::name" and the component disables the control while a change is being saved.

## Notes
- The select value uses the literal format "provider::name"; an empty string is used to clear the selection and is translated to provider/name = null before the update call.
- The onPick handler is async and the select is disabled while saving; callers should not assume the change is instantaneous.
- Fetch errors named "AbortError" are ignored (caused by unmount); other errors are surfaced in the UI below the control.

---

## labelFor

> **File:** `src/webapp/src/components/ModelSelector.tsx`  
> **Kind:** function

Return a compact, human-readable label for a ModelDto suitable for use in a UI (for example, a dropdown or selector). It highlights the provider and model name, shows the context window in thousands of tokens, and optionally annotates models that use a compact trimming threshold or are the default.

## Remarks
This function centralizes the presentation logic for model options so the selector can show at-a-glance differences between models (context window size, whether a model trims early via a compact threshold, and which model is marked as the default). It keeps formatting consistent across the UI and avoids repeating labeling logic at each call site.

## Example
```typescript
const model: ModelDto = {
  provider: 'openai',
  name: 'gpt-4o-mini',
  contextWindowTokens: 200000,
  compactThreshold: 0.95,
  isDefault: true,
};

console.log(labelFor(model));
// Output: "openai / gpt-4o-mini — 200k ctx, compact @ 95%(default)"
```

## Notes
- The context window is shown in thousands (contextWindowTokens / 1000) and rounded with toFixed(0), so smaller windows may display as "0k".
- compactThreshold is treated as a fraction (0–1); it is multiplied by 100 and rounded to the nearest integer for display. The check uses != null, so both null and undefined are treated as absent.
- The label is plain text and not localized; it uses English abbreviations ("k" and "ctx").

---