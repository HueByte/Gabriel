# ProjectSettingsPage.tsx

> **Source:** `src/webapp/src/pages/ProjectSettingsPage.tsx`

## Contents

- [ProjectSettingsPage](#projectsettingspage)
- [formatBytes](#formatbytes)

---

## ProjectSettingsPage

> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

Renders the project settings UI for viewing and editing a single project (name, description, system prompt, skin, and attached files). Use this component for the project-level settings screen where the user should be able to make incremental edits, upload or delete files, and apply visual "skin" changes without a separate save button for skins.

## Remarks
This component loads project data from the ProjectsService (using the projectId from the route), lifts form state out of the fetched model for straightforward dirty-checking and cancel/save behavior, and manages several independent operations: saving project fields (PATCH semantics), uploading and deleting project files, and immediately applying skin changes (PUT). It coordinates optimistic UI updates (e.g., appending uploaded files) and error handling via notifyError/toast, and uses an AbortController to cancel the initial load when the component unmounts or the projectId changes.

## Notes
- Dirty-checking compares local form fields to the fetched project so empty-string edits are preserved and only changed fields are PATCHed.
- File input value is cleared after selection so selecting the same file again triggers onChange; uploaded files are appended to state after a successful POST.
- The skin save call runs immediately on picker change and is gated by a skinSaving flag to avoid concurrent requests/races.
- A 404 from the ProjectsService load is surfaced as a "Project not found." error; other errors use notifyError and do not change the not-found state.
- Deleting a file prompts the user with confirm(...) and removes it from local state only after a successful delete call.

---

## formatBytes

> **File:** `src/webapp/src/pages/ProjectSettingsPage.tsx`  
> **Kind:** function

Converts a byte count into a short, human‑readable string using 1024‑based units (B, KB, MB, GB). Reach for this helper when rendering file or payload sizes in the UI so values are compact and consistently formatted.

## Remarks
This function applies binary (base‑1024) thresholds to choose units and formats values with a small, fixed number of decimal places to keep labels concise: bytes are shown as integers, KB and MB use one decimal place, and GB uses two. It is intended for display purposes only and to provide a lightweight, predictable representation of sizes.

## Example
```typescript
console.log(formatBytes(500));            // "500 B"
console.log(formatBytes(1536));           // "1.5 KB"  (1536 / 1024 = 1.5)
console.log(formatBytes(5_242_880));      // "5.0 MB"  (5,242,880 / 1024 / 1024 = 5)
console.log(formatBytes(3_221_225_472));  // "3.00 GB"
```

## Notes
- Units use base 1024 (binary units) but are labeled "KB/MB/GB" (not the IEC KiB/MiB/GiB labels).
- Rounding is performed with toFixed (1 decimal for KB/MB, 2 for GB), so values are rounded, not truncated.
- Values >= 1 TB are still shown in GB (no TB support).
- Negative and fractional input values are accepted and formatted according to the same rules (e.g. -512 -> "-512 B", 1536.0 -> "1.5 KB").
- Output is not localized (decimal separator is '.') and always returns an ASCII string.

---