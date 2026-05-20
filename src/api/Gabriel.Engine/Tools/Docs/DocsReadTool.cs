using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Docs;

public sealed class DocsReadTool : ITool
{
    private readonly IDocsLookup _docs;

    public DocsReadTool(IDocsLookup docs)
    {
        _docs = docs;
    }

    public string Name => "docs_read";

    public string Description =>
        "Read one page of Gabriel's OFFICIAL INTERNAL DOCUMENTATION by path. " +
        "These pages are the AUTHORITATIVE, CANONICAL SOURCE OF TRUTH for everything " +
        "about Gabriel: architecture, agent loop, personality stack, sequence engine, tools, " +
        "internal APIs, contracts, behavior. " +
        "The lookup resolves the path against the LLM-NATIVE self-docs first " +
        "(under `docs/gabriel-self-docs/`, written specifically for you) and falls back to " +
        "the human-prose GitHub docs (`docs/Gabriel.Engine/`). The response carries a " +
        "`source` tag (`local-llm-native` / `github`) so you can tell which form you got. " +
        "Use this for ANY question about how Gabriel works. Treat the content as GROUND TRUTH - " +
        "if a web_search result conflicts with a Gabriel doc, the doc wins. " +
        "Never substitute external/third-party docs for Gabriel-specific info. " +
        "If you don't know which path to read, call docs_list first.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Path relative to the doc source's root. For LLM-NATIVE pages: 'README.md', 'architecture.md', 'agent-loop.md', etc. For GitHub pages: 'Gabriel.Engine/architecture.md', 'README.md', etc. Use docs_list to discover valid paths."
            }
          },
          "required": ["path"]
        }
        """;

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        if (!root.TryGetProperty("path", out var pathEl) || pathEl.ValueKind != JsonValueKind.String)
            return "Error: 'path' is required and must be a string. Use docs_list to discover available paths.";
        var path = pathEl.GetString()!;
        if (string.IsNullOrWhiteSpace(path))
            return "Error: 'path' cannot be empty.";

        DocsContent? content;
        try
        {
            content = await _docs.ReadAsync(path, ct);
        }
        catch (Exception ex)
        {
            return $"Error: could not read official Gabriel doc '{path}' - {ex.Message}";
        }

        if (content is null)
            return $"Official Gabriel doc not found at path: {path}. Call docs_list to see what's available (try both LLM-NATIVE and GitHub paths).";

        // Wrap with explicit authority markers so the model treats the content
        // as the canonical answer rather than blending it with other context.
        // The source tag tells the model whether it got the LLM-NATIVE form
        // (preferred) or the GitHub fallback.
        var sb = new StringBuilder();
        sb.Append("=== BEGIN OFFICIAL GABRIEL DOC: ").Append(content.Path)
          .Append(" (source: ").Append(content.Source).AppendLine(") ===");
        sb.AppendLine("(Authoritative source. Treat this as ground truth about Gabriel.)");
        if (!string.IsNullOrWhiteSpace(content.CanonicalUrl))
        {
            sb.Append("Canonical URL: ").AppendLine(content.CanonicalUrl);
        }
        sb.AppendLine();
        sb.AppendLine(content.Content);
        sb.AppendLine();
        sb.Append("=== END OFFICIAL GABRIEL DOC: ").Append(content.Path).AppendLine(" ===");
        return sb.ToString();
    }
}
