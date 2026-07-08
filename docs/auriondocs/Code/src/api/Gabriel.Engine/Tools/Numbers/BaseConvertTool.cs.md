# BaseConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Numbers/BaseConvertTool.cs`  
> **Kind:** class

```csharp
public sealed class BaseConvertTool : ITool
```


Converts a whole (integer) number from one radix to another (bases 2–36). Reach for this tool whenever you need an exact, unbounded base conversion (binary, octal, decimal, hexadecimal or any base 2..36) instead of attempting mental conversion or ad-hoc parsing; it accepts a string value (optionally negative, with '_' separators) and returns a human-readable result string.

## Remarks
This class implements a pure, deterministic conversion utility (no I/O or external dependencies) intended to avoid the common off-by-one and overflow mistakes that arise when converting manually or with limited-width numeric types. The tool validates input (including a maximum input length), treats digit characters case-insensitively, and formats a concise textual result; errors are surfaced as user-facing messages rather than exceptions escaping the ExecuteAsync call.

## Example
```csharp
var tool = new BaseConvertTool();
string args = "{ \"value\": \"FF\", \"from_base\": 16, \"to_base\": 10 }";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// result => "FF (base 16) = 255 (base 10)"
```

## Notes
- '_' characters in the input are treated as digit separators and ignored; input letters are case-insensitive and a leading '-' denotes a negative number.
- The tool accepts only whole (integer) numbers — it does not handle fractional parts or perform general arithmetic.
- If the input is invalid the tool returns a string beginning with "Error: " containing the validation message (the method catches BaseConvertException and converts it into this user-visible string).