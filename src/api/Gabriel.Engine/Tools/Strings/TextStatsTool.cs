using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gabriel.Engine.Tools.Strings;

// Measure a block of text - counts, reading time, rough token estimate. A pure
// function of its argument, no I/O or dependency. Exists so "how long is this?"
// has an exact answer instead of an eyeballed guess. Character counts are
// code-point based so emoji and CJK count the way a human would expect.
public sealed partial class TextStatsTool : ITool
{
    private const int MaxTextLength = 1_000_000;
    private const int WordsPerMinute = 200; // typical adult silent-reading pace

    public string Name => "text_stats";

    public string Description =>
        "Measure a block of text: character, word, line, sentence, and paragraph " +
        "counts, plus an estimated reading time and a rough token count. " +
        "USE THIS to answer 'how long is this?' about some text instead of " +
        "eyeballing it. Characters are counted as Unicode code points, so emoji " +
        "and CJK count correctly; sentence and paragraph counts are heuristic. " +
        "NOT for hashing (use `hash`) or arithmetic (use `calculate`).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "text": {
              "type": "string",
              "description": "The text to measure."
            }
          },
          "required": ["text"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string text;
        try { text = ReadText(argumentsJson); }
        catch (TextStatsException ex) { return Task.FromResult($"Error: {ex.Message}"); }

        // One pass over Unicode scalar values: total chars and chars-sans-space.
        var chars = 0;
        var nonWhitespace = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            chars++;
            if (!Rune.IsWhiteSpace(rune)) nonWhitespace++;
        }

        var words = Words().Count(text);

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n').Length;

        var sentences = Sentences().Count(text);
        if (sentences == 0 && words > 0) sentences = 1; // unterminated text is still one sentence

        var paragraphs = CountParagraphs(normalized);
        if (paragraphs == 0 && words > 0) paragraphs = 1;

        var sb = new StringBuilder();
        sb.Append("Characters: ").Append(chars).Append(" (").Append(nonWhitespace).AppendLine(" excluding whitespace)");
        sb.Append("Words: ").Append(words).AppendLine();
        sb.Append("Lines: ").Append(lines).AppendLine();
        sb.Append("Sentences: ").Append(sentences).AppendLine(" (approx)");
        sb.Append("Paragraphs: ").Append(paragraphs).AppendLine();
        sb.Append("Reading time: ").AppendLine(FormatReadingTime(words));
        sb.Append("Est. tokens: ~").Append(Math.Max(1, (chars + 3) / 4)); // ceil(chars/4), the repo's naive estimate
        return Task.FromResult(sb.ToString());
    }

    private static string ReadText(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new TextStatsException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("text", out var el) || el.ValueKind != JsonValueKind.String)
                throw new TextStatsException("'text' is required and must be a string.");
            var text = el.GetString() ?? "";
            if (text.Length == 0)
                throw new TextStatsException("'text' cannot be empty.");
            if (text.Length > MaxTextLength)
                throw new TextStatsException($"'text' is too long (max {MaxTextLength} characters).");
            return text;
        }
    }

    private static int CountParagraphs(string normalized)
    {
        var count = 0;
        foreach (var block in ParagraphBreak().Split(normalized))
            if (block.Trim().Length > 0) count++;
        return count;
    }

    private static string FormatReadingTime(int words)
    {
        if (words == 0) return "0s";
        var seconds = (int)Math.Ceiling(words * 60.0 / WordsPerMinute);
        if (seconds < 60) return $"~{seconds}s";
        var minutes = seconds / 60;
        var rem = seconds % 60;
        return rem == 0 ? $"~{minutes}m" : $"~{minutes}m {rem}s";
    }

    // A "word" is any run of non-whitespace characters.
    [GeneratedRegex(@"\S+")]
    private static partial Regex Words();

    // Heuristic sentence terminator: a run of . ! ? followed by whitespace or
    // end-of-text. Abbreviations and ellipses will skew this - hence "approx".
    [GeneratedRegex(@"[.!?]+(?=\s|$)")]
    private static partial Regex Sentences();

    // A paragraph break is a blank line (newline, optional spaces/tabs, newline).
    [GeneratedRegex(@"\n[ \t]*\n")]
    private static partial Regex ParagraphBreak();

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class TextStatsException : Exception
    {
        public TextStatsException(string message) : base(message) { }
    }
}
