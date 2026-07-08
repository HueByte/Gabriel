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


Converts an unknown error into a user-friendly string by first extracting details from ApiError bodies, then falling back to a plain Error message, and finally using a provided fallback for non-errors. Use this to normalize error messaging in UI layers where errors may originate from API responses or generic exceptions.

## Remarks
This function centralizes error message formatting across different error shapes (ApiError vs. generic Error), reducing duplication and ensuring consistent user feedback. By preferring body.detail over body.title, it surfaces the most specific API-provided information when available, while still presenting meaningful messages for non-ApiError errors. The approach relies on runtime type checks (instanceof) to select the appropriate path, making the behavior predictable at call sites.

## Notes
- Potential gotcha: ApiError must be a runtime class; if not, the ApiError branch is skipped.
- It always returns a string, so no crash due to non-string outputs; however, ensure the fallback is a string.

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


notifyError takes an unknown error value and an optional fallback string, formats the error with formatError, and surfaces it to the user via toast.error. Use this helper to centralize how errors are presented in the UI, instead of calling toast.error and formatting errors ad-hoc throughout the codebase.

## Remarks

By encapsulating error formatting and presentation, this symbol establishes a single, reusable contract for user-facing error notifications. It decouples the UI notification mechanism from error sources, so you can swap the underlying toast library or the formatting strategy in one place without touching call sites.

## Notes

- Triggers a UI side-effect (a toast) and may be affected by tests that mock or suppress UI notifications; ensure test setup accounts for UI interactions.

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


notifyInfo is a tiny helper that renders an informational toast by delegating to toast.info. It accepts a single string message and surfaces it to the user as an info-styled notification. This wrapper exists to centralize usage of the underlying toast library, making it easier to apply consistent defaults, replace or augment the notification mechanism, and keep call sites readable.

## Remarks
By naming the operation notifyInfo rather than calling toast.info directly, the codebase gains a semantic abstraction for informational messages. This makes it straightforward to adjust default behavior (such as autoClose duration, position, or styling) in one place, or to swap the notification implementation during testing or UI rewrites without touching call sites. It also simplifies testing by allowing mocks of the high-level notifyInfo API.

## Example
```typescript
// Common usage
notifyInfo("Data saved successfully");
```

## Notes
- The app must configure the toast container (style and behavior) for the info to display correctly.
- If you need more control over presentation, consider extending this wrapper or configuring toast defaults centrally.

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


notifySuccess is a small wrapper around toast.success that displays a success toast with the given message. Developers should reach for this helper when they want a consistent, centralized way to show success notifications, making it easy to swap the underlying toast implementation in the future without changing call sites.

## Remarks

By centralizing toast usage, you can swap the notification library or adjust default options in one place. It also makes unit testing easier, as you can mock notifySuccess instead of dealing with the toast library directly. The naming convention communicates intent clearly and pairs well with other notify helpers you might introduce (e.g., notifyError).

## Example

```typescript
notifySuccess('Profile updated successfully');
```

## Notes

- It is a thin alias; if you need per-call customization (duration, position, etc.), extend this wrapper or provide an overload.
- Ensure a toast container (or equivalent) is configured in your app; otherwise, the toast may not render.
- When testing, prefer mocking notifySuccess over the toast library to keep tests deterministic.

---