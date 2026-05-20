namespace Gabriel.Core.Configuration;

// On-disk source for Gabriel's LLM-native self-docs. This is the PRIMARY docs
// source for docs_list / docs_read; the GitHub-backed source falls in behind it
// as a fallback (see GitHubDocsOptions).
//
// Resolution:
//   1. If Path is absolute and exists, use as-is.
//   2. Else probe Environment.CurrentDirectory and AppContext.BaseDirectory and
//      walk up a few parents looking for Path. First match wins.
//   3. If nothing is found, the source behaves as empty (no entries, no reads)
//      and logs a one-time warning. The composite lookup then transparently
//      falls back to the GitHub source.
public class LocalDocsOptions : IConfigSection<LocalDocsOptions>
{
    public static string SectionName => "Tools:Docs:Local";

    // Toggle without touching the rest of the wiring. When false, the local
    // source is skipped entirely and only the GitHub source is consulted.
    public bool Enabled { get; set; } = true;

    // Folder to read .md files from. Default points at the LLM-native self-docs
    // folder in the repo. Forward or back slashes both work on Windows; the
    // resolver normalizes to the platform separator.
    public string Path { get; set; } = "docs/gabriel-self-docs";
}
