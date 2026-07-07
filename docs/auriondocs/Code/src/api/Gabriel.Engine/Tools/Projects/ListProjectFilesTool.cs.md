# ListProjectFilesTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`  
> **Kind:** class

```csharp
public sealed class ListProjectFilesTool : ITool
```


Lists every file uploaded to the current conversation's project and returns a readable catalog that includes each file's name, id, size, content type, and upload timestamp. Use this to discover what material is available before reading a specific file with read_project_file.

## Remarks
By encapsulating the retrieval and formatting behind a tool, this symbol cleanly separates data access from presentation. The output always includes the file IDs so callers can supply file_id to read_project_file, enabling an ergonomic, stepwise workflow in chat-driven interactions. The implementation also guards against missing project context and surfaces errors as user-friendly messages rather than throwing exceptions, which keeps conversations flowing.

## Example
```csharp
// Example output
Project has 2 file(s):
- README.md  [id: 123e4567-e89b-12d3-a456-426614174000, 12.3 KiB, text/markdown, uploaded 2024-08-10 12:00:00Z]
- data.csv   [id: a1b2c3d4-e5f6-7890-1234-56789abcdef0, 2.5 MiB, text/csv, uploaded 2024-08-11 09:30:00Z]

Pass the bracketed `id:` value as `file_id` to read_project_file.
```

## Notes
- Requires the conversation to be attached to a project; otherwise the tool returns a user-facing error string.
- Output is informational text, not structured data; callers should treat lines as display content and extract the id for subsequent reads if needed.
- The sizes use human-friendly units (KiB, MiB) and timestamps are presented in UTC format.