using System.Globalization;
using System.Text.Json;
using static System.Math;

namespace Gabriel.Engine.Tools.Calc;

// Evaluate an arithmetic expression and hand back the numeric result. This
// tool exists because a language model can silently misadd or mismultiply
// multi-digit numbers - delegating the arithmetic to a real evaluator removes
// that whole class of mistake. It's a pure function of its argument string:
// no external dependencies, no I/O, no approval.
//
// Supported grammar (recursive descent, doubles throughout):
//   + - * /        binary arithmetic
//   %              modulo (remainder)
//   ^              exponent, right-associative   (2^3^2 = 2^(3^2) = 512)
//   - +            unary sign                    (-3^2 = -9, 2^-3 = 0.125)
//   ( )            grouping
//   functions      sqrt abs round floor ceil sign min max pow
//                  sin cos tan asin acos atan log ln exp        (trig in radians)
//   constants      pi e tau
//
// Anything outside that grammar - an unknown name, malformed syntax, division
// by zero, a result that overflows to infinity or NaN - comes back as an
// "Error: ..." observation rather than throwing, so the agent loop is never
// destabilised by a bad expression (see tools.md INVARIANTS).
public sealed class CalculateTool : ITool
{
    // Guards against a pathological input (deeply nested parens, a megabyte of
    // digits) turning into a stack overflow or a long pause. Real expressions
    // are tiny; these ceilings are deliberately generous.
    private const int MaxExpressionLength = 1000;
    private const int MaxDepth = 64;

    public string Name => "calculate";

