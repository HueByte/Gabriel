# ProjectFilesOptions

> **File:** `src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs`  
> **Kind:** class

```csharp
public class ProjectFilesOptions : IConfigSection<ProjectFilesOptions>
```


ProjectFilesOptions is a configuration container that centralizes the policies for storing and validating per-project uploaded files. It binds to the Projects:Files configuration section and exposes a Root path for per-project subfolders, a maximum file size for uploads, a conservative list of allowed extensions, and a read-time whitelist of text-content types used by ReadTextAsync. Developers reach for it when they need to tailor where project files live, how large uploads can be, and which file types are eligible for reading or inline display, all without touching runtime code.

## Remarks
By isolating these concerns, the class decouples filesystem layout and content moderation from business logic. It documents the expected per-project subfolder structure and the read behavior controlled by TextContentTypePrefixes, ensuring consistent behavior across environments. Because the type implements [`IConfigSection<T>`](IConfigSection.cs.md), changes can be supplied via configuration without recompiling.

## Example
```csharp
// Common binding in startup/host
services.Configure<ProjectFilesOptions>(Configuration.GetSection(ProjectFilesOptions.SectionName));

// Optional programmatic override
var options = new ProjectFilesOptions
{
    Root = "/var/app/projects",
    MaxFileBytes = 50 * 1024 * 1024,
    AllowedExtensions = new List<string> { ".md", ".txt" }
};
```

## Notes
- The Root path is relative to the application root by default; ensure the configured directory is writable by the app process in your deployment.
- MaxFileBytes is a per-upload cap and does not reflect total storage usage; monitor disk consumption and adjust as needed for your workload.
- TextContentTypePrefixes governs which MIME-like types ReadTextAsync will treat as text for inline reading; files outside this set can be downloaded but won’t be surfaced as text content.