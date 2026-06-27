# JsonFormatTool

> **File:** `src/api/Gabriel.Engine/Tools/Data/JsonFormatTool.cs`  
> **Kind:** class

```csharp
public sealed class JsonFormatTool : ITool
```


JsonFormatTool validates, formats, or minifies a JSON string. It delegates parsing and re-serialization to System.Text.Json and returns a human-friendly formatted JSON, a compact one-liner, or a validation message—without performing any I/O.

## Remarks

JsonFormatTool is a pure formatting utility: given the same input, it always yields the same output and has no side effects beyond producing a string. It centralizes JSON handling behind a small API surface, and its error messages include the location (line and position) of invalid JSON to ease debugging in logs and pipelines.

## Example

```csharp
// Pretty-print a JSON string
var tool = new JsonFormatTool();
string input = "{\"a\":1,\"b\":\"text\"}";
string args = "{\"json\": \"" + input.Replace("\"","\\\"") + "\", \"mode\": \"pretty\"}";
string pretty = await tool.ExecuteAsync(args, CancellationToken.None);

// Minify a JSON string
string minArgs = "{\"json\": \"" + input.Replace("\"","\\\"") + "\", \"mode\": \"minify\"}";
string minified = await tool.ExecuteAsync(minArgs, CancellationToken.None);

// Validate a JSON string
string valArgs = "{\"json\": \"" + input.Replace("\"","\\\"") + "\", \"mode\": \"validate\"}";
string validation = await tool.ExecuteAsync(valArgs, CancellationToken.None);
```

## Notes

- The input JSON string is capped at 500,000 characters; larger payloads yield an error: `'json' is too long (max 500000 characters).`.
- The default mode is pretty if `mode` is omitted; supported modes are pretty, minify, and validate.
- The tool reports JSON parsing errors with a precise line and position, and returns results as strings (no exceptions are thrown to the caller).