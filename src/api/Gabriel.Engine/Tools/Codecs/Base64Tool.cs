using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Codecs;

// Encode text to Base64 or decode Base64 back to text. A pure function of its
// arguments - no I/O, no dependency, no approval. Text is treated as UTF-8.
// Exists because hand-rolling Base64 is unreliable, and reading an encoded
// token/payload is a common need mid-conversation.
public sealed class Base64Tool : ITool
{
    private const int MaxTextLength = 100_000;

    public string Name => "base64";

    public string Description =>
        "Encode text to Base64 or decode Base64 back to text. " +
        "USE THIS for Base64 work - decoding a token or payload to read it, or " +
        "encoding text for transport - instead of attempting it by hand, which is " +
        "unreliable. Set url_safe=true when encoding for the URL/filename-safe " +
        "alphabet ('-' and '_' instead of '+' and '/', no '=' padding); decoding " +
        "accepts both standard and URL-safe input automatically. Text is treated " +
        "as UTF-8. NOT for hashing (that's one-way - no tool for it yet) or other " +
        "encodings.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "text": {
              "type": "string",
              "description": "The text to encode, or the Base64 string to decode."
            },
            "op": {
              "type": "string",
              "enum": ["encode", "decode"],
              "description": "encode = text -> Base64. decode = Base64 -> text."
            },
            "url_safe": {
              "type": "boolean",
              "default": false,
              "description": "When encoding, emit the URL/filename-safe alphabet with no padding. Ignored when decoding (both alphabets are accepted)."
            }
          },
          "required": ["text", "op"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        try
        {
            var (text, op, urlSafe) = ReadArgs(argumentsJson);
            return op == "encode"
                ? Task.FromResult($"Encoded: {Encode(text, urlSafe)}")
                : Task.FromResult($"Decoded: {Decode(text)}");
        }
        catch (Base64Exception ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static (string Text, string Op, bool UrlSafe) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new Base64Exception("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("text", out var textEl) || textEl.ValueKind != JsonValueKind.String)
                throw new Base64Exception("'text' is required and must be a string.");
            var text = textEl.GetString() ?? "";
            if (text.Length == 0)
                throw new Base64Exception("'text' cannot be empty.");
            if (text.Length > MaxTextLength)
                throw new Base64Exception($"'text' is too long (max {MaxTextLength} characters).");

            if (!root.TryGetProperty("op", out var opEl) || opEl.ValueKind != JsonValueKind.String)
                throw new Base64Exception("'op' is required and must be 'encode' or 'decode'.");
            var op = opEl.GetString()!.ToLowerInvariant();
            if (op != "encode" && op != "decode")
                throw new Base64Exception($"'op' must be 'encode' or 'decode' (got '{opEl.GetString()}').");

            var urlSafe = false;
            if (root.TryGetProperty("url_safe", out var usEl) && usEl.ValueKind != JsonValueKind.Null)
            {
                if (usEl.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    throw new Base64Exception("'url_safe' must be a boolean.");
                urlSafe = usEl.GetBoolean();
            }

            return (text, op, urlSafe);
        }
    }

    private static string Encode(string text, bool urlSafe)
    {
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        return urlSafe
            ? b64.TrimEnd('=').Replace('+', '-').Replace('/', '_')
            : b64;
    }

    private static string Decode(string text)
    {
        // Accept both alphabets: fold URL-safe chars back to standard, drop any
        // whitespace, then restore '=' padding to a multiple of 4.
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c)) continue;
            sb.Append(c switch { '-' => '+', '_' => '/', _ => c });
        }

        switch (sb.Length % 4)
        {
            case 1: throw new Base64Exception("input is not valid Base64.");
            case 2: sb.Append("=="); break;
            case 3: sb.Append('='); break;
        }

        byte[] bytes;
        try { bytes = Convert.FromBase64String(sb.ToString()); }
        catch (FormatException) { throw new Base64Exception("input is not valid Base64."); }

        return Encoding.UTF8.GetString(bytes);
    }

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class Base64Exception : Exception
    {
        public Base64Exception(string message) : base(message) { }
    }
}
