namespace Gabriel.Infrastructure.Projects;

public class ProjectFilesOptions
{
    public const string SectionName = "Projects:Files";

    // Root directory under which every project gets its own subfolder
    // ({Root}/{ProjectId:N}/{file}). Set via GABRIEL_PROJECTS__FILES__ROOT.
    // Default puts it next to the SQLite DB so dev / single-box deploys
    // "just work".
    public string Root { get; set; } = "./projects-data";

    // Hard cap per upload. Reject larger files at the boundary so a giant
    // upload can't fill the disk.
    public long MaxFileBytes { get; set; } = 25 * 1024 * 1024;  // 25 MiB

    // Extensions that may be uploaded. Conservative whitelist of text / common
    // doc formats - adjust per deployment if you genuinely need others.
    public IList<string> AllowedExtensions { get; set; } = new List<string>
    {
        ".txt", ".md", ".json", ".csv", ".tsv", ".yml", ".yaml", ".xml",
        ".html", ".htm",
        ".py", ".js", ".ts", ".tsx", ".jsx", ".cs", ".java", ".go", ".rs",
        ".sql", ".sh", ".ps1", ".bat",
        ".pdf", ".docx",
        ".log",
    };

    // Content types treated as "text-readable" by ReadTextAsync. Files outside
    // this set can still be downloaded but the read tool refuses them so the
    // model isn't shoved random binary bytes.
    public IList<string> TextContentTypePrefixes { get; set; } = new List<string>
    {
        "text/", "application/json", "application/xml", "application/javascript",
        "application/x-yaml", "application/yaml",
    };
}
