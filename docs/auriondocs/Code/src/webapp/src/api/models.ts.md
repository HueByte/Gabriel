# models.ts

> **Source:** `src/webapp/src/api/models.ts`

## Contents

- [ModelDto](#modeldto)
- [ModelsResponse](#modelsresponse)
- [SelectedModelDto](#selectedmodeldto)
- [SetActiveModelRequest](#setactivemodelrequest)
- [fetchModels](#fetchmodels)
- [setActiveModel](#setactivemodel)
- [withRefresh](#withrefresh)

---

## ModelDto

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

Represents a model's metadata and pricing information exchanged via the web API. Use this DTO when listing available models, showing per-model pricing and limits, or persisting which model is currently selected or considered the default.

## Remarks
This interface centralizes the properties needed by the UI and server to present model choices, estimate costs, and decide when to compact context. The compactThreshold is model-specific but may be null to inherit a global AgentOptions.CompactThreshold; pricing fields are numeric rates used for cost calculation (unit semantics are defined elsewhere in the API). The isDefault and isSelected flags distinguish a model that is marked as the default from one that is currently selected in the active context.

## Example
```typescript
const exampleModel: ModelDto = {
  provider: "openai",
  name: "gpt-4o-mini",
  contextWindowTokens: 8192,
  // null -> use global AgentOptions.CompactThreshold
  compactThreshold: 0.18,
  inputPricePerMTokens: 0.3,
  outputPricePerMTokens: 0.5,
  cacheReadPricePerMTokens: 0.01,
  cacheWritePricePerMTokens: 0.02,
  isDefault: false,
  isSelected: true,
};
```

## Notes
- If compactThreshold is null the system should fall back to the global AgentOptions.CompactThreshold; when present it is a fraction (e.g. 0.18 means 18% of contextWindowTokens).
- To compute the absolute token count that triggers compaction multiply contextWindowTokens by compactThreshold.
- The numeric price fields are rates per “M tokens”; confirm the exact meaning of "M" (thousand vs million) and the currency/units from the API documentation before using these values for billing.
- This DTO carries presentation/selection state (isSelected) in addition to persisted model metadata (isDefault); consumers should treat isSelected as contextual and transient where appropriate.

---

## ModelsResponse

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

Represents the API response that delivers both the set of available models and the currently selected model. Use this interface when consuming the models endpoint to render choices and the active selection together (for example, in a model picker UI or configuration screen).

## Remarks
This DTO groups the full list of model options (availableModels) with the active selection (selected) so callers have both the choices and the current state in a single payload. That reduces extra requests and keeps rendering logic simple — the UI can show the available options and highlight the selected one using the same response.

## Example
```typescript
// Typical usage when calling a server endpoint that returns ModelsResponse
async function loadModels(): Promise<void> {
  const resp = await fetch('/api/models');
  const body = (await resp.json()) as ModelsResponse;

  // availableModels: array of ModelDto
  const options = body.availableModels.map(m => m.name);

  // selected: SelectedModelDto describing the current choice
  console.log('Selected model:', body.selected);
}
```

## Notes
- availableModels may be an empty array; callers should handle that case in the UI.
- The selected entry typically corresponds to one of the availableModels; validate or handle mismatches if the server-side contract isn't enforced.


---

## SelectedModelDto

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

A minimal data-transfer object that identifies a selected model by its provider and name. Reach for this shape when you need to pass or store a compact reference to a model (for example, from UI selection controls to an API) rather than its full configuration or capabilities.

## Remarks
This DTO contains only identifying information: the provider (which names the vendor or service) and the model name (the provider-specific model identifier). It is intentionally small and focused so it can be used in lists, selection controls, and light-weight API payloads where full model metadata is unnecessary.

## Example
```typescript
// Create a selection record from a UI choice
const selected: SelectedModelDto = {
  provider: 'openai',
  name: 'gpt-4'
};

// Send to an API that expects a model identifier
await fetch('/api/models/select', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(selected)
});
```

## Notes
- Both properties are required strings; the interface does not perform runtime validation.
- Ensure provider and name values match the server or registry expectations (IDs, casing, etc.), since this DTO only carries the raw identifiers.

---

## SetActiveModelRequest

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

A simple data transfer object that describes which model should be set as the active model for a given provider. Use this interface when sending or receiving payloads that change the currently selected model — for example, from a web UI to an API endpoint that stores the active provider and model name.

## Remarks
This interface intentionally uses nullable string fields (string | null) instead of optional properties so callers must explicitly provide a value for each property. That explicitness makes it possible to distinguish between "no change" (omitted in some APIs) and an intentional clearing of the value (null). It is a plain DTO — it carries data only and does not perform validation or side effects.

## Example
```typescript
// Select a model
const setActive: SetActiveModelRequest = {
  provider: "openai",
  name: "gpt-4o-mini"
};

await fetch('/api/active-model', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(setActive)
});

// Clear the currently selected model for the provider
const clearModel: SetActiveModelRequest = {
  provider: "openai",
  name: null
};

await fetch('/api/active-model', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(clearModel)
});
```

## Notes
- Both properties are required by the type but may be null; do not use undefined if the API expects this shape.
- Null commonly signals an explicit "clear" action; an empty string may be interpreted differently by the server—prefer null to indicate absence.
- This interface does not validate that provider or name correspond to known values; validate on the client or server as appropriate.

---

## fetchModels

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Fetches the server's models list and returns it as a ModelsResponse. Use this helper when you need the canonical list of models from the backend (it sends credentials/cookies and integrates with the app's refresh logic) and you want a Promise that either resolves to the parsed response or rejects on failure.

## Remarks
This function delegates the actual HTTP call to a withRefresh wrapper, so it participates in whatever token/refresh and retry behavior that helper implements. It always sends credentials (cookies) with the request and accepts an optional AbortSignal for cancellation; callers should provide a signal when they need the ability to cancel the request.

## Example
```typescript
const controller = new AbortController();
try {
  const models = await fetchModels(controller.signal);
  console.log('Received models:', models);
} catch (err) {
  console.error('Failed to fetch models:', err);
}
// To cancel: controller.abort();
```

## Notes
- The function throws a generic Error if the HTTP response has a non-OK status (response.ok === false); inspect the error message for the status and statusText.
- The JSON body is cast to ModelsResponse without runtime validation — malformed responses will still be returned as that type at runtime and may cause downstream errors.
- Network errors and any errors from withRefresh propagate to the caller; handle with try/catch or let the promise reject.

---

## setActiveModel

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Sends a PUT request to the server endpoint /api/models/active with the provided SetActiveModelRequest payload and returns the parsed ModelsResponse. Use this helper from client-side code when you need to change which model the server considers "active" and want the response deserialized for further use.

## Remarks
The network call is performed via the withRefresh helper (the request function is passed into withRefresh). The request is sent as JSON (Content-Type: application/json) and includes credentials ('include') so cookies or other credentialed transport are sent. If the HTTP response is not ok the function throws an Error containing the status code and any response body text it can read.

## Example
```typescript
const controller = new AbortController();
try {
  const request: SetActiveModelRequest = { /* fields per type definition */ };
  const result = await setActiveModel(request, controller.signal);
  // result is a ModelsResponse — handle updated models state here
} catch (err) {
  // network or server error (non-2xx responses) will be thrown as Error
  console.error('Unable to set active model:', err);
}
```

## Notes
- On non-2xx responses an Error is thrown; the message includes the HTTP status and any response text the client could read.
- The call uses credentials: 'include', so cookies and other credentialed transports configured for the origin will be sent.
- The optional AbortSignal allows callers to cancel the request; if provided, aborting the signal will cancel the underlying fetch.

---

## withRefresh

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Attempts a single session refresh when an HTTP 401 is encountered and retries the fetch once. Use this wrapper around network calls that should transparently try to renew an authentication session before giving up; if the retry still returns 401 the function signals that the session has expired and throws an Error so callers can handle sign-in flows.

## Remarks
This helper centralizes the common pattern of "try request, refresh session on 401, retry once" so callers don't need to implement the sequence repeatedly. It performs at most one refresh attempt per invocation and relies on external functions (refreshSession and signalSessionExpired) to perform the refresh and to notify the application that the session is no longer valid.

## Example
```typescript
// Typical usage with the Fetch API
try {
  const resp = await withRefresh(() => fetch('/api/private/data', { credentials: 'include' }));
  if (!resp.ok) throw new Error(`Request failed: ${resp.status}`);
  const data = await resp.json();
  // use data
} catch (err) {
  // handle session expiry (withRefresh throws when it detects an expired session)
  console.error(err);
}
```

## Notes
- doFetch may be invoked twice; avoid passing a fetch wrapper that has one-time side effects (for example a Request whose body is a consumed stream) unless it can be safely retried.
- refreshSession and signalSessionExpired are external and may throw or have side effects; exceptions from refreshSession will propagate to the caller.
- withRefresh only retries once after a successful refresh; it does not implement exponential backoff or repeated refresh attempts.
- The function throws a generic Error with message "Session expired. Please sign in again." when the retry still returns 401; callers may want to catch and handle that specific message or replace it with an application-specific error type.
```

---