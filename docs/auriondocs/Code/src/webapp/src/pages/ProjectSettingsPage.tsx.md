# ProjectSettingsPage.tsx

> **Source:** `src/webapp/src/pages/ProjectSettingsPage.tsx`

## Contents

- [ProjectSettingsPage](#projectsettingspage)
- [formatBytes](#formatbytes)

---

## ProjectSettingsPage

> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

Renders a project settings page and manages the UI and network interactions for viewing and editing a single project's metadata and files. The component loads project data from ProjectsService, exposes editable fields (name, description, system prompt), lets the user upload and delete project files, and saves changes back to the API. Reach for this component when you need a full settings/edit page for a project (typically mounted on a route like /projects/:projectId/settings) rather than managing individual fields or file uploads manually.

## Remarks
This component centralizes client-side form state separate from the loaded ProjectResponse so it can easily detect "dirty" changes and support cancel vs save semantics. It uses an AbortController to cancel in-flight project loads when the component unmounts or the projectId changes. API interactions are delegated to ProjectsService and ProjectFilesService; errors are routed through a shared notifyError helper and successes display toast notifications.

## Example
```typescript
// Typical usage as a route component
import { Route } from 'react-router-dom';

<Route path="/projects/:projectId/settings" element={<ProjectSettingsPage />} />
```

## Notes
- Dirty checking compares the local form fields to the original project values (uses null-coalescing for optional fields) — clearing a field is explicitly detected and sent as an update only when changed.
- The file input value is reset after selection so re-selecting the same file triggers onChange again.
- Deleting a file prompts with window.confirm; callers should be aware this is a synchronous blocking prompt.
- Skin changes are saved immediately and guarded by a `skinSaving` flag to avoid concurrent PUT races.
- The loader uses an AbortController; callers should expect loadProject to be cancellable when the component unmounts or projectId changes.

---

## formatBytes

> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

Convert a byte count into a short, human-readable string using binary (1024) units — B, KB, MB, or GB. Use this helper when displaying file or data sizes in a UI where compact, easy-to-read units are preferred; the function picks the largest unit that keeps the numeric value below the next threshold and formats it with a small number of decimal places.

## Remarks
The function uses 1024 as the unit step (binary units). It returns raw ASCII digits and a unit suffix (no localization of decimal separator or digit grouping). Rounding is chosen to keep the output compact: bytes are shown as an integer, KB and MB use one decimal place, and GB uses two decimal places. The implementation intentionally stops at GB — values larger than or equal to 1 TB will still be presented in GB with a large numeric value.

## Example
```typescript
console.log(formatBytes(500));           // "500 B"
console.log(formatBytes(1536));          // "1.5 KB"  (1536 / 1024 = 1.5)
console.log(formatBytes(1048576));       // "1.0 MB"  (1024 * 1024)
console.log(formatBytes(1073741824));    // "1.00 GB" (1024^3)
```

## Notes
- Non-finite or non-number inputs (NaN, undefined) will produce strings containing "NaN"; callers should validate numeric input when necessary.
- Negative byte counts are formatted with a leading minus sign (the function does not special-case negatives).
- If you need SI units (1000-based), localization, or support for larger units (TB, PB), extend or replace this helper accordingly.

---