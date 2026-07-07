# ListProjectFilesTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`  
> **Kind:** class

```csharp
public sealed class ListProjectFilesTool : ITool
```


ListProjectFilesTool lists all files uploaded to the current conversation's project and returns a compact, human-friendly report containing each file's name, id, size, content type, and upload timestamp. Use it when you need to discover what materials exist in a project before selecting a file_id to pass to read_project_file.

## Remarks
This symbol serves as a discovery primitive: it centralizes the retrieval of project assets and formats them for quick human consumption. By keeping the listing logic in one place, it decouples the decision of which file to read from the act of enumerating available files. It also ensures the file identifier is always present in the output, addressing a prior pattern where only names were shown and downstream actions required a GUID. The produced text relies on the current project context (via the execution context) and on the project-file service to fetch metadata.

## Notes
- Requires the tool to be attached to a project; if the execution context has no ProjectId, it returns an error message.
- The output includes the bracketed id value (id) for each file, which is the value you must pass as file_id to read_project_file.
- Output is a human-readable string; if you need to parse ids programmatically, prefer consuming the id fields from the raw data source or implement a parser for the produced text.