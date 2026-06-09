# ProjectFilesOptions

> **File:** `src/api/Gabriel.Core/Configuration/ProjectFilesOptions.cs`  
> **Kind:** class

```csharp
public class ProjectFilesOptions : IConfigSection<ProjectFilesOptions>
```


ProjectFilesOptions is a configuration POCO that captures settings for per-project file storage and upload policy and is bound to the configuration section "Projects:Files". It exposes Root (default "./projects-data") where each project gets its own subfolder ({Root}/{ProjectId:N}/{file}), a hard upload cap MaxFileBytes (default 25 MiB), a conservative AllowedExtensions whitelist, and TextContentTypePrefixes which control which content types are treated as "text-readable" by ReadTextAsync. Developers use it when they need to configure or override where project files are stored, what file sizes and extensions are accepted, and which MIME types the application should treat as text for in-app reading.

## Remarks
The class implements [`IConfigSection<ProjectFilesOptions>`](IConfigSection.cs.md) and exposes SectionName so it can be bound from configuration providers; the Root path may be overridden via the GABRIEL_PROJECTS__FILES__ROOT environment variable according to the inline comment.