# CalculateTool

> **File:** `src/api/Gabriel.Engine/Tools/Calc/CalculateTool.cs`  
> **Kind:** class

```csharp
public sealed class CalculateTool : ITool
```


Evaluate a mathematical expression and return its numeric result as a string. Reach for this tool whenever the agent needs to perform arithmetic (sums, products, powers, roots, trig, common functions) instead of attempting to compute multi-digit or complex numeric results internally — the implementation delegates evaluation to a dedicated parser/evaluator so numeric mistakes are avoided and errors are reported as stable "Error: ..." observations rather than thrown exceptions.

## Remarks
This tool is a small, self-contained evaluator that enforces a restricted arithmetic grammar (binary + - * /, modulo %, exponent ^ with right-associativity, unary sign, parentheses, a set of standard math functions and constants). It exists to remove a class of mistakes LLMs commonly make when doing arithmetic: feed the raw expression as a string and the tool returns either a formatted "expression = value" result or a stable error message. The implementation is intentionally pure (no I/O, no external state) and defensive: it enforces maximum expression length and recursion depth, and maps parse/evaluation problems, division-by-zero, overflow and NaN results into user-visible error strings rather than allowing exceptions to bubble up.

## Example
```csharp
// Typical usage
var tool = new CalculateTool();
string args = "{\"expression\": \"(1234 * 9) / 7\"}";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// result -> "(1234 * 9) / 7 = 1586"

// Example with a function and constants
args = "{\"expression\": \"sqrt(2) * pi\"}";
result = await tool.ExecuteAsync(args, CancellationToken.None);
// result -> "sqrt(2) * pi = 4.442882938158366"

// On errors (syntax, unknown name, division by zero, overflow) the tool returns
// a stable Error:... string instead of throwing.
```

## Notes
- Operators must be explicit between tokens: write "2*pi" or "2 * pi", not "2pi".
- Exponentiation is right-associative: 2^3^2 is parsed as 2^(3^2) = 512.
- Unary sign binds as in the implementation examples: "-3^2" yields -9 (unary minus applied after exponent), and "2^-3" yields 0.125.
- Trigonometric functions use radians (sin, cos, tan, asin, acos, atan).
- The tool enforces limits (MaxExpressionLength and MaxDepth) to avoid pathological inputs; overly long or deeply nested expressions will produce an error.
- Results that are NaN or Infinity are returned as error strings (e.g. division by zero or overflow), not as thrown exceptions.
- This is for numeric evaluation only — not suitable for symbolic algebra, equation solving, calculus, or date/time arithmetic.