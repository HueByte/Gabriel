# ColorConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Colors/ColorConvertTool.cs`  
> **Kind:** class

```csharp
public sealed partial class ColorConvertTool : ITool
```


Convert a color value between hex, rgb(), and hsl() notations. Use this tool when you need a reliable, well‑tested conversion (including preserving an alpha channel) instead of reimplementing RGB↔HSL math by hand. Provide a JSON object with a required "value" string (hex, rgb()/rgba(), or hsl()/hsla()) and an optional "to" of "hex", "rgb", or "hsl"; omit "to" to get all three representations.

## Remarks
This class is a pure, self-contained converter: no I/O or external dependencies. It centralizes input validation (required string, max length) and the fiddly channel math (including alpha) so callers can rely on consistent formatting and error text. ExecuteAsync catches conversion errors and returns them as user-facing messages rather than throwing, making the tool safe to call from orchestrators that expect string results.

## Notes
- The JSON argument must include a non-empty string "value"; inputs longer than 200 characters are rejected.
- The optional "to" is validated case-insensitively and must be exactly "hex", "rgb", or "hsl"; other values produce a friendly error.
- Named CSS colors (e.g., "red") are not supported; accepted inputs are hex, rgb()/rgba(), or hsl()/hsla().
- Conversion errors are returned as strings prefixed with "Error: " (ExecuteAsync does not propagate ColorException to the caller).