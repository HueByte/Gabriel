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

Represents model metadata returned by the server for use in the web UI and client logic. Contains identifying information (provider/name), the model's token window, model-specific compaction guidance, per-token pricing (expressed per MTokens), and simple flags used by the UI or selection logic.

## Remarks
This DTO bundles both capability information (contextWindowTokens, compactThreshold) and cost metrics (input/output/cache read/write prices) so callers can present model choices and compute cost estimates without further lookups. The compactThreshold field can be null to indicate the model should inherit the global AgentOptions.CompactThreshold; when non-null it is a fractional threshold (e.g. 0.18 means "compact when the used portion of the context window reaches 18%").

## Example
```typescript
const example: ModelDto = {
  provider: "acme-ai",
  name: "acme-text-4k",
  contextWindowTokens: 4096,
  compactThreshold: null, // inherit global AgentOptions.CompactThreshold
  inputPricePerMTokens: 2.5,    // price per 1,000,000 input tokens
  outputPricePerMTokens: 5.0,   // price per 1,000,000 output tokens
  cacheReadPricePerMTokens: 0.1,
  cacheWritePricePerMTokens: 0.05,
  isDefault: false,
  isSelected: false,
};

// Use case: show model in a list and compute an estimated cost for a request
const estimatedInputCost = (example.inputPricePerMTokens / 1_000_000) * 1500; // for 1500 input tokens
```

## Notes
- compactThreshold is a fraction (expected between 0 and 1); null means "use the global setting" — it is NOT an absolute token count.
- Prices are expressed per MTokens (per 1,000,000 tokens). The DTO does not include currency information; consumers should apply the appropriate currency/formatting.
- The DTO contains isSelected and isDefault booleans; confirm whether those flags are authoritative server state or intended for client-side UI state before relying on them for persistence or business logic.

---

## ModelsResponse

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

Represents the payload that lists all models available to the client together with which model is currently selected. Use this DTO when receiving model information and the current selection from the API so the UI or other consumers can render available options and highlight or act on the selected model.

## Remarks
This interface groups two related pieces of data: a collection of ModelDto entries (the universe of choices) and a SelectedModelDto (the single choice the user or system has marked as active). Returning them together avoids an extra lookup and keeps list rendering and selection state synchronized in one response.

## Example
```typescript
// Assume `fetchModels()` returns a ModelsResponse from the server
const resp: ModelsResponse = await fetchModels();

// Render simple list and mark the selected model (matching strategy depends on DTO shape)
resp.availableModels.forEach(model => {
  const isSelected = /* compare model with resp.selected, e.g. model.id === resp.selected.id */ false;
  console.log(model, isSelected ? '(selected)' : '');
});

// If you need the selected object from the available list:
const selectedFromList = resp.availableModels.find(m => /* match m with resp.selected */ false);
if (!selectedFromList) {
  // fallback: use resp.selected directly or fetch details as needed
}
```

## Notes
- Do not assume resp.selected is necessarily present inside availableModels; handle the case where the selected model must be used directly or fetched separately.
- These are plain data transfer objects (DTOs); mutating resp.availableModels or resp.selected will affect local state but will not update server-side state unless sent back explicitly.


---

## SelectedModelDto

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

Identifies a chosen model by its provider and model name/identifier. Use this DTO when conveying which model a user or system selected (for example in API requests, responses, or UI state) rather than passing raw configuration objects.

## Remarks
This is a minimal data-transfer object that pairs a provider identifier with a model name. It intentionally carries no behavior or validation: it only conveys identity. Any interpretation (resolving to a concrete model, validating availability, or mapping to service-specific configuration) should happen outside this DTO.

## Example
```typescript
const selected: SelectedModelDto = {
  provider: 'example-provider',
  name: 'model-v1'
};
// Send as part of an API request or store in UI state
```

## Notes
- Both properties are required strings; the combination of provider + name is typically used to uniquely identify a model.
- This interface does not perform validation or normalization (e.g., trimming or case normalization); callers should prepare values appropriately before using them where identity matters.

---

## SetActiveModelRequest

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

Represents the request payload used by the webapp to set (or clear) the active model for a provider. Use this interface when calling the backend endpoint that updates which model is considered active — it conveys both the provider identifier and the model name to apply.

## Remarks
This is a simple data-transfer object (DTO) between the client and server. Both properties are typed as string | null: the string value selects the provider or model by name, while null is an explicit value that a server can interpret as "unset" or "clear" for that field. Note that the properties are nullable but not optional — callers must include the properties on the object (they may be set to null if no value is intended).

