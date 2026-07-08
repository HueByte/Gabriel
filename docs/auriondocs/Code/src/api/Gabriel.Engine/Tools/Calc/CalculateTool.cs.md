# CalculateTool

> **File:** `src/api/Gabriel.Engine/Tools/Calc/CalculateTool.cs`  
> **Kind:** class

```csharp
public sealed class CalculateTool : ITool
```


Evaluate a mathematical expression and return a numeric result as a plain string. Reach for CalculateTool whenever an agent needs reliable numeric evaluation of an arithmetic expression (sums, products, powers, roots, trig, common functions) instead of trying to compute the value in-protocol; the tool parses and evaluates the expression and returns either "<expression> = <value>" or an "Error: ..." observation so callers never receive an exception.

## Remarks
CalculateTool is a thin, self-contained evaluator wrapper that delegates parsing and numeric computation to the internal Evaluator class and enforces a safe, stable contract for callers: no I/O, no thrown exceptions for bad input, and defensive limits to avoid pathological inputs. Errors such as malformed syntax, unknown names, division by zero, or numeric overflow are converted into human-readable "Error: ..." strings rather than propagated as exceptions, keeping calling agent loops stable. The tool also enforces expression limits (MaxExpressionLength = 1000 and MaxDepth = 64) to guard against deep recursion or extremely large inputs.

## Example
```csharp
// Call the tool with a JSON argument containing the expression string.
var tool = new CalculateTool();
var result = await tool.ExecuteAsync("{\"expression\": \"(1234 * 9) / 7\"}", CancellationToken.None);
// result => "(1234 * 9) / 7 = 1586"
```

## Notes
- The JSON argument must contain an "expression" property whose value is a string; otherwise the tool returns an error.
- Operators are required between terms (write `2*pi`, not `2pi`).
- Trigonometric functions use radians.
- Unary sign and precedence follow the documented rules (exponentiation is right-associative; unary minus binds as shown: `-3^2 = -9`).
- If evaluation produces NaN or Infinity the tool returns a corresponding "Error: ..." message rather than a numeric value.
