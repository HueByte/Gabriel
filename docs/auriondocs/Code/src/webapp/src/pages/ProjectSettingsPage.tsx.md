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


The ProjectSettingsPage component renders the user interface for editing a project's metadata and managing its files. It reads the projectId from the route, loads the project data on mount, and initializes local form state (name, description, systemPrompt) from the server. Users can modify these fields and save; the save operation PATCHes only the fields that have changed to avoid overwriting other metadata, and it provides immediate feedback via toasts. The page also supports uploading new files to the project, listing current files, and deleting files with confirmation, updating the UI as changes succeed or fail. Separate loading flags (saving, uploading, skinSaving) ensure actions do not race with each other, and an AbortController cancels in-flight requests when the route changes, preventing stale data.

## Remarks
This component acts as a thin view-model that ties together UI state and server persistence for a project's settings. By computing a precise dirty state and performing a partial PATCH, it prevents unintended data loss while minimizing payloads. It also centralizes concerns around project metadata and asset management, delegating actual persistence to dedicated services (ProjectsService and ProjectFilesService) to improve testability and future reuse.

## Example
```typescript
// Usage within router
<Route path="/projects/:projectId/settings" element={<ProjectSettingsPage />} />
```

## Notes
- Deleting a file prompts the user for confirmation before performing the API call.
- After selecting a file for upload, the input value is reset to allow re-uploading the same file if needed.
- The patch payload includes only fields that have changed; unchanged fields are omitted to avoid unintended resets.

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


Converts a numeric byte count into a human-friendly string for display, using binary units. It selects B, KB, MB, or GB based on the magnitude of the input and applies specific decimal precision: values under 1024 are shown as an integer with a ' B' suffix; values under a megabyte are shown in kilobytes with one decimal place; values under a gigabyte are shown in megabytes with one decimal place; and larger values are shown in gigabytes with two decimal places. This utility is commonly used when presenting file sizes in the UI to ensure consistent, readable formatting across the application.

## Remarks
This function centralizes size formatting to avoid duplicating the same logic across components. By using 1024-based thresholds and fixed decimal precision, it provides predictable, readable output that remains consistent across the UI. If you anticipate sizes above a gigabyte, consider extending the function (e.g., adding a TB branch) or adding a separate formatter for very large values.

## Notes
- This implementation uses 1024-based boundaries, so KB/MB/GB reflect binary sizes rather than decimal prefixes.
- KB and MB are shown with one decimal place; GB is shown with two decimal places.
- There is no explicit TB branch; extremely large values will still render as GB (with two decimals), which may be misleading for very large datasets.

---