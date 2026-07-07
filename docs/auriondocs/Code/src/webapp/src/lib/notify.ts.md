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


formatError converts different error shapes into a human-friendly message. It first tries to extract a detailed message from an ApiError's body (detail or title), then falls back to the Error's message, and finally returns the supplied fallback for non-errors.

## Remarks
Centralizes error-to-string conversion used by UI code paths when displaying errors from API calls. The ApiError branch avoids exposing raw exception data and surfaces user-friendly details when available, while preserving the original message as a fallback. This function helps keep error messaging consistent across the web UI and reduces repetitive boilerplate in catch blocks.

## Notes
- If e is an ApiError, the function looks for body.detail, then body.title, and returns the first present; if neither exists, it returns e.message.
- If e is a plain Error, the function returns e.message.
- For any other input, it returns the provided fallback string (default: 'Something went wrong.').

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


notifyError is a small utility that formats any error value and displays it as an error toast. It accepts an unknown error and an optional fallback string, delegates the message creation to formatError, and then shows the result via toast.error.

## Remarks
By centralizing error formatting and presentation, notifyError reduces duplication and enforces a consistent user experience for errors across the UI. It decouples the business logic from presentation details, so changes to formatting or the toast library can be applied in one place. This wrapper also makes testing easier by providing a single, predictable path from error input to user notification.

## Notes
- If formatError returns an empty string for some inputs, the toast might render an empty message; provide a fallback or ensure formatError always yields meaningful text.
- This function is a UI-level concern and relies on toast and formatError being available in scope; ensure they are properly imported in the consuming module.

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


notifyInfo is a small helper that shows an informational toast with the provided message. It delegates to toast.info, providing a single entry point for info-level notifications so you can swap the underlying library or adjust defaults in one place without touching individual call sites.

## Remarks
It abstracts the toast.info call to centralize how informational messages are presented, promoting a consistent user experience. If you later replace the toast library or want to adjust presentation details (such as duration or position), you only need to modify this function.

## Example
```typescript
notifyInfo("Data saved successfully");
```

## Notes
- Make sure a ToastContainer (or equivalent) is mounted in your app; otherwise the toast won't render.
- The function returns the result of toast.info(msg); you can capture and use it to dismiss or update the toast if needed, otherwise ignore it.

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


NotifySuccess is a tiny wrapper around the toast library's success notification. It takes a message string and delegates to toast.success(msg) to display a standardized success toast to the user. Use this helper at call sites to keep success notifications consistent and to centralize any future changes to how success toasts are shown.

## Remarks
This abstraction centralizes usage of success toasts, making it easier to swap the toast implementation or apply consistent styling, duration, or behavior across the app. It also clarifies intent at call sites by naming the action as a success notification rather than directly calling the underlying toast API.

## Notes
- The wrapper currently forwards only the message and does not expose options for configuring duration, position, or styling. If you need richer control, extend the wrapper or call the underlying API directly.

---