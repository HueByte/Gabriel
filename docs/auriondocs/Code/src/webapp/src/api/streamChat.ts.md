# streamChat.ts

> **Source:** `src/webapp/src/api/streamChat.ts`

## Contents

- [StreamChatOptions](#streamchatoptions)
- [AgentEvent](#agentevent)
- [doFetch](#dofetch)
- [streamChat](#streamchat)
- [streamRegenerate](#streamregenerate)
- [streamSse](#streamsse)

---

## StreamChatOptions

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** interface

A small options bag for stream-based chat requests that allows the caller to provide an AbortSignal to cancel the underlying request or stream. Use this when you need to be able to abort or time out an in-progress streaming chat operation from the caller side.

## Remarks
StreamChatOptions is intentionally minimal: it exists to surface cancellation control (AbortSignal) to the streaming chat API without coupling the API to a specific cancellation implementation. Callers typically create an AbortController and pass its signal here so the stream can be terminated early.

## Example
```typescript
// create a controller and start the streaming chat request
const controller = new AbortController();
const options: StreamChatOptions = { signal: controller.signal };

// start streaming (pseudo call — replace with actual API function)
const stream = await streamChat(prompt, options);

// later, cancel the stream if needed
controller.abort();
```

## Notes
- Aborting the provided signal will typically stop the underlying fetch/stream; ensure any cleanup logic on the consumer side handles partial data.
- Do not reuse the same AbortSignal across unrelated requests if you need independent cancellation; aborting it cancels all consumers sharing that signal.
- AbortSignal is a Web API (also available in modern Node.js). In environments without AbortController/AbortSignal, provide a compatible polyfill or guard usage.

---

## AgentEvent

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** type

Represents events emitted by the server during a streamed chat turn. Consumers of the chat stream should inspect AgentEvent values to react to lifecycle milestones; in particular the 'userMessagePersisted' variant is emitted as the first event of every send-driven turn and carries the real database id for the just-persisted user message so a client can replace its optimistic local id without performing a follow-up GET.

## Remarks
This union exists to make server-driven lifecycle signals explicit to stream consumers. By sending a dedicated 'userMessagePersisted' event at the start of a send-driven turn the server enables clients to reconcile optimistic UI state (temporary message ids) with canonical ids immediately, reducing round-trips and avoiding extra GETs. It is intentionally emitted only for send-driven turns (not for regenerate flows), so clients should not rely on this event during regeneration.

## Example
```typescript
function handleAgentEvent(event: AgentEvent) {
  if (event.type === 'userMessagePersisted') {
    // The event includes the persisted message's DB id; use that value to
    // replace the client's optimistic id without issuing another GET.
    // Access the concrete id field here according to the server's payload.
  }
}
```

## Notes
- Emitted only for send-driven turns (not for regenerate); make no assumptions that it will appear during a regenerate flow.
- Because this event is sent first for a send-driven turn, handle it before processing later events in the same turn to avoid temporary-id mismatches.

---

## doFetch

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

Sends a POST request to the given URL, optionally JSON-encoding the provided body, including credentials (cookies), and requesting an SSE-compatible response via the Accept header. Use this when initiating server-sent event streams or other POST-based streaming endpoints where cookies must be sent even if the API and webapp might end up on different origins.

## Remarks
This helper centralizes the fetch configuration required for streaming endpoints: it forces method to POST, sets credentials: 'include' so browser cookies travel with the request (avoiding stale-cookie 401s in cross-origin deployments), and sets Accept: 'text/event-stream' so the server can return an SSE stream. It only attaches a Content-Type header and JSON-encodes the body when the caller passes a non-null body value; callers should pass null to indicate an intentional no-body request.

## Example
```typescript
const controller = new AbortController();

// Start a streaming POST with a JSON body
doFetch('/api/stream', { prompt: 'Hello' }, controller.signal)
  .then(res => {
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    // Read streamed bytes from res.body (ReadableStream) or handle SSE server-side framing
    const reader = res.body?.getReader();
    // ...consume the stream
  })
  .catch(err => {
    if (err.name === 'AbortError') console.log('Fetch aborted');
    else console.error(err);
  });

// Omit body entirely (regen/no-body) – pass null to avoid sending Content-Type
doFetch('/api/stream', null);
```

## Notes
- Pass null to indicate "no body"; the function uses a strict null check so other falsy values (e.g. undefined) will cause a Content-Type header to be sent even if JSON.stringify results in undefined.
- JSON.stringify may throw for objects with circular references; handle or sanitize input before calling.
- The function always uses POST; it is not suitable for GET requests.
- Because credentials are included, cookies will be sent to the target origin—ensure this is intended and secure for your deployment.


---

## streamChat

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
export function streamChat(
  conversationId: string,
  content: string,
  opts: StreamChatOptions =
```


Sends the provided content for the conversation identified by conversationId and allows the caller to supply an AbortSignal to cancel the operation. Reach for streamChat when you need to initiate a chat-related request scoped to a specific conversation and optionally make that request abortable from the caller.

## Remarks
This API surface centralizes chat requests that are tied to a conversation id. The only explicit option exposed is an AbortSignal (via StreamChatOptions.signal), so callers are expected to manage cancellation and timeouts using AbortController.

## Example
```typescript
// Start a chat request and cancel it after 5 seconds
const controller = new AbortController();
streamChat("conversation-123", "Hello, are you there?", { signal: controller.signal });
setTimeout(() => controller.abort(), 5000);
```

## Notes
- Passing an already-aborted signal will typically cause an immediate cancellation; create a fresh AbortController per logical operation.
- Aborting the signal requests cancellation on the client side; the server may still observe or finish processing the request depending on how the underlying implementation handles AbortSignal.


---

## streamRegenerate

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

Requests regeneration of a specific message (messageId) within a conversation (conversationId). Use this when you need to re-run or refresh the content for an existing message—typically to ask the chat backend to recreate or update a previously generated assistant message. An optional StreamChatOptions object can carry an AbortSignal to cancel the operation.

## Remarks
This helper centralizes the client-side initiation of a "regenerate" operation for a Stream-based chat flow and exposes cancellation via AbortSignal. It is intended to be used alongside other stream* helpers in the same module so callers can follow the same consumption and error-handling patterns established by the surrounding API utilities.

## Example
```typescript
// Start a regeneration and allow cancelling it via AbortController
const controller = new AbortController();
const opts = { signal: controller.signal };
// Call the helper (how the result is consumed depends on this project's streaming conventions)
streamRegenerate('conversation-123', 'message-456', opts);
// Cancel if needed
controller.abort();
```

## Notes
- Ensure the messageId belongs to the provided conversationId; mismatched IDs will likely result in an error from the server.
- Provide an AbortSignal when you need the ability to cancel network activity; aborting cancels the client-side request but may not undo any server-side changes already committed.
- The concrete shape of the returned/streamed data and how to consume it is defined by this module's streaming conventions; consult the surrounding stream* helpers or implementation for exact consumption patterns.

---

## streamSse

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

Creates an async generator that opens a Server‑Sent Events (SSE) stream to the given URL and yields each message or chunk produced by the server as it arrives. Reach for this when you need to consume a streaming HTTP response incrementally (for example, processing partial results from a chat/model endpoint) instead of waiting for a single completed response.

## Remarks
This function provides a low‑level streaming primitive: it hides the mechanics of establishing and reading an SSE connection and exposes an async iterable to the caller. Higher‑level utilities can be built on top of it to aggregate events into messages, handle reconnection logic, or transform streamed payloads.

## Example
```typescript
// Consume the stream incrementally
for await (const event of streamSse(endpointUrl, { prompt: 'Hello' })) {
  // `event` is a server-provided chunk/message — handle or accumulate as needed
  console.log('received event:', event);
}
```

## Notes
- The stream does not start until the returned async generator is consumed (e.g. with `for await...of` or by calling `.next()`).
- Network or parsing errors that occur while reading the stream will be thrown from the iterator; wrap iteration in try/catch to handle failures and do any cleanup.
- Each yielded value corresponds to one server-sent message/chunk — do not assume they represent complete logical messages unless the server guarantees that boundary.

---