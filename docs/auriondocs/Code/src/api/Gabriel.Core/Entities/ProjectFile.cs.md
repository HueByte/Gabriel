# ProjectFile

> **File:** `src/api/Gabriel.Core/Entities/ProjectFile.cs`  
> **Kind:** class

```csharp
public class ProjectFile
```


ProjectFile is the metadata record for a file uploaded to a project, capturing its identity, project affiliation, display name, relative path, size, content type, and upload timestamp. Its static Create factory validates inputs, trims and normalizes values, and defaults the content type when omitted to ensure consistently constructed, safe metadata before persistence.

## Remarks

These properties establish a stable contract between a file's binary data and its descriptive metadata, enabling services to link files to their projects without leaking path or normalization concerns. The class uses a private setter pattern, so metadata remains immutable from outside code after creation, with changes routed through controlled construction. RelativePath normalization to forward slashes helps maintain a uniform repository layout, while the service layer enforces any deeper path traversal protections. In typical usage, ProjectFile instances are produced when a file is uploaded and then stored alongside its ProjectId reference.

## Example

```csharp
// Common usage
var projectId = Guid.NewGuid();
var file = ProjectFile.Create(projectId, "Report.pdf", "reports/2026/Report.pdf", 2048, "application/pdf");

// Accessing metadata (ids are generated on creation)
Console.WriteLine(file.Id);        // generated unique id
Console.WriteLine(file.ProjectId); // ties the file to its project
```

## Notes

- Create enforces invariants by throwing ArgumentException for invalid inputs (empty project id, empty name, empty relative path, negative size).
- RelativePath normalization only replaces separators; path traversal protection is enforced at the service layer.