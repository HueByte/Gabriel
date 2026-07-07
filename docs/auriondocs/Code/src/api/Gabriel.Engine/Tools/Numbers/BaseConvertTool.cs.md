# BaseConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Numbers/BaseConvertTool.cs`  
> **Kind:** class

```csharp
public sealed class BaseConvertTool : ITool
```


Convert a whole (integer) number between numeral bases from 2 to 36. Reach for this tool whenever you need a reliable, unbounded base conversion (binary, octal, decimal, hexadecimal or any base up to 36) instead of hand-converting or ad-hoc parsing; it accepts a string input (optionally negative, with '_' separators), parses it exactly using arbitrary-size integers, and renders the result using digits 0-9 then A-Z.

## Remarks
This class is a pure, deterministic conversion utility with no I/O or external dependencies; it exists to avoid the subtle off-by-one and overflow errors people make when converting longer values by hand. Inputs are validated (including a max length) and parsed against the specified source base; the implementation uses an unbounded integer representation so conversions do not overflow for large values. The tool returns a human-readable string of the form "{original} (base {from}) = {result} (base {to})" when successful and returns an "Error: ..." string if the input is invalid (the ExecuteAsync method catches BaseConvertException and returns the message).

## Notes
- Input is case-insensitive; letters are upper-cased for digit lookup and output uses uppercase A-Z for values 10–35.
- Only whole numbers are supported: fractional values or radix points are not handled.
- '_' may be used inside the input as a visual separator and is ignored during parsing; a leading '-' denotes negativity.
- There is a hard limit on input length (1000 characters); overly long inputs produce a descriptive error message rather than throwing.