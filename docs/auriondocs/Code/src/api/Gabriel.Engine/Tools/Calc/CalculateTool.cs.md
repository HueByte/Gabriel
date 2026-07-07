# CalculateTool

> **File:** `src/api/Gabriel.Engine/Tools/Calc/CalculateTool.cs`  
> **Kind:** class

```csharp
public sealed class CalculateTool : ITool
```


Evaluate a mathematical expression and return a deterministic, formatted numeric result (e.g. "(1+2) = 3"). Reach for this tool whenever the agent needs to perform numeric computation—sums, products, percentages, powers, roots, trig—instead of attempting the arithmetic itself. The tool accepts a single JSON argument with an "expression" string and returns either the computed result or an "Error: ..." observation for any problem (syntax error, unknown name, division by zero, overflow, etc.).

## Remarks
CalculateTool is a thin, safe wrapper around the internal Evaluator parser/executor. It enforces input size and nesting limits to avoid pathological inputs (MaxExpressionLength and MaxDepth), normalizes error handling by returning human-readable "Error: ..." messages instead of throwing, and formats successful results as "<expression> = <value>". This design keeps callers (particularly agent loops) stable: malformed or problematic expressions never throw uncaught exceptions and never perform I/O or depend on external state.

## Notes
- Operators must be explicit between terms (write `2*pi`, not `2pi`).
- Exponentiation is right-associative and unary sign binds as in the source comments (e.g. `-3^2` is interpreted as `-(3^2)` and yields `-9`; `2^-3` yields `0.125`).
- Trigonometric functions use radians. Division by zero, NaN, or Infinity are returned as "Error: ..." messages rather than throwing; very large or deeply nested inputs may be rejected by the built-in length/depth limits.