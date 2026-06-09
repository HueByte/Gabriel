Evaluates a mathematical expression provided as a string and returns the numeric result (or an "Error: ..." message) as a string. Use this tool whenever you need reliable numeric computation — sums, products, percentages, powers, roots, and common math functions — instead of attempting to compute values in your head or relying on a language model's raw text arithmetic.

## Remarks
This class exists to offload numeric evaluation to a dedicated, deterministic evaluator so the agent loop never fails due to trivial arithmetic mistakes. It is a pure, side-effect-free evaluator: no I/O, no external dependencies beyond parsing the argument JSON, and all error conditions are converted into "Error: ..." results rather than thrown exceptions (this prevents destabilising callers that expect a stable tool contract). The implementation enforces input size and recursion-depth limits to avoid pathological inputs.

## Example
```csharp
var tool = new CalculateTool();
string args = "{ \"expression\": \"(1234 * 9) / 7\" }";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// result -> "(1234 * 9) / 7 = 1586.5714285714287"  (actual formatting may vary)
```

## Notes
- Input limits: expressions longer than ~1000 characters or deeper than ~64 recursion levels are rejected to avoid stack/CPU problems.
- Errors such as parse failures, unknown names, division by zero, overflow to infinity, or NaN return an "Error: ..." string rather than throwing.
- The evaluator uses double-precision floating point: expect normal floating-point rounding/precision behavior (not arbitrary-precision rational arithmetic).
- Trigonometric functions take radians, and functions/constants supported include: sqrt, abs, round, floor, ceil, sign, min, max, pow, sin, cos, tan, asin, acos, atan, log, ln, exp and constants pi, e, tau.
- Operators must be explicit (write "2*pi", not "2pi"). Unary minus binds so that -3^2 = -9 (i.e. exponentiation is evaluated before unary sign), and exponentiation is right-associative (2^3^2 = 512).