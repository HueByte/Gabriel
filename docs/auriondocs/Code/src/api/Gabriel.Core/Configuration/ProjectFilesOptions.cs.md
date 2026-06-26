# ProjectFilesOptions

> **File:** `src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs`  
> **Kind:** class

Holds configuration for how project-scoped files are stored and treated by the application — where files are placed on disk, the per-upload size cap, an extension whitelist for uploads, and which content types are safe to treat as text. Use this options type when binding configuration (or environment variables) that control project file storage and the behavior of file-reading utilities like ReadTextAsync.

## Remarks
This POCO is intended to be bound from the application's configuration system (the SectionName constant "Projects:Files" is used as the config section key and environment variables such as GABRIEL_PROJECTS__FILES__ROOT can override the Root). Defaults are chosen to make development and single-box deployments work out of the box (a local ./projects-data root and a conservative file-extension whitelist). The AllowedExtensions and TextContentTypePrefixes shape both upload acceptance and the read-paths (e.g., ReadTextAsync uses the content-type prefixes to decide whether a file can be safely presented as text to downstream consumers).

## Example
```csharp
// Bind from IConfiguration in startup
var options = new ProjectFilesOptions();
configuration.GetSection(ProjectFilesOptions.SectionName).Bind(options);

// Later: use options when validating an uploaded file
if (uploadedFile.Length > options.MaxFileBytes) throw new InvalidOperationException("File too large");
var ext = Path.GetExtension(uploadedFile.FileName);
if (!options.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException("Extension not allowed");
```

## Notes
- AllowedExtensions entries include the leading dot (e.g. ".txt"). Checks should typically be performed case-insensitively (the list itself is not normalized by this class).
- Root is a relative path by default ("./projects-data"); treat it as a runtime/configuration value — changing it requires rebinding/restart if your host binds once at startup.
- TextContentTypePrefixes are prefix matches (for example, "text/" matches "text/plain"); files whose content type is not in this set may still be downloadable but will be refused by text-reading helpers to avoid feeding binary data into text-based processing.
