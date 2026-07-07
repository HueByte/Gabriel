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

```typescript
export function formatError(e: unknown, fallback = 'Something went wrong.'): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `unknown` | — |
| `fallback` | — | `'Something went wrong.'` |

**Returns:** `string`


Formats a given error into a user-friendly string. It accepts any value as the first argument and a fallback message as the second; it returns a string representation suitable for display in the UI or logs. The function applies specialized formatting for ApiError by preferring body.detail, then body.title, and finally the error message; for standard Error instances it returns the error's message; and for any other input it falls back to the provided fallback.

## Remarks
Acts as a small normalization layer for error handling. By centralizing the logic of how ApiError shapes are surfaced, it prevents leaking internal error structures and keeps error presentation consistent across the app. It relies on ApiError having a body with optional detail and title fields, and gracefully degrades to the error message or a fallback when those fields are absent.

## Notes
- Non-Error inputs that are not ApiError will not yield their own message; the function falls back to the provided default in those cases.
- The default fallback message is configurable via the second parameter; supply a localized string if needed.
- When ApiError.body.detail or body.title are present, formatError uses those values in preference to e.message.

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


notifyError is a concise wrapper that accepts an error value (unknown) and an optional fallback string, formats the error with formatError(e, fallback), and displays it via toast.error. Developers would reach for it when they need to surface a runtime error to users as a standardized toast notification, optionally providing a fallback message if the error lacks a user-friendly description. By funneling errors through a single path (formatError -> toast.error), it provides a consistent error presentation and reduces duplication across the codebase.

## Remarks
It acts as a centralized UX layer for error notifications: all error toasts pass through this function, enabling consistent styling and making it easier to adjust how errors are formatted and shown in one place.

---

## notifyInfo
> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

```typescript
export const notifyInfo = (msg: string) => toast.info(msg)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `msg` | `string` | — |


notifyInfo is a small wrapper around the toast.info call that displays an informational toast with the provided message. Use it when you want to surface a non-critical, user-facing info notification in a consistent way without sprinkling toast.info calls all over the codebase.

## Remarks
Centralizes informational notifications behind a stable API, making it easier to swap the underlying notification library or apply consistent defaults later (such as autoClose duration or position). It communicates intent clearly at the call site, differentiating info toasts from success or error toasts. This can simplify testing by allowing a mock or stub for info toasts and reduces boilerplate in call sites.

## Notes
- Ensure a toast container is mounted in the app; otherwise calls to notifyInfo will not render anything.
- If you plan to change defaults (e.g., duration, position) across all info toasts, prefer updating this wrapper rather than individual call sites.

---

## notifySuccess
> **File:** `src/webapp/src/lib/notify.ts`  
> **Kind:** function

```typescript
export const notifySuccess = (msg: string) => toast.success(msg)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `msg` | `string` | — |


notifySuccess is a small, dedicated wrapper around toast.success that displays a success message to the user. Use it when you want a consistent, semantic way to show success feedback instead of calling toast.success(msg) at every call site.

## Remarks
This wrapper centralizes success notification concerns, enabling future changes to the underlying notification mechanism without touching call sites. By providing a stable API surface, it helps maintain UX consistency and simplifies testing since you can mock or stub this single function. It also makes it easier to apply global defaults (such as auto-close duration or position) from a single place if you later refactor the library usage. The wrapper exists to decouple business logic from presentation details and to provide a single point of change for success-toasts.

## Example
```typescript
notifySuccess('Data saved successfully');
```

## Notes
- Ensure the toast container (or equivalent) is rendered in the app; otherwise, the toast will not appear.
- This function forwards the message to the underlying library unchanged. For localization or message normalization, consider translating before calling this wrapper or wrapping it with your i18n layer.

---