    public string Description =>
        "Evaluate a mathematical expression and return the exact numeric result. " +
        "USE THIS for any arithmetic the user needs - sums, products, percentages, " +
        "powers, roots - instead of computing it in your head, which is error-prone " +
        "for multi-digit numbers. " +
        "Supports + - * /, modulo %, exponent ^, parentheses, unary minus, the " +
        "functions sqrt/abs/round/floor/ceil/sign/min/max/pow/sin/cos/tan/asin/acos/" +
        "atan/log/ln/exp, and the constants pi, e, tau. Trig functions take radians. " +
        "NOT for symbolic algebra, solving equations, calculus, or date/time math " +
        "(use get_current_time for the current time).";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "expression": {
              "type": "string",
              "description": "The expression to evaluate, e.g. \"(1234 * 9) / 7\", \"sqrt(2)\", \"min(3, 8) ^ 2\". Operators are required between terms (write 2*pi, not 2pi). Trig functions take radians."
            }
          },
          "required": ["expression"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        string expression;
        try
        {
            expression = ReadExpressionArg(argumentsJson);
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }

        try
        {
            var value = new Evaluator(expression).Evaluate();
            if (double.IsNaN(value))
                return Task.FromResult("Error: the result is undefined (not a number).");
            if (double.IsInfinity(value))
                return Task.FromResult("Error: the result is too large to represent (overflowed to infinity).");
            return Task.FromResult($"{expression.Trim()} = {Format(value)}");
        }
        catch (CalculateException ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static string ReadExpressionArg(string argumentsJson)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        if (!doc.RootElement.TryGetProperty("expression", out var el) || el.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException("'expression' is required and must be a string.");
        var expr = el.GetString();
        if (string.IsNullOrWhiteSpace(expr))
            throw new InvalidOperationException("'expression' cannot be empty.");
        if (expr.Length > MaxExpressionLength)
            throw new InvalidOperationException($"'expression' is too long (max {MaxExpressionLength} characters).");
        return expr;
    }

    // Friendly number formatting: integral results print without a decimal
    // point; everything else trims to 12 fractional digits, which folds away
    // binary-float dust like 0.1 + 0.2 = 0.30000000000000004 into "0.3".
    private static string Format(double d)
    {
        if (d == Floor(d) && Abs(d) < 1e15)
            return ((long)d).ToString(CultureInfo.InvariantCulture);
        return d.ToString("0.############", CultureInfo.InvariantCulture);
    }

    // Single-pass recursive-descent evaluator over a position cursor. Each
    // grammar rule is one method; precedence falls out of the call chain
    // (expr -> term -> unary -> power -> primary), lowest-binding first.
    private sealed class Evaluator
    {
        private readonly string _s;
        private int _pos;
        private int _depth;

        public Evaluator(string s) => _s = s;

        public double Evaluate()
        {
            var value = ParseExpr();
            SkipWs();
            if (_pos != _s.Length)
                throw new CalculateException($"unexpected '{_s[_pos]}' at position {_pos}.");
            return value;
        }

        // expr := term (('+' | '-') term)*
        private double ParseExpr()
        {
            var value = ParseTerm();
            while (true)
            {
                SkipWs();
                var c = Peek();
                if (c == '+') { _pos++; value += ParseTerm(); }
                else if (c == '-') { _pos++; value -= ParseTerm(); }
                else break;
            }
            return value;
        }

        // term := unary (('*' | '/' | '%') unary)*
        private double ParseTerm()
        {
            var value = ParseUnary();
            while (true)
            {
                SkipWs();
                var c = Peek();
                if (c == '*') { _pos++; value *= ParseUnary(); }
                else if (c == '/')
                {
                    _pos++;
                    var divisor = ParseUnary();
                    if (divisor == 0) throw new CalculateException("division by zero.");
                    value /= divisor;
                }
                else if (c == '%')
                {
                    _pos++;
                    var divisor = ParseUnary();
                    if (divisor == 0) throw new CalculateException("modulo by zero.");
                    value %= divisor;
                }
                else break;
            }
            return value;
        }

        // unary := ('+' | '-') unary | power
        private double ParseUnary()
        {
            SkipWs();
            var c = Peek();
            if (c == '+') { _pos++; return ParseUnary(); }
            if (c == '-') { _pos++; return -ParseUnary(); }
            return ParsePower();
        }

        // power := primary ('^' unary)?
        // Right-associative, and the exponent is a unary so 2^-3 parses.
        private double ParsePower()
        {
            var baseValue = ParsePrimary();
            SkipWs();
            if (Peek() == '^')
            {
                _pos++;
                return Pow(baseValue, ParseUnary());
            }
            return baseValue;
        }

        // primary := number | identifier funccall? | '(' expr ')'
        private double ParsePrimary()
        {
            if (++_depth > MaxDepth) throw new CalculateException("expression is nested too deeply.");
            try
            {
                SkipWs();
                var c = Peek();
                if (c == '\0') throw new CalculateException("unexpected end of expression.");
                if (c == '(')
                {
                    _pos++;
                    var value = ParseExpr();
                    Expect(')');
                    return value;
                }
                if (char.IsDigit(c) || c == '.') return ParseNumber();
                if (char.IsLetter(c) || c == '_') return ParseIdentifier();
                throw new CalculateException($"unexpected '{c}' at position {_pos}.");
            }
            finally { _depth--; }
        }

        private double ParseNumber()
        {
            var start = _pos;
            while (_pos < _s.Length && (char.IsDigit(_s[_pos]) || _s[_pos] == '.')) _pos++;

            // Optional scientific exponent: e/E [+/-] digits. If what follows the
            // 'e' isn't a valid exponent we back off, so a trailing constant like
            // the 'e' in "3*e" is left for the identifier parser.
            if (_pos < _s.Length && (_s[_pos] == 'e' || _s[_pos] == 'E'))
            {
                var mark = _pos;
                _pos++;
                if (_pos < _s.Length && (_s[_pos] == '+' || _s[_pos] == '-')) _pos++;
                if (_pos < _s.Length && char.IsDigit(_s[_pos]))
                    while (_pos < _s.Length && char.IsDigit(_s[_pos])) _pos++;
                else
                    _pos = mark;
            }

            var text = _s[start.._pos];
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new CalculateException($"'{text}' is not a valid number.");
            return value;
        }

        private double ParseIdentifier()
        {
            var start = _pos;
            while (_pos < _s.Length && (char.IsLetterOrDigit(_s[_pos]) || _s[_pos] == '_')) _pos++;
            var name = _s[start.._pos].ToLowerInvariant();

            SkipWs();
            if (Peek() == '(')
            {
                _pos++;
                var args = ParseArgs();
                Expect(')');
                return ApplyFunction(name, args);
            }

            return name switch
            {
                "pi" => PI,
                "e" => E,
                "tau" => Tau,
                _ => throw new CalculateException($"unknown name '{name}' (write 2*pi, not 2pi)."),
            };
        }

        private List<double> ParseArgs()
        {
            var args = new List<double>();
            SkipWs();
            if (Peek() == ')') return args; // zero-arg call, e.g. a bad pi()
            args.Add(ParseExpr());
            while (true)
            {
                SkipWs();
                if (Peek() == ',') { _pos++; args.Add(ParseExpr()); }
                else break;
            }
            return args;
        }

        private static double ApplyFunction(string name, List<double> a)
        {
            double Unary() => a.Count == 1
                ? a[0]
                : throw new CalculateException($"{name}() takes 1 argument, got {a.Count}.");

            switch (name)
            {
                case "sqrt":
                    var x = Unary();
                    if (x < 0) throw new CalculateException("sqrt of a negative number.");
                    return Sqrt(x);
                case "abs": return Abs(Unary());
                case "floor": return Floor(Unary());
                case "ceil": return Ceiling(Unary());
                case "sign": return Sign(Unary());
                case "sin": return Sin(Unary());
                case "cos": return Cos(Unary());
                case "tan": return Tan(Unary());
                case "asin": return Asin(Unary());
                case "acos": return Acos(Unary());
                case "atan": return Atan(Unary());
                case "exp": return Exp(Unary());
                case "ln":
                    var lnArg = Unary();
                    if (lnArg <= 0) throw new CalculateException("ln of a non-positive number.");
                    return Log(lnArg);
                case "round":
                    if (a.Count == 1) return Round(a[0], MidpointRounding.AwayFromZero);
                    if (a.Count == 2)
                    {
                        var digits = (int)a[1];
                        if (digits < 0 || digits > 15)
                            throw new CalculateException("round() digit count must be between 0 and 15.");
                        return Round(a[0], digits, MidpointRounding.AwayFromZero);
                    }
                    throw new CalculateException($"round() takes 1 or 2 arguments, got {a.Count}.");
                case "log":
                    if (a.Count is 1 or 2)
                    {
                        if (a[0] <= 0) throw new CalculateException("log of a non-positive number.");
                        return a.Count == 1 ? Log10(a[0]) : Log(a[0], a[1]);
                    }
                    throw new CalculateException($"log() takes 1 or 2 arguments, got {a.Count}.");
                case "pow":
                    if (a.Count == 2) return Pow(a[0], a[1]);
                    throw new CalculateException($"pow() takes 2 arguments, got {a.Count}.");
                case "min":
                    if (a.Count == 0) throw new CalculateException("min() needs at least 1 argument.");
                    return a.Min();
                case "max":
                    if (a.Count == 0) throw new CalculateException("max() needs at least 1 argument.");
                    return a.Max();
                default:
                    throw new CalculateException($"unknown function '{name}'.");
            }
        }

        private void Expect(char c)
        {
            SkipWs();
            if (Peek() != c) throw new CalculateException($"expected '{c}' at position {_pos}.");
            _pos++;
        }

        private char Peek() => _pos < _s.Length ? _s[_pos] : '\0';

        private void SkipWs()
        {
            while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos])) _pos++;
        }
    }

    // Distinguishes "the expression was bad" (reported to the agent as an
    // observation) from a genuine bug (which should surface normally).
    private sealed class CalculateException : Exception
    {
        public CalculateException(string message) : base(message) { }
    }
}
