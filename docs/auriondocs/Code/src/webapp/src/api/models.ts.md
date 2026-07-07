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


ModelDto is a data-transfer interface that describes a model's identity, usage characteristics, pricing, and UI state for the web application. It aggregates the model's provider and name, the context window size (contextWindowTokens), an optional compactThreshold that either inherits from the global AgentOptions or overrides it with a model-specific fraction, and per-token pricing for input, output, and cache operations. The isDefault and isSelected flags encode UI intent, such as which model is the default in a list or which model the user has currently chosen. Use this type whenever you need to pass model metadata between backend and frontend layers; it centralizes model descriptors and enables per-model overrides without altering the global configuration.

## Remarks
CompactThreshold semantics: null means inherit from AgentOptions.CompactThreshold; a non-null number is the model's explicit fraction of the window used for compacting. This separation lets a global default be tuned centrally while still permitting per-model customization. The boolean flags isDefault and isSelected are for UI state management and should be treated as ephemeral; they do not define model capabilities.

## Notes
- When computing the effective compact threshold, resolve the null value to the global default before any logic that depends on the threshold.
- The numeric price fields are model-level costs used for billing or display; ensure consistent currency handling across the UI.

## Dependencies
- AgentOptions

## Dependency APIs
- class AgentOptions (src/api/Gabriel.Core/Configuration/AgentOptions.cs)
  - property string SectionName
  - property int MaxIterations
  - property double CompactThreshold
  - property int CompactKeepLast


---

## ModelsResponse
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface ModelsResponse
```


ModelsResponse is a compact data-transfer interface that represents the payload returned by the models API. It bundles two pieces of information: availableModels, an array of ModelDto representing the models that can be used, and selected, a SelectedModelDto describing the model currently chosen. Use it when consuming the endpoint that returns both the available models and the current selection in a single payload.

## Remarks

Front-end code can rely on this shape to render a model picker without issuing separate requests. It also mirrors the server contract: ModelDto definitions live in the same contract family as SelectedModelDto, ensuring consistent data semantics across the API layer. If either availableModels or selected changes, the consuming code can react accordingly thanks to TypeScript's type-checking.

## Example

```typescript
function summarize(res: ModelsResponse) {
  const total = res.availableModels.length;
  const current = res.selected;
  return { total, current };
}
```

## Notes
- The selected property is expected to be present per this signature; if your backend can omit or nullify it, guard against undefined/null during integration tests and add defensive checks at the call site.

---

## SelectedModelDto
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SelectedModelDto
```


The SelectedModelDto interface defines a minimal data transfer object used to identify a user-selected model. It exposes two required string properties: provider, which identifies the model provider, and name, which identifies the specific model within that provider. This shape is intended for API boundaries and UI interactions, allowing the system to reference a chosen model without exposing richer domain objects.

## Remarks
SelectedModelDto serves as a stable transport contract that decouples the API surface from internal domain representations. By carrying only provider and name, it accommodates multi-provider scenarios and enables straightforward JSON serialization across system boundaries. Requiring both fields ensures a precise, unambiguous reference to the selected model during selection flows and subsequent usage.

---

## SetActiveModelRequest
> **File:** `src/webapp/src/api/models.ts`  
> **Kind:** interface

```typescript
export interface SetActiveModelRequest
```


Represents the request payload used to set the active model for a given provider via the web API. It specifies two nullable fields, provider and name, which identify the provider and the model to activate. When constructing a request, provide the provider identifier and the model name you want to activate; both fields are typed as string | null to allow explicit omission or clearing.

## Remarks
This interface acts as a contract between the client and server for the active-model selection, centralizing the payload structure and enabling type-safe usage across the web application's API surfaces. By expressing both provider and model as nullable strings, it accommodates scenarios where the provider or model is intentionally unspecified or cleared, without introducing undefined values into the payload.

## Notes
- Nullable fields: both provider and name are string | null; undefined is not a permitted value for these fields in this shape.
- Serialization considerations: when sending as JSON, ensure the fields are included with null if you intend to clear them; omitting a field may be interpreted as "not provided" depending on the API.

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


Fetches the list of models from the backend at /api/models, including credentials such as cookies. It accepts an optional AbortSignal to cancel the request. The actual HTTP call runs through withRefresh to ensure an authenticated context before performing the fetch. If the response has a non-ok status, it throws an Error that includes the HTTP status and status text. On success, the JSON payload is parsed and returned as a ModelsResponse.

## Remarks
This function centralizes the retrieval of model definitions and provides a consistent error-handling and cancellation pattern for callers. By wrapping the raw fetch with withRefresh, it relies on a shared mechanism that maintains or refreshes authentication as needed, so UI layers can request models without duplicating boilerplate.

## Example
```typescript
async function demo() {
  const controller = new AbortController();
  try {
    const models = await fetchModels(controller.signal);
    console.log('Fetched models:', models);
  } catch (err) {
    if ((err as any).name === 'AbortError') {
      console.log('Fetch models aborted');
    } else {
      console.error('Failed to fetch models', err);
    }
  }
}
```

## Notes
- The function expects the server to respond with a payload that conforms to the ModelsResponse type; if the payload shape changes, the runtime may not match the declared type.
- The request uses credentials: 'include', so cookies are sent with the request; ensure server CORS and authentication expectations align with this.

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


setActiveModel updates the server's active model by issuing a PUT to /api/models/active with the given SetActiveModelRequest, returning a ModelsResponse when the operation succeeds. It uses a withRefresh wrapper to refresh authentication if needed and honors an optional AbortSignal for cancellation; on non-success, it throws a descriptive error including the status and server text.

## Remarks
setActiveModel abstracts the server-side mutation of the active model behind a typed request, shielding callers from the exact HTTP details and endpoint. It relies on withRefresh to transparently recover from authentication gaps, so call sites need only provide the desired model change. By returning a typed ModelsResponse, it gives consuming code a predictable shape for updating UI state or triggering downstream workflows.

## Notes
- This function uses credentials: 'include'; if your API is hosted on a different origin, ensure CORS allows credentials and that cookies are sent.
- On error, it throws a generic Error with a message including the HTTP status and response text; callers may want to catch and inspect the message or wrap it for richer error handling.

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


WithRefresh is a higher-order helper that wraps an authentication-sensitive fetch-like operation to automatically handle an expired session. You pass it a doFetch function that returns a `Promise<Response>`, and it executes the request. If the server replies with 401 Unauthorized, it attempts to refresh the session via refreshSession and, if the refresh succeeds, retries the original request once. If the retried response still indicates 401, it signals that the session has expired and throws an error prompting the user to sign in again. Use it to unify retry-on-unauthorized behavior across API calls without duplicating refresh logic at every call site.

## Remarks
This abstraction centralizes the common pattern of exchanging an expired token for a refreshed session. It hides the retry mechanics behind a single function, so callers simply provide how to perform the request. It relies on standard HTTP 401 semantics and a boolean result from refreshSession; on success it retries, on failure it signals expiration.

## Example
```ts
async function loadProfile() {
  const res = await withRefresh(() => fetch('/api/profile'));
  if (!res.ok) throw new Error(`Request failed: ${res.status}`);
  return res.json();
}
```

## Notes
- The function retries at most once after a successful refresh. If the second attempt still returns 401, it throws.
- If refreshSession() indicates failure or returns false, no retry occurs and the original 401 is propagated.
- If doFetch has side effects, those effects may be repeated on the retry; ensure the operation is idempotent or safe to retry.


---