## Example
```typescript
// Set the active model for a provider
const req: SetActiveModelRequest = {
  provider: "openai",
  name: "gpt-4"
};
// Clear the active model name for a provider (explicit null)
const clearNameReq: SetActiveModelRequest = {
  provider: "openai",
  name: null
};
// Explicitly indicate no provider/model (both null)
const clearAllReq: SetActiveModelRequest = {
  provider: null,
  name: null
};
```

## Notes
- The properties are nullable (string | null) but required on the object; when constructing instances make sure to include both keys rather than omitting them.
- JSON serialization will include nulls (e.g. { "name": null }), so verify how the server interprets null versus an absent property.
- When using TypeScript's strict null checks, callers will need to handle the union type or assert a non-null value before sending.

---

## fetchModels

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Fetches the application's models from the server endpoint (/api/models) and returns the parsed JSON as a ModelsResponse. Use this helper when the UI or other client code needs the server-provided list of models; it automatically includes credentials, supports cancellation via an AbortSignal, and relies on withRefresh to perform the fetch (so authentication/session refresh logic is applied).

## Remarks
This centralizes model retrieval so callers don't need to repeat fetch boilerplate or handle JSON parsing and common error handling. The call is wrapped with withRefresh, which is intended to ensure authentication/session refresh behavior is applied before or during the request; callers therefore get a consistent, authenticated fetch for model data.

## Example
```typescript
const controller = new AbortController();
try {
  const models = await fetchModels(controller.signal);
  console.log('Models received:', models);
} catch (err) {
  console.error('Failed to load models:', err);
}
// to cancel:
// controller.abort();
```

## Notes
- The function throws an Error when the HTTP response is not ok; callers must catch errors to handle non-2xx responses.
- The returned value is the parsed JSON cast to ModelsResponse — there is no runtime schema validation here, so mismatched shapes will only surface at runtime.
- The request uses credentials: 'include' (cookies are sent) and accepts an AbortSignal for cancellation; both behaviors affect how the request is sent and how it can be aborted.

---

## setActiveModel

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Sends a PUT request to the server endpoint /api/models/active with the given SetActiveModelRequest as a JSON body to change which model is active. Returns the parsed ModelsResponse on success and throws an Error if the HTTP response is not successful. Use this from client code when you need to update the active model for the current user/session.

## Remarks
This function is a thin API-client helper that centralizes the HTTP request, JSON serialization, credentials and basic error handling for the "set active model" operation. The network call is wrapped with withRefresh, a shared helper used across the client to apply cross-cutting behaviors (for example, credential/authorization handling or automatic token refresh). Consumers get a parsed ModelsResponse or an exception describing the failure; there is no additional runtime validation of the response shape beyond JSON parsing.

## Example
```typescript
import { setActiveModel } from './api/models';

const controller = new AbortController();
try {
  const request = { modelId: 'my-model-id' } as SetActiveModelRequest;
  const result = await setActiveModel(request, controller.signal);
  console.log('Updated models:', result);
} catch (err) {
  console.error('Failed to set active model:', err);
}
```

## Notes
- If the response has a non-2xx status the function throws an Error containing the status code and any response text; callers should catch and handle this.
- The request uses credentials: 'include' so cookies (and other same-origin credentials) are sent with the request.
- The optional AbortSignal allows callers to cancel the request; aborting will reject the fetch and typically surface as an exception.
- The returned value is cast to ModelsResponse after JSON parsing — there is no schema validation at runtime, so mismatched shapes will only surface as downstream type assumptions fail.

---

## withRefresh

> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

Wraps a single fetch-like operation and transparently attempts a session refresh on HTTP 401 responses. Call this when you want API requests to automatically try renewing authentication and only surface a final "session expired" error if a refresh does not resolve the 401.

## Remarks
Centralizes the common pattern of retrying a failing request after refreshing authentication state. The provided doFetch callback is invoked once normally and at most once more after a successful refresh; if the response is still 401 the function signals session expiration and throws an Error. This keeps retry logic in one place but does not implement advanced concurrency controls (e.g., queuing or deduping simultaneous refresh attempts).

## Example
```typescript
// Typical usage wrapping a fetch call
try {
  const res = await withRefresh(() => fetch('/api/profile', { credentials: 'include' }));
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  const profile = await res.json();
  console.log(profile);
} catch (err) {
  // If the session could not be refreshed, withRefresh throws an Error
  console.error(err);
}
```

## Notes
- doFetch may be called twice: ensure the operation is safe to retry or idempotent.
- Errors thrown by doFetch or refreshSession propagate to the caller; this function does not swallow network or other exceptions.
- The implementation relies on externally defined refreshSession() and signalSessionExpired() functions being available and correctly updating authentication state before the retry.
- If refreshSession returns true but the retry still receives 401, the function signals session expiration and throws an Error with message 'Session expired. Please sign in again.'


---