# BaseConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Numbers/BaseConvertTool.cs`  
> **Kind:** class

```csharp
public sealed class BaseConvertTool : ITool
```


Converts a whole (integer) number between numeral bases in the range 2..36. Reach for this tool whenever you need a precise, unbounded base conversion (binary, octal, decimal, hexadecimal or custom bases up to 36) instead of attempting the conversion mentally or with ad-hoc code; it accepts a string input, supports negative values and underscore separators, and returns a single-line, human-readable result string.

## Remarks
This is a pure, self-contained converter intended to avoid subtle errors that occur when doing multi-digit base conversions by hand or with naive code. It accepts the input as JSON (see ParametersJsonSchema on the class), parses the provided value, converts using an unbounded integer representation (the implementation uses BigInteger to avoid overflow), and renders the result using digits 0-9 then A-Z. The tool normalizes input (case-insensitive digits, ignores '_' separators) and produces uppercase digits in the output. It is explicitly for whole numbers only — not for fractional values or general arithmetic.

## Example
```csharp
// Example usage calling the tool's ExecuteAsync. The tool expects a JSON string.
var tool = new BaseConvertTool();
string argsJson = "{\"value\": \"FF\", \"from_base\": 16, \"to_base\": 10}";
string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
// result: "FF (base 16) = 255 (base 10)"
```

## Notes
- The input string may include a leading '-' for negative numbers and may contain '_' separators which are ignored during parsing.
- Input is case-insensitive; output digits use uppercase A-Z for values 10–35.
- 'from_base' defaults to 10 when omitted; 'to_base' is required by the JSON schema. The class enforces a maximum input length (MaxValueLength = 1000).
- ExecuteAsync catches and reports BaseConvertException instances as a user-facing string prefixed with "Error:"; other exception types are not converted and will propagate.