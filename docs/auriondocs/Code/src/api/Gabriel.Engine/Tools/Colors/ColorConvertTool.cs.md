# ColorConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Colors/ColorConvertTool.cs`  
> **Kind:** class

```csharp
public sealed partial class ColorConvertTool : ITool
```


Convert a color between hex, rgb(), and hsl() notations. A pure function with no I/O or dependencies, it centralizes the parsing and color-model conversions so callers don’t reimplement RGB↔HSL math or hex formatting. It accepts hex forms (#rgb, #rgba, #rrggbb, #rrggbbaa), rgb()/rgba(), or hsl()/hsla(), preserves the alpha channel, and either returns all three representations when 'to' is omitted or a single target notation when 'to' is provided. Not intended for named colors like 'red'.

## Remarks
Color conversion logic is isolated behind a stable API, reducing duplication and subtle bugs across tools that consume color data. Because the function is pure, the same input yields the same output with no side effects, which makes it straightforward to unit-test and reason about. It reports issues as textual errors (e.g., invalid JSON or unsupported 'to') rather than throwing exceptions, which is convenient for scripting and CLI-like usage.

## Notes
- Preserves alpha channel across conversions; an input with alpha (#rrggbbaa or rgba) yields outputs that also reflect alpha.
- Invalid inputs produce an error string starting with 'Error:' rather than throwing; callers should check for this pattern.
- Validation enforces a maximum value length of 200 characters and requires a non-empty 'value' string.