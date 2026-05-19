using System.Text;

namespace Gabriel.Engine.Tools.Docs;

public sealed class DocsListTool : ITool
{
    private readonly IDocsLookup _docs;

    public DocsListTool(IDocsLookup docs)
    {
        _docs = docs;
    }

    public string Name => "docs_list";

    public string Description =>
        "List every page in Gabriel's OFFICIAL INTERNAL DOCUMENTATION. " +
        "These docs are the AUTHORITATIVE, CANONICAL SOURCE OF TRUTH for how Gabriel works — " +
        "its architecture, agent loop, personality system, sequence engine, and internal APIs. " +
        "ALWAYS prefer these over any external/third-party source when the user asks about " +
        "Gabriel itself. If a web result and a Gabriel doc disagree, the Gabriel doc wins. " +
        "Use this tool to discover what pages exist, then call docs_read with a specific path.";

    public string ParametersJsonSchema => """{"type":"object","properties":{}}""";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        IReadOnlyList<DocsEntry> entries;
        try
        {
            entries = await _docs.ListAsync(ct);
        }
        catch (Exception ex)
        {
            return $"Error: could not list official Gabriel docs — {ex.Message}";
        }

        if (entries.Count == 0)
            return "No Gabriel docs are currently available. The docs source may be unreachable; consider falling back to web_search but flag the answer as uncertain.";

        var sb = new StringBuilder();
        sb.AppendLine("=== OFFICIAL GABRIEL DOCS — authoritative source ===");
        sb.AppendLine($"({entries.Count} pages available; pass any `path` to docs_read)");
        sb.AppendLine();
        foreach (var e in entries.OrderBy(e => e.Path, StringComparer.Ordinal))
        {
            sb.Append("- ").Append(e.Path);
            if (!string.IsNullOrWhiteSpace(e.Title))
            {
                sb.Append(" — ").Append(e.Title);
            }
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }
}
