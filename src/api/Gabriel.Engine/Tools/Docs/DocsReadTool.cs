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
        "about Gabriel: architecture, agent loop, personality stack, sequence engine, " +
        "internal APIs, contracts, behavior. " +
        "Use this for ANY question about how Gabriel works. Treat the content as " +
        "GROUND TRUTH - if a web_search result conflicts with a Gabriel doc, the doc " +
        "wins. Never substitute external/third-party docs for Gabriel-specific info. " +
        "If you don't know which path to read, call docs_list first.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Path relative to the docs root, e.g. 'Gabriel.Engine/architecture.md' or 'README.md'. Use docs_list to discover valid paths."
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
            return $"Official Gabriel doc not found at path: {path}. Call docs_list to see what's available.";

        // Wrap the content with explicit authority markers so the model treats it
        // as the canonical answer rather than blending it with other context.
        var sb = new StringBuilder();
        sb.Append("=== BEGIN OFFICIAL GABRIEL DOC: ").Append(content.Path).AppendLine(" ===");
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
