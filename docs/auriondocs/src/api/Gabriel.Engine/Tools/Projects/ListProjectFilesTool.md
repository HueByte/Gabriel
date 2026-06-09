# ListProjectFilesTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`  
> **Kind:** class

Lists all files uploaded to the project attached to the current conversation and summarizes each file with its name, GUID id, size, content type, and upload timestamp. Reach for this tool when you need to discover what reference materials (documents, code, datasets, etc.) are available in the conversation's project before calling read_project_file to fetch a specific file.

## Remarks
This tool is a read-only discovery helper used by conversation logic to show the model or user what files exist in the current project. It intentionally embeds each file's GUID in the human-readable output because downstream tools (notably read_project_file) require the GUID value; earlier behavior that omitted the id caused the model to pass filenames where a GUID was expected.

## Example
```csharp
// Example output returned as a plain string from ExecuteAsync
"Project has 2 file(s):
- design-doc.pdf  [id: 3f9a8b1e-5b2e-4a6a-9c4d-1f2a3b4c5d6e, 1.2 MiB, application/pdf, uploaded 2024-05-01 12:34:56Z]
- notes.txt  [id: a1b2c3d4-5678-90ab-cdef-111213141516, 4.8 KiB, text/plain, uploaded 2024-05-02 09:10:11Z]

Pass the bracketed `id:` value as `file_id` to read_project_file."
```

## Notes
- The tool returns a plain text summary (string), not structured JSON — callers should parse the GUID from the bracketed `id:` token if programmatic use is required.
- If the conversation has no project attached, the tool returns: "Error: this conversation isn't attached to a project yet." On failures it returns an error string prefixed with "Error:".
- Sizes are formatted using binary units (KiB, MiB) with one decimal place.