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

Returns a short, human-readable message extracted from an unknown error value; reach for this when you need a consistent string to display in the UI or in logs instead of inspecting different error shapes manually. It prefers ApiError.body.detail, then ApiError.body.title, then the standard Error.message, and finally the provided fallback string when the value isn't an Error.

## Remarks
This helper centralizes the logic for normalizing error-like values into a single display string so callers don't repeat extraction logic across the codebase. It is designed to handle a custom ApiError shape (an error with a body object containing detail/title) while still falling back to plain Error behavior and a safe fallback for non-error values.

## Example
```typescript
// Minimal mock of the runtime shape used by formatError
class ApiError extends Error {
  constructor(public body?: { detail?: string; title?: string }, message?: string) {
    super(message);
  }
}

console.log(formatError(new ApiError({ detail: 'Invalid input' })));
// -> 'Invalid input'

console.log(formatError(new ApiError({ title: 'Bad request' }, 'request failed')));
// -> 'Bad request'

console.log(formatError(new Error('Something exploded')));
// -> 'Something exploded'

console.log(formatError(null, 'Unknown error'));
// -> 'Unknown error'
```

## Notes
- The extraction order is: body.detail, body.title, then Error.message; if none exist the fallback parameter is returned.  
- If an ApiError has a non-object body (or different shape), detail/title access will yield undefined and the function will fall back to other fields — it does not validate or coerce the body shape.  
- Returned strings are not sanitized or localized by this helper; perform any escaping or i18n outside of formatError if needed.

---

## notifyError

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

```typescript
export const notifyError = (e: unknown, fallback?: string) => toast.error(formatError(e, fallback))
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `unknown` | — |
| `fallback` | `string` | — |


Show a user-facing error toast for an exception or any error-like value. The function formats the provided `unknown` value using `formatError` (forwarding the optional `fallback` string) and immediately passes the resulting message to `toast.error`. Use this helper when you want a consistent, one-line way to surface errors to the user without repeating formatting and toast code.

## Remarks
Centralizes the pattern of converting arbitrary error values into a user-visible toast notification. By delegating message construction to `formatError` and delivery to `toast.error`, this wrapper keeps calling code simple and ensures all error toasts are produced in a consistent manner.

## Example
```typescript
// Show a toast for a caught exception
try {
  await doSomethingRisky();
} catch (err) {
  notifyError(err);
}

// Provide a fallback message when formatting the error
notifyError(someValue, 'An unexpected error occurred. Please try again.');
```

## Notes
- This is side-effecting: it displays a toast immediately via the configured toast system (e.g., requires a toast provider/container in the app).
- The function returns whatever `toast.error` returns (typically a toast identifier from the toast library used).
- `e` is typed as `unknown`; ensure `formatError` can handle the inputs your code may pass in.

---

## notifyInfo

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Displays a short informational toast to the user by delegating to toast.info. Reach for notifyInfo when you need to show non-critical, informational feedback (for example: confirmations like "Saved", status updates like "Sync complete", or simple hints). Using this wrapper keeps notification usage consistent and makes it easy to change behavior application-wide.

## Remarks
This function is a thin central wrapper over the underlying toast implementation. Centralizing calls here makes it easy to substitute a different toast library, add default options, or mock notifications in tests without changing call sites throughout the codebase.

## Example
```typescript
import { notifyInfo } from './lib/notify';

notifyInfo('Profile saved successfully');
```

## Notes
- A toast container/provider (e.g., <ToastContainer /> when using react-toastify) must be mounted in the app for notifications to appear.
- The parameter is typed as a string; to show richer content (React nodes or formatted markup) extend this wrapper or call the underlying toast API directly.
- This function delegates all behavior to the underlying toast library (appearance, autoClose, position, etc.), so those settings determine the final presentation.

---

## notifySuccess

> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

Displays a success-styled toast notification using the application's toast implementation. Call this helper to show brief confirmation or success feedback to the user (for example after saving or submitting data); it simply delegates to toast.success with the provided message.

## Remarks
This small wrapper centralizes success notifications so call sites remain concise and the underlying toast implementation or behavior can be changed in one place. It intentionally does not add extra side effects or formatting — its role is to provide a semantic, easily discoverable API for success toasts.

## Example
```typescript
import { notifySuccess } from './lib/notify';

notifySuccess('Profile updated successfully.');
```

## Notes
- Behavior, styling, and lifecycle (duration, position, dismissal) are determined by the configured toast library and its provider; ensure the toast provider is mounted in the app root.
- The function forwards whatever value toast.success returns (for example a toast id) — if your code depends on that return value it will be preserved.
- Messages are passed through as-is; perform localization or interpolation before calling if needed.

---