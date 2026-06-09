Evaluate a mathematical expression and return its numeric result as a string. Use this tool whenever precise numeric evaluation is needed (sums, products, powers, roots, percentages, standard math functions) instead of performing arithmetic mentally or inside an LLM; the tool is a pure evaluator with no I/O and always returns either a formatted result or an "Error: ..." message rather than throwing.

## Remarks
This class wraps a deterministic expression evaluator to remove a common source of agent errors: incorrect manual arithmetic. It implements simple input validation and runtime guards (maximum expression length and parse depth) to avoid pathological inputs causing stack overflows or long pauses. Errors such as syntax problems, unknown names, division by zero, or numeric overflow are reported as "Error: ..." strings so callers can handle failures without catching exceptions.

## Example
```csharp
var tool = new CalculateTool();
string args = "{ \"expression\": \"sqrt(2)\" }";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// result => "sqrt(2) = 1.4142135623730951"
```

## Notes
- The JSON argument must contain an "expression" property of type string; otherwise the tool returns an error.
- Operators are required between tokens (write "2*pi", not "2pi"). Exponentiation is right-associative and unary minus binds tighter than ^ (for example, -3^2 evaluates as -(3^2) == -9).
- The tool never throws for evaluation problems: syntax errors, unknown functions, division by zero, NaN, or overflow are returned as "Error: ..." observations so callers remain stable.