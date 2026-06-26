using System.Text.Encodings.Web;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Data;

// Validate, pretty-print, or minify a JSON string. A pure function of its
// arguments - no I/O, no dependency. System.Text.Json does the parsing and
// re-serialisation; this just wraps it with friendly modes and error reporting.
public sealed class JsonFormatTool : ITool
{
    private const int MaxJsonLength = 500_000;

    // Relaxed escaping so re-serialised output stays readable - Unicode and
    // characters like < > & are emitted literally rather than as \uXXXX. This
    // output is for a human/agent to read, not to embed in HTML.
    private static readonly JsonSerializerOptions Pretty =
        new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly JsonSerializerOptions Compact =
        new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public string Name => "json_format";

    public string Description =>
        "Validate, pretty-print, or minify JSON. " +
        "USE THIS to make JSON readable, compact it onto one line, or check whether " +
        "a string is valid JSON (it reports the error location if not) - instead of " +
        "reformatting by hand. mode: pretty (default, 2-space indent), minify " +
        "(single line), or validate (report valid/invalid only). " +
        "NOT for querying or restructuring JSON (no tool for that yet).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "json": {
              "type": "string",
              "description": "The JSON text to format or validate."
            },
            "mode": {
              "type": "string",
              "enum": ["pretty", "minify", "validate"],
              "default": "pretty",
              "description": "pretty = re-indent; minify = compact to one line; validate = check only."
            }
          },
          "required": ["json"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string json, mode;
        try { (json, mode) = ReadArgs(argumentsJson); }
        catch (JsonFormatException ex) { return Task.FromResult($"Error: {ex.Message}"); }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            // LineNumber / BytePositionInLine are 0-based; present them 1-based.
            var line = (ex.LineNumber ?? 0) + 1;
            var pos = (ex.BytePositionInLine ?? 0) + 1;
            return Task.FromResult($"Error: invalid JSON at line {line}, position {pos}.");
        }

        using (doc)
        {
            return Task.FromResult(mode switch
            {
                "validate" => $"Valid JSON ({doc.RootElement.ValueKind.ToString().ToLowerInvariant()}).",
                "minify" => JsonSerializer.Serialize(doc.RootElement, Compact),
                _ => JsonSerializer.Serialize(doc.RootElement, Pretty),
            });
        }
    }

    private static (string Json, string Mode) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new JsonFormatException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("json", out var jsonEl) || jsonEl.ValueKind != JsonValueKind.String)
                throw new JsonFormatException("'json' is required and must be a string.");
            var json = jsonEl.GetString() ?? "";
            if (json.Length == 0)
                throw new JsonFormatException("'json' cannot be empty.");
            if (json.Length > MaxJsonLength)
                throw new JsonFormatException($"'json' is too long (max {MaxJsonLength} characters).");

            var mode = "pretty";
            if (root.TryGetProperty("mode", out var modeEl) && modeEl.ValueKind != JsonValueKind.Null)
            {
                if (modeEl.ValueKind != JsonValueKind.String)
                    throw new JsonFormatException("'mode' must be a string.");
                mode = modeEl.GetString()!.ToLowerInvariant();
                if (mode is not ("pretty" or "minify" or "validate"))
                    throw new JsonFormatException($"'mode' must be pretty, minify, or validate (got '{modeEl.GetString()}').");
            }

            return (json, mode);
        }
    }

    // Validation failures for the tool's own arguments, kept distinct from a
    // parse failure of the user's JSON payload (which has its own message).
    private sealed class JsonFormatException : Exception
    {
        public JsonFormatException(string message) : base(message) { }
    }
}
