using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Tools.Numbers;

// Convert a whole number between numeral bases (2-36). A pure function of its
// arguments - no I/O, no dependency, no approval. It exists because base
// conversion past a couple of digits is exactly the mechanical work a language
// model gets subtly wrong; backing it with BigInteger makes the result exact
// and unbounded (a 40-digit hex value converts without overflow).
public sealed class BaseConvertTool : ITool
{
    private const int MaxValueLength = 1000;

    // Index in this string is the digit's value; covers every base up to 36.
    // Output uses these (uppercase); input is upper-cased before lookup so it's
    // case-insensitive.
    private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string Name => "base_convert";

    public string Description =>
        "Convert a whole number between numeral bases - binary, octal, decimal, " +
        "hexadecimal, or any base from 2 to 36. " +
        "USE THIS whenever a base conversion is needed (reading a hex value as " +
        "decimal, turning a binary literal into a number, and so on) instead of " +
        "doing it in your head, which is error-prone past a couple of digits. " +
        "Digits are 0-9 then A-Z for 10-35 (input is case-insensitive); a leading " +
        "'-' is allowed and '_' may be used as a separator. Whole numbers only - " +
        "NOT for fractions, and NOT for arithmetic (use `calculate` for math).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "value": {
              "type": "string",
              "description": "The number to convert, written in from_base. May be negative; '_' separators are ignored. Digits 0-9 then a-z for 10-35, case-insensitive. E.g. \"FF\", \"-1010\", \"7\"."
            },
            "from_base": {
              "type": "integer",
              "minimum": 2,
              "maximum": 36,
              "default": 10,
              "description": "Base the input is written in (2-36). Defaults to 10."
            },
            "to_base": {
              "type": "integer",
              "minimum": 2,
              "maximum": 36,
              "description": "Base to convert the number into (2-36)."
            }
          },
          "required": ["value", "to_base"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        try
        {
            var (value, fromBase, toBase) = ReadArgs(argumentsJson);
            var magnitude = Parse(value, fromBase, out var negative);
            var rendered = Render(magnitude, toBase, negative);
            return Task.FromResult($"{value.Trim()} (base {fromBase}) = {rendered} (base {toBase})");
        }
        catch (BaseConvertException ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static (string Value, int FromBase, int ToBase) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new BaseConvertException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("value", out var valueEl) || valueEl.ValueKind != JsonValueKind.String)
                throw new BaseConvertException("'value' is required and must be a string.");
            var value = valueEl.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new BaseConvertException("'value' cannot be empty.");
            if (value.Length > MaxValueLength)
                throw new BaseConvertException($"'value' is too long (max {MaxValueLength} characters).");

            var fromBase = ReadBase(root, "from_base", 10);
            var toBase = ReadBase(root, "to_base", null);
            return (value, fromBase, toBase);
        }
    }

    // Reads a base property. Pass a default for optional bases; pass null to
    // require it. Enforces the 2-36 range that the digit alphabet supports.
    private static int ReadBase(JsonElement root, string name, int? defaultValue)
    {
        if (!root.TryGetProperty(name, out var el) || el.ValueKind == JsonValueKind.Null)
        {
            if (defaultValue is int d) return d;
            throw new BaseConvertException($"'{name}' is required.");
        }
        if (el.ValueKind != JsonValueKind.Number || !el.TryGetInt32(out var b))
            throw new BaseConvertException($"'{name}' must be an integer.");
        if (b < 2 || b > 36)
            throw new BaseConvertException($"'{name}' must be between 2 and 36 (got {b}).");
        return b;
    }

    // Parses the input string in fromBase into a non-negative magnitude, with
    // the sign reported separately so Render doesn't have to special-case it.
    private static BigInteger Parse(string value, int fromBase, out bool negative)
    {
        var s = value.Trim();
        var i = 0;
        negative = false;
        if (s.Length > 0 && (s[0] == '+' || s[0] == '-'))
        {
            negative = s[0] == '-';
            i = 1;
        }

        BigInteger result = BigInteger.Zero;
        BigInteger radix = fromBase;
        var sawDigit = false;
        for (; i < s.Length; i++)
        {
            if (s[i] == '_') continue; // grouping separator
            var digit = Digits.IndexOf(char.ToUpperInvariant(s[i]));
            if (digit < 0 || digit >= fromBase)
                throw new BaseConvertException($"'{s[i]}' is not a valid digit in base {fromBase}.");
            result = result * radix + digit;
            sawDigit = true;
        }

        if (!sawDigit)
            throw new BaseConvertException("'value' has no digits.");
        return result;
    }

    private static string Render(BigInteger magnitude, int toBase, bool negative)
    {
        if (magnitude.IsZero) return "0"; // sign on zero is meaningless

        var sb = new StringBuilder();
        BigInteger radix = toBase;
        var n = magnitude;
        while (n > 0)
        {
            sb.Insert(0, Digits[(int)(n % radix)]);
            n /= radix;
        }
        if (negative) sb.Insert(0, '-');
        return sb.ToString();
    }

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class BaseConvertException : Exception
    {
        public BaseConvertException(string message) : base(message) { }
    }
}
