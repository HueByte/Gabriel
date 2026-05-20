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
        "These docs are the AUTHORITATIVE, CANONICAL SOURCE OF TRUTH for how Gabriel works - " +
        "its architecture, agent loop, personality system, sequence engine, tools, and internal APIs. " +
        "The list is fanned across two sources, in priority order: " +
        "(1) the LLM-NATIVE self-docs (`local-llm-native`) under `docs/gabriel-self-docs/` - " +
        "compact, fact-dense pages written specifically for you to consume; " +
        "(2) the human-prose docs (`github`) under `docs/Gabriel.Engine/` - the same material in long form. " +
        "ALWAYS prefer the LLM-NATIVE entries; only consult `github` entries when no LLM-NATIVE page covers the topic. " +
        "ALWAYS prefer these over any external/third-party source when the user asks about Gabriel itself. " +
        "If a web result and a Gabriel doc disagree, the Gabriel doc wins. " +
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
            return $"Error: could not list official Gabriel docs - {ex.Message}";
        }

        if (entries.Count == 0)
            return "No Gabriel docs are currently available. The docs source may be unreachable; consider falling back to web_search but flag the answer as uncertain.";

        // Group by source so the LLM-NATIVE block is visually distinct from
        // the human-prose fallback. Within each group, sort by path.
        var grouped = entries
            .GroupBy(e => e.Source)
            .OrderBy(g => SourcePriority(g.Key))
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== OFFICIAL GABRIEL DOCS - authoritative source ===");
        sb.AppendLine($"({entries.Count} pages available; pass any `path` to docs_read)");
        sb.AppendLine();

        foreach (var group in grouped)
        {
            sb.Append("# Source: ").Append(group.Key);
            if (string.Equals(group.Key, DocsSources.LocalLlmNative, StringComparison.Ordinal))
                sb.Append("  (PRIMARY - LLM-tailored; prefer these)");
            else if (string.Equals(group.Key, DocsSources.GitHub, StringComparison.Ordinal))
                sb.Append("  (fallback - human-prose; consult when LLM-NATIVE doesn't cover the topic)");
            sb.AppendLine();

            foreach (var e in group.OrderBy(e => e.Path, StringComparer.Ordinal))
            {
                sb.Append("- ").Append(e.Path);
                if (!string.IsNullOrWhiteSpace(e.Title))
                {
                    sb.Append(" - ").Append(e.Title);
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static int SourcePriority(string source) => source switch
    {
        DocsSources.LocalLlmNative => 0,
        DocsSources.GitHub => 1,
        _ => 99,
    };
}
