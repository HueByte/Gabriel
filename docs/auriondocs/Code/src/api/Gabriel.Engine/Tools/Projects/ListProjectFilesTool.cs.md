# ListProjectFilesTool

> **File:** `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`  
> **Kind:** class

Lists all files uploaded to the current conversation's project and formats a concise, human-readable summary including each file's GUID, name, size, content type, and upload timestamp. Use this tool when you need to discover what reference materials are available in the conversation-scoped project before calling read_project_file to fetch specific file contents. The tool takes no parameters and returns a plain text result suitable for model consumption or display to a user.

## Remarks
This tool is a thin adapter over IProjectFileService.ListAsync that enforces the conversation's project context (via IToolExecutionContext) and converts the returned ProjectFile entities into a single textual summary. It intentionally includes each file's GUID (labeled as `id:`) because downstream tools — notably read_project_file — require the GUID rather than a filename. Errors from the underlying service are caught and returned as error messages rather than being thrown, so callers should treat the returned string as the complete response.

## Example
```csharp
// Typical usage inside an async context
var tool = new ListProjectFilesTool(projectFileService, toolExecutionContext);
string result = await tool.ExecuteAsync("{}", CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The tool requires the conversation to be attached to a project; if not, it returns an error message rather than listing files.
- File sizes use binary units (KiB, MiB) and are formatted to one decimal place for readability.
- Upload timestamps are formatted using the universal sortable pattern (ToString("u")).
- The tool's ParametersJsonSchema is empty — no arguments are accepted — and the returned value is plain text (not structured JSON).