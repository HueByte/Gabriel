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


ProjectSettingsPage is a React function component that renders the user interface for viewing and editing a single project's metadata and assets. It derives the target project from the route parameter projectId, fetches the project via ProjectsService, and initializes local form state for name, description, systemPrompt, and the project’s files. Users can modify the project's name, description, and system prompt, upload new files, delete existing files, and adjust skin/appearance settings. Changes to metadata are sent using a PATCH request that only includes fields that actually changed, reducing unintended overwrites; file uploads and skin changes are performed through dedicated flows. The component coordinates loading, optimistic updates, and error handling to provide a cohesive project-admin experience within the web app.

## Remarks
ProjectSettingsPage centralizes the project-editing lifecycle on the web UI, encapsulating loading, mutation, and persistence in one place. By computing a dirty flag and PATCHing only changed fields, it minimizes network chatter and guards against inadvertently overwriting unrelated metadata. The file-management and appearance customization flows are kept close to the related UI controls to ensure a responsive, single-page experience while delegating server interactions to the respective services. It also uses abort signals to cancel in-flight requests when the route changes or the component unmounts, preventing stale state updates.

## Notes
- The dirty flag depends on the initially loaded project; until loading completes, the Save action remains inert to avoid sending partial data.
- Aborting the load via AbortController prevents state updates after unmount or route changes, reducing a class of race conditions.
- The PATCH payload sends only fields that differ from the original values; due to this, intentionally clearing a field (e.g., description) requires providing a new value, since unchanged empty strings are treated as non-differences. This ensures unrelated metadata is not overwritten by simultaneous edits.


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


Converts a numeric byte count into a human-friendly string using binary units (B, KB, MB, GB). It yields "N B" for small values, "N.N KB" for kilobytes, "N.N MB" for megabytes, and "N.NN GB" for gigabytes, choosing the largest unit that keeps the number readable. KB and MB are shown with one decimal place, while GB uses two decimals.

## Remarks
formatBytes serves as a small, reusable formatter for byte counts used anywhere the UI needs a human-friendly size. It codifies a consistent boundary at 1024-based thresholds and a uniform decimal precision, so developers don’t have to replicate the logic. By centralizing the formatting, it helps keep the UI consistent and makes future adjustments straightforward.

## Notes
- Locale and i18n: the function is locale-agnostic and uses a period as the decimal separator; if localization is required, adapt the formatting to the target locale.
- Negative values are not validated; negative byte counts will yield a negative size string (e.g., "-1 B").

---