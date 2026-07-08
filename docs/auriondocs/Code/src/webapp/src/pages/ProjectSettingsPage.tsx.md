# ProjectSettingsPage.tsx

> **Source:** `src/webapp/src/pages/ProjectSettingsPage.tsx`

## Contents

- [ProjectSettingsPage](#projectsettingspage)
- [formatBytes](#formatbytes)

---

## ProjectSettingsPage
> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

```typescript
export function ProjectSettingsPage()
```


ProjectSettingsPage is a React functional component that renders a settings editor for a single project identified by the route's projectId. It fetches the project's data, mirrors key fields (name, description, systemPrompt) in local state for dirty-checking, and provides save, file upload, and file deletion capabilities with abortable requests and user feedback.

## Remarks
This component acts as the UI and orchestration layer for project configuration, coordinating data loading, optimistic updates, and server mutations via the ProjectsService and ProjectFilesService. It demonstrates a common pattern in admin-style pages: loading a domain object, presenting a controlled form, and performing partial updates (PATCH) that only send changed fields to minimize unintended overwrites. The component also integrates file management and a lightweight skin-update flow, illustrating how cross-cutting concerns like loading state, error handling, and cancellation are wired into a single page.

## Notes
- The Save operation patches only the fields that actually changed, reducing the risk of clobbering unrelated fields.
- Data loading is cancelable via AbortController/AbortSignal to avoid state updates after unmounting or navigation.
- Deleting a file uses a browser confirm dialog to prevent accidental removals; file uploads reset the input value to allow re-uploading the same file.
- A skin update pathway exists and is guarded to avoid race conditions (skinSaving flag); this enables instant visual feedback when the user selects a new skin configuration, without requiring a separate explicit action.


---

## formatBytes
> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

```typescript
function formatBytes(bytes: number): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `bytes` | `number` | — |

**Returns:** `string`


Formats a numeric byte count into a human-friendly string using binary-size units (B, KB, MB, GB). This small, pure helper is useful whenever you need to display storage sizes in the UI rather than raw byte counts.

## Remarks

This function centralizes the formatting rules for byte counts, ensuring consistent display across the application. It relies on binary thresholds (1024, 1024^2, 1024^3) and formats KB and MB with one decimal place, while GB uses two decimal places. Note that negative inputs are not guarded against; they will be formatted as a negative number of bytes in B (e.g., -500 B). If negative values are possible in your data, consider clamping beforehand or extending the function with input validation.

## Example

```typescript
formatBytes(500); // "500 B"
formatBytes(2048); // "2.0 KB"
formatBytes(5 * 1024 * 1024); // "5.0 MB"
formatBytes(3 * 1024 * 1024 * 1024); // "3.00 GB"
```

## Notes

- Negative inputs are not guarded against; callers may want to clamp to non-negative values.
- Uses binary prefixes (KB/MB/GB) with 1 decimal for KB/MB and 2 decimals for GB.
- This is a pure formatter; it does not localize units or numbers.

---