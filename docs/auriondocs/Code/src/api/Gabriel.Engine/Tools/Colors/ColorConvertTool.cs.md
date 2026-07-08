# ColorConvertTool

> **File:** `src/api/Gabriel.Engine/Tools/Colors/ColorConvertTool.cs`  
> **Kind:** class

```csharp
public sealed partial class ColorConvertTool : ITool
```


Converts a single color value between hex, rgb()/rgba(), and hsl()/hsla() notations and (optionally) returns just one target format or all three. Use this tool whenever you need a reliable channel-aware conversion or to preserve an alpha channel instead of implementing RGB↔HSL math yourself.

## Remarks
This class exists to centralize the fiddly, easy-to-get-wrong math involved in color-channel conversions and to provide consistent validation and error messages for input strings. It is a pure, stateless converter: no I/O, no external dependencies, and it preserves alpha when present. The tool accepts short/long hex forms, rgb()/rgba(), and hsl()/hsla(), and can either return a single requested notation or a block containing all three.

## Example
```csharp
// Create the tool and call ExecuteAsync with a JSON argument string.
var tool = new ColorConvertTool();

// Ask for all three notations (omit "to"): result is a newline-separated block
// with lines that start with "hex:", "rgb:", and "hsl:".
var allFormats = await tool.ExecuteAsync("{\"value\": \"#03f\"}", CancellationToken.None);
Console.WriteLine(allFormats);

// Request a single target notation. The returned string is of the form
// "<original trimmed value> → <converted value>" (alpha is preserved if present).
var single = await tool.ExecuteAsync("{\"value\": \"rgba(255, 0, 0, 0.5)\", \"to\": \"hex\"}", CancellationToken.None);
Console.WriteLine(single);
```

## Notes
- Named CSS color keywords (e.g. "red") are not supported; input must be hex, rgb()/rgba(), or hsl()/hsla().
- Input validation errors are returned as a string prefixed with "Error: " rather than thrown to the caller (ExecuteAsync catches ColorException and returns the message).
- The 'value' property must be a non-empty string no longer than 200 characters; the optional 'to' must be one of "hex", "rgb", or "hsl".
- The converter preserves any alpha channel provided in the input.
- The tool is stateless and safe for concurrent use from multiple callers.
