namespace Gabriel.Infrastructure.Tools.Docs;

public class GitHubDocsOptions
{
    public const string SectionName = "Tools:Docs:GitHub";

    // The repository hosting Gabriel's docs. Defaults point at the canonical
    // upstream so the docs tool works out of the box with no config.
    // Note: the local working-tree folder is named "PulsePixel" but the GitHub
    // repo is "Gabriel" — the remote name is what counts here.
    public string Owner { get; set; } = "HueByte";
    public string Repo { get; set; } = "Gabriel";
    public string Branch { get; set; } = "main";

    // Path within the repo where the docs live. The tool only walks .md files
    // under this prefix.
    public string DocsPath { get; set; } = "docs";

    // Optional personal access token. Unauthenticated GitHub API has a 60 req/h
    // per-IP limit which is plenty for a docs lookup tool, but a PAT raises it
    // to 5000/h. Supply via Infisical (TOOLS__DOCS__GITHUB__TOKEN) if needed.
    public string? Token { get; set; }

    // Cache the list response for this long. Reads are not cached (small enough,
    // and may reflect in-flight doc edits).
    public int ListCacheMinutes { get; set; } = 5;
}
