# ListProjectFilesTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`  
> **Kind:** class

```csharp
public sealed class ListProjectFilesTool : ITool
```


ListProjectFilesTool lists all files uploaded to the current conversation's project and formats them into a human-friendly manifest. It resolves the active project from the execution context, fetches the files via the project file service, and renders a header with the total count followed by one line per file in the form "- {Name}  [id: {Id}, {Size}, {ContentType}, uploaded {UploadedAt:u}]". Sizes are shown using B, KiB, or MiB units, and the upload timestamp uses the universal sortable format. The manifest always includes the file's GUID id so downstream commands (such as read_project_file) can reliably locate a file by its ID. If the conversation isn't attached to a project, it returns the error "Error: this conversation isn't attached to a project yet." If listing fails, it returns a descriptive error like "Error: could not list project files - {ex.Message}". If there are no files, it returns "No files uploaded to this project yet." This tool is intended for discovery: use it to see what's available before selecting a file to read by its id.

## Remarks
ListProjectFilesTool serves as a stable discovery surface for a project's assets. By delegating to the project-file service and consistently formatting every entry with its GUID, it decouples presentation from storage and ensures downstream operations can locate a file deterministically. The design prioritizes human-readable output while guaranteeing the presence of the essential identifier needed for subsequent read operations.

## Notes
- The output is a plain string designed for readability; if you need structured data, consider querying the underlying services directly or parsing the string with the guaranteed id field present.
- File names containing unusual characters or newlines may affect the formatting of a single line; consumers should be prepared for potential edge cases in parsing.
- The operation requires an attached project context; ensure the conversation is connected to a project before invoking.