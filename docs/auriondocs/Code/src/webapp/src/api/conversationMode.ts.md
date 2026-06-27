# conversationMode.ts

> **Source:** `src/webapp/src/api/conversationMode.ts`

## Contents

- [GabrielMode](#gabrielmode)
- [setConversationMode](#setconversationmode)
- [withRefresh](#withrefresh)

---

## GabrielMode

> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** type

A string-union type that enumerates the allowed conversation modes for Gabriel. Use this type for function parameters, component props, or configuration objects when you want compile-time enforcement of the assistant's tone/verbosity preset rather than allowing arbitrary strings.

## Remarks
Centralizes the valid mode names in one place so callers and implementors can rely on a fixed set of presets (chatty, elaborative, concise, tutor, critic). Because this is a TypeScript-only union, it provides static type safety during development but no runtime validation; external input should be validated before being treated as a GabrielMode value.

## Example
```typescript
function setConversationMode(mode: GabrielMode) {
  switch (mode) {
    case 'chatty':
      // use a very talkative, informal style
      break;
    case 'elaborative':
      // provide detailed explanations
      break;
    case 'concise':
      // short, to-the-point responses
      break;
    case 'tutor':
      // guided, step-by-step teaching style
      break;
    case 'critic':
      // evaluative, critical feedback
      break;
  }
}

// Accepting user-supplied strings requires a runtime guard
function isGabrielMode(value: string): value is GabrielMode {
  return ['chatty', 'elaborative', 'concise', 'tutor', 'critic'].includes(value);
}
```

## Notes
- This type is compile-time only; use a runtime check (like the isGabrielMode example) when parsing external input.
- Adding or renaming modes requires updating all consuming code and any persisted configuration values.
- Prefer this union over plain strings to get autocompletion and exhaustiveness checking in switch statements.

---

## setConversationMode

> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

Sets the mode for a conversation by sending a PUT request to the server-side /api/conversations/:id/mode endpoint. Use this when the client needs to change or clear (pass null) the conversation's GabrielMode; the function performs the network request and throws if the server responds with a non-success status.

## Remarks
The HTTP request is executed through a withRefresh wrapper and uses fetch with credentials included and a JSON body of the form { mode }. The conversationId is encoded with encodeURIComponent before being placed in the URL. If the server responds with a non-OK status the function reads the response body (falling back to an empty string on read error) and throws an Error containing the HTTP status and any returned text.

## Example
```typescript
// set a mode
await setConversationMode('conversation-123', 'assistant');

// clear a mode with abort support
const controller = new AbortController();
setTimeout(() => controller.abort(), 5000);
try {
  await setConversationMode('conversation-123', null, controller.signal);
} catch (err) {
  // handle network error, non-2xx response, or abort
}
```

## Notes
- The function throws for non-OK responses; callers should catch errors to handle server-side validation or network failures.
- Passing null for mode will send { mode: null } — servers commonly interpret this as clearing the mode.
- Provide an AbortSignal to cancel the underlying fetch; the conversationId is URL-encoded so characters like slashes are safe.
- The request is sent with credentials: 'include', so cookies or other credentials will be included in the request.

---

## withRefresh

> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

Attempts the provided fetch-like operation and, if it returns a 401 Unauthorized, tries to refresh the session and retry the operation once. If the retry still yields a 401 the function calls signalSessionExpired(), throws an Error('Session expired. Please sign in again.'), and rejects; otherwise it returns the Response from the successful call. Use this wrapper when you want automatic, single-attempt session refresh for API calls that may return 401.

## Remarks
This helper centralizes the common pattern of: perform a request, refresh credentials on a 401, then retry the request once. It delegates the actual refresh logic and session-expiration handling to external functions (refreshSession and signalSessionExpired) so callers only provide the request action (doFetch). The function does not attempt multiple refresh retries and does not catch exceptions thrown by doFetch or refreshSession — such errors will propagate to the caller.

## Example
```typescript
// Typical usage with the Fetch API
const response = await withRefresh(() =>
  fetch('/api/user/profile', { method: 'GET', credentials: 'include' })
);
if (!response.ok) {
  // handle non-401 error statuses as usual
}

// Using withRefresh in a helper that adds headers
const response2 = await withRefresh(() =>
  fetch('/api/items', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
    credentials: 'include'
  })
);
```

## Notes
- The wrapper retries the provided doFetch at most once after a successful refresh; it will not perform repeated refresh attempts.
- If refreshSession() or doFetch() throws an exception (instead of returning a Response or boolean), that exception is not caught here and will propagate to the caller.
- The function only treats an HTTP 401 status as a trigger for refresh; other status codes (including 403) are not handled specially.

---