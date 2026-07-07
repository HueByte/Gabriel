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

```typescript
export interface ModelDto
```


Represents a model option in the web API surface. ModelDto aggregates identifying information (provider and model name), operational attributes (contextWindowTokens, optional compactThreshold), pricing details (input/output/cache per MTokens), and UI state flags (isDefault, isSelected). The compactThreshold is nullable to allow inheriting the global setting from AgentOptions when null; otherwise it uses a model-specific fraction to determine when context-compact behavior should apply. This DTO is typically produced when listing available models or rendering selectable options, enabling consistent display, selection, and cost estimation without exposing internal model mechanics.

---

## ModelsResponse
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface ModelsResponse
```


ModelsResponse represents the API response that conveys both the list of available model descriptors and the currently selected model in the web client. It enables UI components to render the full option set while highlighting or using the active model without issuing separate calls.

## Remarks

It serves as a simple DTO boundary that decouples the catalog of options from the current selection. By bundling availableModels and selected in one object, the frontend can render dropdowns, lists, or other selectors while maintaining a single source of truth for the active model. This shape also makes it easy to extend with additional metadata in the future (for example, pagination or model status) without changing the contract elsewhere.

## Example

```typescript
// Basic usage example
function logModels(res: ModelsResponse): void {
  console.log(`Available models: ${res.availableModels.length}`);
  console.log(`Selected model present: ${!!res.selected}`);
}
```

## Notes

- The selected field may be null/undefined if no model is currently chosen; guard against dereferencing it.
- Treat the arrays and objects as data-transfer shapes from the API; avoid mutating them in-place to preserve immutability expectations.
- If consumers need more details about a model, make separate requests or rely on the ModelDto/SelectedModelDto structures provided by the API.

---

## SelectedModelDto
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SelectedModelDto
```


Represents a selected machine learning model by specifying its provider and model name. This interface is used to identify and reference a particular model within the application or API.


---

## SetActiveModelRequest
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SetActiveModelRequest
```


SetActiveModelRequest defines the payload used when requesting the activation of a model in the API. It contains two fields, provider and name, both of which are string or null. A caller uses this object to indicate which provider should supply the model and which model name should be activated; null values express that a particular field is not being specified in this request. This interface acts as a boundary between the HTTP API layer and the server-side model resolution logic.

## Remarks
SetActiveModelRequest is a minimal DTO that decouples the external API surface from the internal representation of models. The nullable fields provide flexibility for partial updates and forward-compatibility as the set-active semantics evolve. In practice, the provider and name are consumed by the service that selects the active model, keeping the transport contract stable while allowing internal naming and resolution to vary.

## Notes
- Null values are explicit in this contract; ensure your client serializes nulls if your API expects them, since some serializers drop nulls by default.
- If both fields are null, the API may treat the request as a no-op or return an error; verify the endpoint's contract before sending such a payload.

---

## fetchModels
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

```typescript
export async function fetchModels(signal?: AbortSignal): Promise<ModelsResponse>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<ModelsResponse>`


Fetches the list of available models from the server by issuing a GET request to /api/models, honoring an optional AbortSignal for cancellation and including credentials so cookies are sent. It delegates the actual network call to withRefresh to centralize refresh/retry behavior. If the server response is not OK, it throws an error containing the HTTP status and status text. On success, it parses the response body as JSON and returns it cast to ModelsResponse.

---

## setActiveModel
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

```typescript
export async function setActiveModel(
  request: SetActiveModelRequest,
  signal?: AbortSignal,
): Promise<ModelsResponse>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `request` | `SetActiveModelRequest` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<ModelsResponse>`


Sets the active model on the server by sending a PUT request to /api/models/active with a JSON payload describing the desired model. It relies on withRefresh to refresh authentication as needed and supports cancellation via an optional AbortSignal. On success, it returns the server’s ModelsResponse parsed from the JSON body. If the response is not OK, it reads any response text and throws an Error including the HTTP status and the text to help diagnose the failure.

## Remarks
This abstraction centralizes the mutation of the active model behind a single API surface. It handles token refresh and credential transmission, and surfaces a clear failure mode when the server rejects the request, keeping callers focused on business logic rather than transport details.

## Notes
- If the server returns non-JSON on a successful response, JSON parsing may fail (the function expects a JSON body that matches ModelsResponse).
- The error path includes response text for diagnostics; be mindful that large or sensitive content may appear in the error message.
- The AbortSignal is forwarded to fetch; passing a signal enables cancellation from the caller if needed.

---

## withRefresh
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** function

```typescript
async function withRefresh(doFetch: () => Promise<Response>): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `doFetch` | `() => Promise<Response>` | — |

**Returns:** `Promise<Response>`


withRefresh is an asynchronous helper that executes a provided fetch-like operation and automatically handles an expired session. It runs doFetch, and if the server responds with 401, it attempts to refresh the session; if the refresh succeeds, it retries the original fetch. If the retry also returns 401, it signals that the session has expired and throws a clear error. The function then returns the resulting Response. This pattern is useful when multiple API calls can encounter an expired session and you want a centralized retry flow instead of duplicating refresh logic at every call site.

## Remarks
By encapsulating the refresh-and-retry flow, withRefresh decouples authentication concerns from business logic, ensuring consistent handling of 401s across API calls. It collaborates with refreshSession to obtain a new token and signalSessionExpired to notify the app when re-authentication is required. This keeps API-call sites small and focused on data handling.

## Example
```typescript
async function loadCurrentUser() {
  const resp = await withRefresh(() => fetch('/api/me'));
  if (!resp.ok) {
    throw new Error(`Request failed: ${resp.status}`);
  }
  return resp.json();
}
```

## Notes
- If the initial doFetch returns 401 and refreshSession() returns false, the original 401 is still surfaced, and the function will throw a "Session expired. Please sign in again." error.
- The final error is a generic Error; callers can catch it to trigger a login flow or user-facing messaging.
- Non-401 errors (e.g., network failures) propagate as-is and are not intercepted by this wrapper.

---