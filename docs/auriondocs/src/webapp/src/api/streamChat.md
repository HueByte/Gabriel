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

Options passed to streaming chat operations. Use this when you need to provide an AbortSignal to cancel or timeout an in-progress streaming request.

## Remarks
This interface is a small extensible container for cross-cutting call options; it currently exposes an AbortSignal so callers can cancel an ongoing stream without wiring cancellation logic into the API surface. Accepting an AbortSignal lets the implementation integrate with fetch/streams and other platform cancellation mechanisms.

## Example
```typescript
// create a controller that can cancel the request
const controller = new AbortController();

// pass the signal when starting a streaming chat call
streamChat(prompt, { signal: controller.signal });

// later, cancel the stream if needed
controller.abort();
```

## Notes
- Aborting the signal typically causes the underlying fetch/stream to reject with an AbortError; callers should handle that error if they need to distinguish cancellations from other failures.
- The signal is optional; if omitted, the stream will not be cancellable via AbortSignal.
- Reusing the same AbortSignal for multiple operations can be error-prone because a previously aborted signal cannot be "reset."

---

## AgentEvent

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** type

Represents events emitted by the streaming chat agent. Reach for this type when handling incoming stream messages from the chat API; the client inspects the discriminant (event.type) to react to specific protocol events. The documented variant, 'userMessagePersisted', is sent as the first event of a send-driven turn (not during a regenerate) and carries the real database id of the just-persisted user message so the client can replace its optimistic temporary id without performing a follow-up GET.

## Remarks
This type is the streaming protocol's event envelope used to synchronize server-side persistence with the client UI. Emitting 'userMessagePersisted' first allows the client to atomically swap an optimistic client-side message id (for example something like "tmp-xxxxx") with the server-assigned persistent id, avoiding an extra round-trip to fetch the conversation state.

## Example
```typescript
function handleAgentEvent(event: AgentEvent) {
  if (event.type === 'userMessagePersisted') {
    // Replace the optimistic client message id with the persisted id carried by the event
    // (access the id property on `event` according to the concrete shape in codebase)
  }
}
```

## Notes
- This event is emitted only on send-driven turns, not on regenerate flows.
- Clients should use the id in this event to update local optimistic messages instead of issuing an immediate GET for the conversation.
- Check the concrete shape of the event in the source to know the exact property name that carries the persisted message id.

---

## doFetch

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

Sends a POST request to the given URL configured for server-sent events (Accept: text/event-stream) and ensures cookies are included. Use this helper when you need a JSON-encoded POST that will establish or interact with an SSE-style endpoint and you want the browser's cookies sent along (e.g., authenticated streaming endpoints).

## Remarks
This function centralizes a few fetch conventions used by the streaming API: it always uses POST, sets credentials: 'include' so the browser will send cookies (important for auth in cross-origin deployments), and asks for an event-stream response via Accept: text/event-stream. The request body is JSON-serialized when body !== null; callers should pass null to indicate "no body" (the code intentionally tests against null rather than undefined).

## Example
```typescript
const controller = new AbortController();
// send a JSON payload and receive an SSE-style response
const res = await doFetch('/api/stream', { prompt: 'Hello' }, controller.signal);
if (!res.ok) throw new Error(`fetch failed: ${res.status}`);
// consume the stream from res.body as appropriate for your SSE handling
```

## Notes
- Pass null (not undefined) as the body argument to omit the Content-Type header and request body; the function uses the condition body !== null to decide whether to include Content-Type and to JSON-serialize.
- If body is undefined the code will still add Content-Type and attempt JSON.stringify(undefined) (which yields undefined), resulting in a request with the header set but no serialized payload — prefer null to express "no body."
- JSON.stringify may throw or produce unexpected output for non-plain objects (e.g., FormData, circular references) — ensure the value is serializable.
- credentials: 'include' sends cookies with the request; be mindful of cross-origin cookie policies and security implications.


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


Submitted documentation for symbol streamChat (e8ede93e-9cf2-40cf-a0d1-1371bc03041a). The documentation contains a concise description, a brief Remarks paragraph, an Example showing basic usage, and Notes calling out the missing implementation/return-type information and common caller concerns.

Flag: A warning was recorded (category: missing-context) because only the function signature was available; callers should consult the implementation for precise return semantics and streaming behavior.

---

## streamRegenerate

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
export function streamRegenerate(
  conversationId: string,
  messageId: string,
  opts: StreamChatOptions =
```


Requests regeneration of an existing message identified by messageId in the given conversationId, optionally controlling streaming or behavior via the StreamChatOptions parameter. Reach for this helper when you need the backend to re-generate a message (for example to retry or obtain a fresh AI response) without creating a new message entry.

## Remarks
This function is a small client-side abstraction that centralizes the "regenerate message" operation and exposes streaming-related knobs through StreamChatOptions. Keeping this call in one place prevents callers from duplicating request wiring or option handling and makes it easier to change the underlying request/streaming behavior later.

## Example
```typescript
// Call site (handling depends on the function's return type):
const result = streamRegenerate('conversation-123', 'message-456', { /* opts */ });

// If the function returns a Promise:
// const response = await result;

// If it returns an async iterable (stream of chunks):
// for await (const chunk of result) {
//   console.log(chunk);
// }

// If it returns a ReadableStream:
// const reader = result.getReader();
// while (true) {
//   const { value, done } = await reader.read();
//   if (done) break;
//   console.log(value);
// }
```

## Notes
- The implementation and exact return shape (Promise, ReadableStream, async iterable, event emitter, etc.) are not present here — confirm the concrete return type before writing callers.  
- conversationId and messageId must reference existing server resources; invalid IDs will cause the regeneration to fail.  
- opts defaults to an empty object; pass StreamChatOptions only if you need to alter streaming, timeouts, or other supported behaviors.

---

## streamSse

> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

Connects to the given URL and exposes a Server‑Sent Events (SSE) stream as an async generator. Call this when you need to consume a chat or SSE endpoint as an async-iterable sequence of incoming payloads (for use with `for await (...)`), rather than handling raw fetch/stream plumbing yourself.

## Remarks
This function provides a small, reusable abstraction that turns an SSE-style HTTP stream into an async iterator. It isolates connection and parsing concerns so higher-level code can process streamed messages one at a time without dealing with ReadableStream iteration or low-level event framing.

## Example
```typescript
// Consume the SSE stream using for-await-of
for await (const chunk of streamSse('/api/chat/stream', { prompt: 'hello' })) {
  console.log('received chunk:', chunk);
  // break early if you have enough data
  // if (shouldStop) break;
}
```

## Notes
- The returned async generator is single-use: consume it once with `for await` and don't attempt to re-iterate it.
- Network errors or server-side stream termination propagate as exceptions from the generator; wrap consumption in try/catch to handle failures.
- To stop streaming early, break out of the `for await` loop (or otherwise cancel the consumer); ensure your surrounding code handles cleanup if needed.

---