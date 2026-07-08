# ProjectFilesOptions

> **File:** `src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs`  
> **Kind:** class

```csharp
public class ProjectFilesOptions : IConfigSection<ProjectFilesOptions>
```


ProjectFilesOptions is a configuration container that defines how uploaded project files are stored and consumed. It binds to the SectionName property for the Projects:Files configuration section and exposes defaults for the root storage path, a hard file-size cap, a whitelist of allowed file extensions, and the set of content-type prefixes that ReadTextAsync will treat as text. Developers reference this type to customize per-environment behavior—such as changing the storage root, tightening or relaxing upload limits, expanding or narrowing supported file types, or adjusting which content should be opened as text for in-app previews—without scattering constants across the codebase.

## Remarks
By encapsulating these settings in a single, strongly-typed options object, the system ensures consistent defaults and a single source of truth for project-file handling. It decouples file-system layout, validation rules, and read behavior from business logic, enabling easier testing and deployment-time overrides via configuration sources.

## Notes
- The MaxFileBytes boundary prevents giant uploads from filling disk by rejecting larger files at the boundary, helping protect storage.
- The AllowedExtensions whitelist should reflect deployment needs; add or remove extensions as required and ensure the uploader enforces the same policy.
- The TextContentTypePrefixes determine which content types ReadTextAsync will treat as text; content outside this set can still be downloaded but will not be read as text for in-app previews.