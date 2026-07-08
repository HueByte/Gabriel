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


ModelDto is a lightweight data transfer object that captures a single model's identity, cost parameters, and UI state for selection. Use it when listing available models, transmitting per-model configuration, or persisting a choice between client and server layers without pulling in broader domain logic.

## Remarks
ModelDto isolates model identity, economics, and selection state from the rest of the system. A null compactThreshold inherits the global compact policy from AgentOptions, while a numeric value imposes a per-model fraction of the context window for compaction. The price fields encode per-million-token costs for inputs, outputs, and cache operations, enabling simple budgeting per model. The isDefault and isSelected flags drive UI defaults and current user choice.

## Notes
- Be explicit about compactThreshold: null means inheritance; do not omit the field.
- Ensure consistent use of isDefault and isSelected across the model list to avoid conflicting UI states.
- Serialization: keep null vs undefined distinct to preserve inheritance semantics when sending ModelDto across processes.

---

## ModelsResponse
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface ModelsResponse
```


ModelsResponse is a lightweight interface that models the payload returned by the models API. It carries two related pieces of data: availableModels, an array of ModelDto objects describing each available model; and selected, a SelectedModelDto representing the model currently chosen. This shape lets callers fetch both the list of options and the active selection in a single response, which is convenient for populating UI controls and maintaining selection state.

## Remarks
This interface acts as a compact boundary between backend data and frontend presentation. By pairing the options list with the selected item, it reduces the number of roundtrips needed for a model picker and eliminates the need for separate lookups to determine the current choice. The two collaborator DTOs—ModelDto and SelectedModelDto—keep model metadata and selection state distinct while still composing a coherent view of the model domain.

---

## SelectedModelDto
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SelectedModelDto
```


SelectedModelDto is a lightweight data transfer object that identifies a model chosen from a provider by its provider identifier and model name. It is used when API boundaries require a single, stable payload to convey which model should be used.

## Remarks
By separating provider and model name, this interface enables generic handling of models from multiple providers without coupling to any specific provider API. It acts as a simple contract that supports routing, logging, and auditing decisions based on the selected model. The shape is resilient to provider-specific changes, so client-side code can stay stable while backends evolve the available models.

---

## SetActiveModelRequest
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SetActiveModelRequest
```


SetActiveModelRequest is a small data contract used to request changing the active model by supplying a provider and a model name. Both fields are nullable, so callers can omit or explicitly set a value to express their intent.

## Remarks

This interface defines the boundary between the client API and the server-side active-model management. It decouples the API surface from internal domain types, enabling the client to convey intent without exposing implementation details. The nullable properties support partial updates and clear intent when only one aspect (provider or model name) needs to be changed.

## Notes

- Nullable fields mean you must validate on the server and define how nulls are interpreted (no-op, reset, or error).
- Ensure that a meaningful combination of provider and name is validated against known models; otherwise the request may fail during backend validation.
- Be consistent with other API models that use nullable strings to represent optional fields.

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


Fetches the list of available models from the backend by calling /api/models and returning it as a ModelsResponse. It delegates the actual HTTP work to withRefresh, ensuring a refreshed session before issuing the request. The function supports cancellation via an optional AbortSignal; if provided, the request will be aborted according to the signal. On success, it parses the response body as JSON and returns it; if the HTTP response indicates failure (non-OK status), it throws an Error that includes the HTTP status.

## Remarks
Consolidates the API surface for retrieving models in one place, centralizing error handling and session refresh logic via withRefresh. The use of an AbortSignal enables responsive UIs to cancel in-flight requests when a component unmounts or a user navigates away. The return type ModelsResponse provides a typed contract for downstream processing, reducing the risk of misinterpreting the payload.

## Example
```typescript
// Example usage: cancel pending fetch with AbortController
async function loadModelsWithTimeout() {
  const controller = new AbortController();
  // Cancel after 5 seconds to demonstrate abort behavior
  const timer = setTimeout(() => controller.abort(), 5000);
  try {
    const models = await fetchModels(controller.signal);
    console.log('Retrieved models:', models);
    return models;
  } catch (err) {
    if (err && (err as any).name === 'AbortError') {
      console.log('FetchModels request was aborted');
    } else {
      throw err;
    }
  } finally {
    clearTimeout(timer);
  }
}
```

## Notes
- Aborting the request will reject the returned promise with an AbortError; callers should be prepared to catch and handle this scenario.
- The function forwards the AbortSignal to the underlying fetch call, so cancellation behavior depends on the environment's fetch implementation.
- It uses credentials: 'include', which means cookies or other credentials are sent with the request; ensure the backend CORS configuration allows this.


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


Updates the active model on the server by sending a PUT request to /api/models/active with the provided SetActiveModelRequest payload. The operation is wrapped by withRefresh to ensure any necessary authentication refresh happens before the request, and it supports cancellation via an AbortSignal passed through to fetch. On success, it returns the server’s ModelsResponse; on failure it throws a descriptive error that includes the HTTP status and any response text.

## Remarks
Centralizes the HTTP interaction for model activation and provides consistent error handling for callers. It relies on the withRefresh helper to keep authentication current and uses credentials: 'include' to permit cookie-based sessions. By accepting an AbortSignal, callers can cancel in-flight requests to avoid UI stalls during navigation or shutdown.

## Notes
- If the server returns a non-2xx status, the function includes the status code and any textual body in the thrown Error; if the body cannot be read, the text portion defaults to an empty string.
- The function casts the successful response body to ModelsResponse; ensure the server response conforms to that shape to avoid runtime type issues.

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


withRefresh is an async wrapper that runs a provided fetch function and, if the response is 401, attempts to refresh the session and retry once. If the retry still returns 401, it signals that the session has expired and throws an error prompting the user to sign in again. Developers would reach for it to automatically recover from expired authentication on API calls, centralizing 401 handling and session refresh logic.

## Remarks
This wrapper encapsulates the authenticated request flow: execute the call, refresh on unauthorized, retry, and fail-fast if the session cannot be refreshed, promoting a consistent, centralized approach to session management.

---