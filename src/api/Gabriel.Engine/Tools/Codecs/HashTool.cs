using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Codecs;

// Compute a cryptographic hash digest of text and return it as lowercase hex.
// A pure function of its arguments - no I/O, no dependency, no approval. Text
// is hashed as UTF-8. Exists so the agent can fingerprint/checksum a string
// exactly instead of guessing a digest it cannot actually compute.
public sealed class HashTool : ITool
{
    private const int MaxTextLength = 1_000_000;

    public string Name => "hash";

    public string Description =>
        "Compute a cryptographic hash of text and return it as lowercase hex. " +
        "USE THIS to fingerprint or checksum a string - verifying integrity, " +
        "producing a stable id - instead of guessing a digest. Algorithms: " +
        "sha256 (default), sha512, sha1, md5; treat md5 and sha1 as legacy " +
        "checksums only, not security. Text is hashed as UTF-8. One-way - there " +
        "is no 'unhash'; for reversible encoding use `base64`.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "text": {
              "type": "string",
              "description": "The text to hash (UTF-8). The empty string is allowed and has a well-defined digest."
            },
            "algo": {
              "type": "string",
              "enum": ["md5", "sha1", "sha256", "sha512"],
              "default": "sha256",
              "description": "Hash algorithm. Defaults to sha256. md5/sha1 are for legacy checksums only."
            }
          },
          "required": ["text"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        try
        {
            var (text, algo) = ReadArgs(argumentsJson);
            var bytes = Encoding.UTF8.GetBytes(text);
            var digest = algo switch
            {
                "md5" => MD5.HashData(bytes),
                "sha1" => SHA1.HashData(bytes),
                "sha256" => SHA256.HashData(bytes),
                "sha512" => SHA512.HashData(bytes),
                _ => throw new HashException($"unknown algorithm '{algo}'."), // ReadArgs already validated
            };
            return Task.FromResult($"{algo}: {Convert.ToHexString(digest).ToLowerInvariant()}");
        }
        catch (HashException ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static (string Text, string Algo) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new HashException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("text", out var textEl) || textEl.ValueKind != JsonValueKind.String)
                throw new HashException("'text' is required and must be a string.");
            var text = textEl.GetString() ?? "";
            if (text.Length > MaxTextLength)
                throw new HashException($"'text' is too long (max {MaxTextLength} characters).");

            var algo = "sha256";
            if (root.TryGetProperty("algo", out var algoEl) && algoEl.ValueKind != JsonValueKind.Null)
            {
                if (algoEl.ValueKind != JsonValueKind.String)
                    throw new HashException("'algo' must be a string.");
                algo = algoEl.GetString()!.ToLowerInvariant();
                if (algo is not ("md5" or "sha1" or "sha256" or "sha512"))
                    throw new HashException($"'algo' must be md5, sha1, sha256, or sha512 (got '{algoEl.GetString()}').");
            }

            return (text, algo);
        }
    }

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class HashException : Exception
    {
        public HashException(string message) : base(message) { }
    }
}
