using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gabriel.Engine.Tools.Strings;

// Reshape the case or form of a string. A pure function of its arguments - no
// I/O, no dependency. Exists so the agent can convert an identifier or label
// between conventions exactly, instead of hand-editing it character by
// character. Case ops are word-boundary aware: spaces, underscores, hyphens,
// and camelCase humps all split into the same word list, so any input form
// converts to any output form.
public sealed partial class TextTransformTool : ITool
{
    private const int MaxTextLength = 100_000;

    public string Name => "text_transform";

    public string Description =>
        "Transform the case or shape of text. Ops: upper, lower, title, sentence; " +
        "the programmer cases snake, camel, pascal, kebab; slug (a URL slug); and " +
        "trim (trim ends + collapse runs of whitespace to one space). " +
        "USE THIS to reshape an identifier or label instead of editing it by hand. " +
        "The case ops understand word boundaries in mixed input - spaces, '_', '-', " +
        "and camelCase humps all split into words - so any form converts to any " +
        "other. 'slug' is ASCII-only (other characters are dropped); the rest keep " +
        "Unicode letters. NOT for measuring text (use `text_stats`).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "text": {
              "type": "string",
              "description": "The text to transform."
            },
            "op": {
              "type": "string",
              "enum": ["upper", "lower", "title", "sentence", "snake", "camel", "pascal", "kebab", "slug", "trim"],
              "description": "upper/lower/title/sentence case; snake_case; camelCase; PascalCase; kebab-case; slug (URL slug); trim (trim + collapse whitespace)."
            }
          },
          "required": ["text", "op"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        try
        {
            var (text, op) = ReadArgs(argumentsJson);
            return Task.FromResult(Transform(text, op));
        }
        catch (TextTransformException ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static string Transform(string text, string op) => op switch
    {
        "upper" => text.ToUpperInvariant(),
        "lower" => text.ToLowerInvariant(),
        "trim" => Whitespace().Replace(text.Trim(), " "),
        "title" => System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant()),
        "sentence" => SentenceCase(text),
        "snake" => string.Join('_', Tokenize(text).Select(t => t.ToLowerInvariant())),
        "kebab" => string.Join('-', Tokenize(text).Select(t => t.ToLowerInvariant())),
        "camel" => CamelCase(Tokenize(text)),
        "pascal" => string.Concat(Tokenize(text).Select(Capitalize)),
        "slug" => SlugStrip().Replace(text.ToLowerInvariant(), "-").Trim('-'),
        _ => throw new TextTransformException($"unknown op '{op}'."), // ReadArgs already validated
    };

    private static (string Text, string Op) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new TextTransformException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("text", out var textEl) || textEl.ValueKind != JsonValueKind.String)
                throw new TextTransformException("'text' is required and must be a string.");
            var text = textEl.GetString() ?? "";
            if (text.Length == 0)
                throw new TextTransformException("'text' cannot be empty.");
            if (text.Length > MaxTextLength)
                throw new TextTransformException($"'text' is too long (max {MaxTextLength} characters).");

            if (!root.TryGetProperty("op", out var opEl) || opEl.ValueKind != JsonValueKind.String)
                throw new TextTransformException("'op' is required and must be a string.");
            var op = opEl.GetString()!.ToLowerInvariant();
            if (op is not ("upper" or "lower" or "title" or "sentence" or "snake"
                or "camel" or "pascal" or "kebab" or "slug" or "trim"))
            {
                throw new TextTransformException(
                    $"'op' must be one of upper, lower, title, sentence, snake, camel, pascal, kebab, slug, trim (got '{opEl.GetString()}').");
            }

            return (text, op);
        }
    }

    // Splits arbitrary input into words: separators become breaks, and camelCase
    // / acronym humps are split too (so "HTMLParser userID" -> HTML, Parser, user, ID).
    private static List<string> Tokenize(string s)
    {
        var spaced = Separators().Replace(s, " ");
        spaced = CamelBoundary().Replace(spaced, "$1 $2");
        spaced = AcronymBoundary().Replace(spaced, "$1 $2");
        return spaced.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static string CamelCase(List<string> tokens)
    {
        if (tokens.Count == 0) return "";
        var sb = new StringBuilder(tokens[0].ToLowerInvariant());
        for (var i = 1; i < tokens.Count; i++) sb.Append(Capitalize(tokens[i]));
        return sb.ToString();
    }

    private static string Capitalize(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();

    private static string SentenceCase(string text)
    {
        var lower = text.ToLowerInvariant();
        for (var i = 0; i < lower.Length; i++)
            if (char.IsLetter(lower[i]))
                return lower[..i] + char.ToUpperInvariant(lower[i]) + lower[(i + 1)..];
        return lower;
    }

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex Separators();

    // lower-or-digit followed by an upper: the "camelCase" hump.
    [GeneratedRegex(@"(\p{Ll}|\p{N})(\p{Lu})")]
    private static partial Regex CamelBoundary();

    // run of uppers followed by upper+lower: the "HTMLParser" -> "HTML Parser" seam.
    [GeneratedRegex(@"(\p{Lu}+)(\p{Lu}\p{Ll})")]
    private static partial Regex AcronymBoundary();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugStrip();

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class TextTransformException : Exception
    {
        public TextTransformException(string message) : base(message) { }
    }
}
