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

```typescript
export interface StreamChatOptions
```


StreamChatOptions defines a small, cancellable configuration contract for stream chat operations. It currently exposes an optional signal property (AbortSignal) that enables callers to cancel in-flight operations via an AbortController.

## Remarks
By encapsulating cancellation behind this interface, the API surface remains focused on chat semantics while reusing standard browser and Node cancellation patterns. Implementations should listen to the AbortSignal and terminate any ongoing work promptly, then release resources. This approach also makes cancellation composable with other APIs that accept AbortSignal, allowing a single controller to govern multiple related operations.

## Example
```typescript
const controller = new AbortController();
const options: StreamChatOptions = { signal: controller.signal };

// Example usage: a chat API that accepts StreamChatOptions
await startStreamChatConnection(options);

controller.abort();
```

## Notes
- Some API calls may ignore the signal; always verify the specific API supports cancellation.
- Aborting usually raises a cancellation error; handle AbortError and perform any necessary cleanup.
- If signal is omitted, the operation will run to completion unless another cancellation mechanism is triggered.

---

## AgentEvent
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** type alias

```typescript
export type AgentEvent =
  
  
  
  
  |
```


AgentEvent is a TypeScript type alias that models the events emitted by the streaming chat API during a send-driven turn. It is a discriminated union where the first and primary variant signals the persistence of the user's message and provides the real database identifier, enabling the client to reconcile its optimistic UI state with the server state without an extra fetch.

## Remarks
AgentEvent serves as the outbound contract for streaming updates related to a user’s message lifecycle. It isolates the moment when the server has committed a user message from other progress updates, allowing the UI to replace temporary identifiers (like tmp-xxxxx) with the server-assigned real IDs in a single, deterministic step. This avoids an unnecessary follow-up GET of the conversation and helps keep the client state in sync with the backend as messages are processed.

## Example
```typescript
function handleEvent(e: AgentEvent) {
  if (e.type === 'userMessagePersisted') {
    // Reconcile the client's optimistic message entry with the server-provided real ID
    // (exact field names depend on the concrete payload shape)
  }
}
```

## Notes
- The exact payload shape carried by userMessagePersisted (beyond carrying the real DB id) is not fully visible in the snippet; inspect the full type definition to rely on the correct property names.
- Since AgentEvent is a discriminated union, handle all variants explicitly (or provide a safe default) to avoid runtime surprises when new events are added.
- This event marks the first signal in a send-driven turn; design UI state updates to wait for this acknowledgement before assuming server-side persistence guarantees for the new message.

---

## doFetch
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
function doFetch(url: string, body: unknown, signal?: AbortSignal): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `url` | `string` | — |
| `body` | `unknown` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<Response>`


doFetch is a small, opinionated wrapper around fetch that performs a POST with an optional JSON body while reliably carrying cookies across origins and requesting an SSE stream. Use it when you need to start a server-sent events workflow that relies on cookie-based authentication and a JSON payload, rather than duplicating the boilerplate headers and body handling every time.

## Remarks
It centralizes the cross-origin cookie handshake and the streaming-oriented headers, so callers don't have to repeat credentials and Content-Type logic. It isolates the decision to use 'application/json' only when a body is provided and to request text/event-stream, making future changes (e.g., switching to a different payload strategy) easier to apply in one place. In the bigger picture, it complements other API helpers by standardizing the startup of streaming requests that need authentication cookies.

## Notes
- Not a stream parser: the function returns a Response; you must read the body yourself (e.g., via resp.body.getReader() or resp.text()) to handle the SSE.
- Ensure body is JSON-serializable; non-serializable values may cause JSON.stringify to throw.
- Cross-origin usage requires server-side support for credentials (Access-Control-Allow-Credentials) and the appropriate origin; otherwise the request may fail with an auth error.

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


Streams a chat message into the specified conversation by delivering the content progressively as it becomes available. Use this when you want real-time feedback or long-form messages to render incrementally, rather than waiting for the complete payload before showing anything to the user.

## Remarks
streamChat serves as a higher-level abstraction over the underlying streaming transport used by the web app. It encapsulates the interaction with the chat backend behind a stable API, allowing UI components to render as content arrives. The optional StreamChatOptions parameter configures streaming behavior (for example, chunking strategy or callbacks) without leaking transport details to callers. This separation helps keep the chat rendering responsive and consistent across different backends.

## Notes
- Streaming implies partial, ordered delivery; don't assume the entire message is available in a single payload.
- Handle mid-stream errors and cancellation gracefully to avoid leaving the UI in an inconsistent state.

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


Initiates a regeneration of the streaming reply for a specific message within a conversation, allowing a fresh stream to be produced for that message. Provide the conversationId, the target messageId, and optional StreamChatOptions to tailor the streaming session; use this when you want to refresh or redo a previously streamed reply without altering the original message structure.

## Remarks
streamRegenerate encapsulates the regeneration workflow behind a simple API boundary. It enables UI layers to request a new stream without implementing back-end regeneration logic themselves, and it relies on the StreamChatOptions type to influence the streaming behavior without exposing internal details.

## Example
```typescript
streamRegenerate("conversation-42", "message-7", { /* options */ });
```


---

## streamSse
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
async function* streamSse(
  url: string,
  body: unknown,
  opts: StreamChatOptions =
```


streamSse is an async generator that streams server-sent events from the given URL by sending the provided payload. It yields each event as it arrives, enabling consumers to process a live stream of messages or updates (for example, real-time chat) rather than awaiting a single response.

## Remarks
Encapsulates the SSE transport lifecycle and keeps transport concerns separate from business logic. By taking a StreamChatOptions, it centralizes streaming configuration (headers, credentials, and other transport-level settings) so higher-level chat code can subscribe to events without managing the low-level fetch/stream boilerplate.

## Example
```typescript
// Example usage: consume a live stream of events
for await (const event of streamSse('https://api.example.com/stream', { roomId: 'room-123' }, { headers: { Authorization: 'Bearer token' } })) {
  console.log('received event:', event);
}
```

## Notes
- Cancellation: break the loop to stop streaming and release resources.
- Error handling: wrap in try/catch to handle errors emitted by the async iterator.

---