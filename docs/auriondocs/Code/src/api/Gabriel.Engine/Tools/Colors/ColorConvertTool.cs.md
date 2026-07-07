# ColorConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Colors/ColorConvertTool.cs`  
> **Kind:** class

```csharp
public sealed partial class ColorConvertTool : ITool
```


Converts a color value between hex, rgb(), and hsl() notations (alpha preserved). Reach for this tool whenever you need a correct, tested conversion between those textual color formats instead of hand-implementing channel math or ad-hoc parsing; it accepts hex (#rgb, #rgba, #rrggbb, #rrggbbaa), rgb()/rgba(), and hsl()/hsla() inputs and can return a single requested notation or all three.

## Remarks
This class centralizes the fiddly parsing and channel-math for RGB↔HSL and provides a pure, deterministic conversion surface with no I/O. It validates input JSON and the color string (including a 200-character max), normalizes the optional `to` target to one of "hex", "rgb", or "hsl", and preserves any alpha channel found in the source. When executed via ExecuteAsync it catches parsing/validation errors (ColorException) and returns an error message string rather than throwing, and the method returns a completed Task (Task.FromResult) for immediate results.

## Example
```csharp
// Request all three formats
var argsAll = "{ \"value\": \"#ff8800\" }";
var allResult = await tool.ExecuteAsync(argsAll, CancellationToken.None);
// allResult => "hex: #ff8800\nrgb: rgb(255, 136, 0)\nhsl: hsl(30, 100%, 50%)"

// Request a single target format
var argsHex = "{ \"value\": \"rgb(255, 136, 0)\", \"to\": \"hex\" }";
var hexResult = await tool.ExecuteAsync(argsHex, CancellationToken.None);
// hexResult => "rgb(255, 136, 0) → #ff8800"

// Invalid input returns an error string (not an exception) from ExecuteAsync
var bad = "{ \"value\": \"not-a-color\" }";
var err = await tool.ExecuteAsync(bad, CancellationToken.None);
// err => "Error: ..."
```

## Notes
- The JSON argument must include a string property `value`; missing or non-string `value` yields a ColorException (reported as an "Error: ..." string by ExecuteAsync).
- `to` is optional; when present it must be the string "hex", "rgb", or "hsl" (case-insensitive). Any other value triggers validation failure.
- `value` is limited to 200 characters (MaxValueLength) to guard against excessively long inputs.
- Named CSS colors (e.g. "red") are not supported; only hex, rgb(a), and hsl(a) textual forms are recognized.