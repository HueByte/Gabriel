# JsonFormatTool

> **File:** `src/api/Gabriel.Engine/Tools/Data/JsonFormatTool.cs`  
> **Kind:** class

```csharp
public sealed class JsonFormatTool : ITool
```


JsonFormatTool is a pure utility that can validate, pretty-print, or minify a JSON string. It delegates parsing and re-serialization to System.Text.Json, wrapping those capabilities with a mode-based interface and user-friendly error reporting. Provide the JSON text via the `json` argument and an optional `mode` of `pretty` (default), `minify`, or `validate`. The tool returns either the formatted JSON (pretty or compact) or a short validity message. If the input JSON is invalid, it returns an error string like `Error: invalid JSON at line {line}, position {pos}.` A hard input length limit of 500,000 characters guards against excessively large payloads. Because this is a pure function with no I/O, outputs are deterministic and suitable for editors, pipelines, or testing.