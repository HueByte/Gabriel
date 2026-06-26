# notify.ts

> **Source:** `src/webapp/src/lib/notify.ts`

## Contents

- [formatError](#formaterror)
- [notifyError](#notifyerror)
- [notifyInfo](#notifyinfo)
- [notifySuccess](#notifysuccess)

---

## formatError

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Convert an unknown thrown value into a user-facing string message. Prefer this helper when presenting errors to UI or logging messages intended for end users — it understands the application ApiError shape (prefers body.detail, then body.title, then the error message), falls back to a generic Error message, and finally returns a supplied fallback string for non-error values.

## Remarks
Centralizes the logic for extracting a readable message from different error shapes so callers don't duplicate the same instanceof checks and property access. The function expects an application-specific ApiError class that carries a body with optional detail/title fields; for all other Error instances it uses the standard Error.message. It intentionally does not perform logging, localization, or side effects — just string extraction.

## Example
```typescript
// minimal stand-in for the real ApiError used in your app
class ApiError extends Error {
  constructor(public body?: { detail?: string; title?: string }) {
    super(body?.title ?? 'api error');
  }
}

// Usage examples
const apiErrWithDetail = new ApiError({ detail: 'Invalid email address' });
console.log(formatError(apiErrWithDetail)); // 'Invalid email address'

const apiErrWithTitle = new ApiError({ title: 'Bad Request' });
console.log(formatError(apiErrWithTitle)); // 'Bad Request'

const runtimeErr = new Error('Something exploded');
console.log(formatError(runtimeErr)); // 'Something exploded'

console.log(formatError(42, 'Unknown failure')); // 'Unknown failure'
```

## Notes
- The function uses runtime instanceof checks; if an ApiError comes from a different JS realm (iframe, worker) the instanceof test may fail.
- The ApiError body is accessed via a type cast and may be undefined; the code falls through safely to message or the fallback.
- Default fallback is the literal string 'Something went wrong.' when no fallback is provided.

---

## notifyError

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Format an arbitrary error value and display it to the user using the application's toast error UI. Reach for this helper when you need a consistent, single-line call-site to show user-facing error messages (it delegates message extraction to formatError and the actual display to toast.error).

## Remarks
This small wrapper centralizes how errors are presented to users: callers pass any thrown value (unknown) and an optional fallback message, and notifyError ensures the application uses the same formatting and toast mechanism everywhere. That keeps components and services free of toast-specific details and promotes consistent messaging across the UI.

## Example
```typescript
try {
  await saveDocument(doc);
} catch (err) {
  // Show a user-facing toast with a formatted message; provide a fallback if needed
  notifyError(err, 'Failed to save document');
}
```

## Notes
- notifyError only affects user-facing UI; it does not perform logging or error propagation — log separately if you need diagnostics.
- The function relies on formatError to handle unknown values; ensure formatError is robust for the kinds of errors your code may surface.
- Avoid exposing sensitive or detailed internal error information in toasts; prefer user-friendly fallbacks for production messages.


---

## notifyInfo

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Shows a brief informational notification to the user by delegating to the application's toast/notification library.

This small helper wraps the underlying toast.info call so callers can show informational toasts with a single import and without depending directly on the toast API.

## Remarks
Centralizes how informational messages are displayed across the app so the implementation can be swapped or extended (for example, to add default options, logging, or telemetry) without changing call sites. It intentionally keeps the surface minimal — a single string message — because it's intended for simple, transient user feedback.

## Example
```typescript
import { notifyInfo } from './lib/notify';

notifyInfo('Settings saved successfully');
```

## Notes
- The function has side effects (it shows a UI toast) and does not return a value; callers should not rely on a result.
- Ensure the app's toast/notification system is initialized (e.g., rendering the required container/provider) before calling this helper, otherwise toasts may not appear.
- Use for informational messages only; use a different helper for warnings or errors if available so styling/semantics remain consistent.


---

## notifySuccess

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Displays a success toast using the application's toast implementation. Use this helper at call sites when you want to show a standard success notification without depending on the toast library directly.

## Remarks
This is a thin wrapper around the underlying toast.success call. It centralizes success-notification usage so callers remain decoupled from the toast library and so default behavior (timing, styling, position, etc.) can be changed in one place.

## Example
```typescript
import { notifySuccess } from './lib/notify';

async function saveItem() {
  await api.save(...);
  notifySuccess('Save completed');
}
```

## Notes
- The function forwards to the underlying toast implementation and returns whatever that call returns (an opaque handle, not a Promise).
- Ensure the toast system is mounted in the app (for example, render the Toast container/provider required by your toast library) or notifications will not appear.
- No validation is performed on the message; pass a non-empty string for best UX.

---