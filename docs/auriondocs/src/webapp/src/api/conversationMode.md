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

A narrow string-union type that enumerates the supported conversational behavior modes for Gabriel-powered conversations. Use GabrielMode when you need compile-time validation of one of the predefined persona/behavior labels (for example, as a parameter, prop, or field) so callers cannot pass arbitrary strings.

## Remarks
This type encodes the limited set of conversation "modes" the application recognizes (chatty, elaborative, concise, tutor, critic). It exists as a TypeScript union rather than an enum to keep the runtime representation as plain strings while providing type safety at compile time. The chosen mode typically controls response length, tone, and instructional style in downstream code that implements the conversation behavior.

## Example
```typescript
function startConversation(mode: GabrielMode) {
  // runtime code reads `mode` to configure the assistant's behavior
  console.log(`Starting conversation in mode: ${mode}`);
}

const mode: GabrielMode = 'tutor';
startConversation(mode);
```

## Notes
- The values are case-sensitive and must match any expected server/API values exactly.
- This is a compile-time type only; at runtime values are plain strings (not an enum object).
- Adding or removing modes requires updating all consumers and any backend components that understand these mode labels.

---

## setConversationMode

> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

Set the server-side mode for a conversation by sending a PUT request to /api/conversations/:id/mode. Call this when you need to change or clear the GabrielMode associated with a conversation; this helper wraps the request with withRefresh (so authentication refresh behavior is applied), encodes the conversationId, and sends the mode as JSON in the request body.

## Remarks
This function delegates the actual network call to withRefresh (typically used to transparently handle auth token refresh and retry). It sends credentials ('include'), so cookies or other same-origin credentials will be transmitted. Passing null for mode will clear the mode on the server.

## Example
```typescript
const controller = new AbortController();
try {
  await setConversationMode('conversation-123', 'assistant', controller.signal);
  // mode set successfully
} catch (err) {
  // handle network or server error
  console.error('Failed to set mode', err);
}
// To cancel the request:
controller.abort();
```

## Notes
- On non-2xx responses the function throws an Error whose message includes the HTTP status and any response text returned by the server.
- The function uses encodeURIComponent on conversationId; ensure you pass the raw id (it will be escaped for the URL).
- The optional AbortSignal is forwarded to fetch so callers can cancel the request.

---

## withRefresh

> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

Attempts a single automatic session refresh when an HTTP 401 (Unauthorized) is encountered, then retries the provided fetch function once. Use this wrapper around HTTP calls that may fail due to an expired session so the module can try to refresh credentials and only surface an error if the session truly cannot be recovered.

## Remarks
withRefresh centralizes the common pattern of detecting a 401 response, invoking a session refresh flow (via refreshSession), and retrying the request one time. If the retry still returns 401 the function signals a terminal session expiry (via signalSessionExpired) and throws an Error to force calling code to handle sign-in flow. It does not mutate the original request function; callers provide the fetch logic as a thunk so it can be re-invoked after a refresh.

## Example
```typescript
// Typical usage with the global fetch API
try {
  const response = await withRefresh(() => fetch('/api/messages', { method: 'GET' }));
  if (!response.ok) {
    // handle other non-401 failures
  }
  const data = await response.json();
  // use data
} catch (err) {
  // If the session could not be refreshed, withRefresh throws an Error
  // (and signalSessionExpired() has been called). Handle sign-in flow here.
}
```

## Notes
- The function only retries once after a successful refresh attempt; it does not perform multiple retries.
- Errors thrown by doFetch or refreshSession are not caught here and will propagate to the caller.
- This implementation does not de-duplicate concurrent refresh attempts; multiple parallel calls may each attempt refreshSession.

---