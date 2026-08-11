# JsonFormatTool

> **File:** `src/api/Gabriel.Engine/Tools/Data/JsonFormatTool.cs`  
> **Kind:** class

```csharp
public sealed class JsonFormatTool : ITool
```


JsonFormatTool provides a pure function to validate, pretty-print, or minify a JSON string. It delegates parsing and serialization to System.Text.Json, wrapping common formatting modes behind a user-friendly API and returning either the formatted JSON, a single-line minified string, or a validation message. The mode can be pretty (default), minify, or validate; invalid JSON yields a precise error location, and argument parsing validates required fields and length constraints. This tool is designed for human-readable JSON in logs or UIs and quick correctness checks without performing I/O.

## Remarks
JsonFormatTool encapsulates a small, focused concern: transforming JSON text according to a requested presentation while reporting errors consistently. By centralizing formatting behavior (indentation, escaping) and leveraging a standard library for parsing/serialization, it provides a predictable, testable surface that callers can reuse across tooling without duplicating logic. The use of relaxed JSON escaping keeps non-ASCII characters readable in output, which is helpful for human inspection and diagnostics.

## Example
```csharp
// Example usage: pretty-print a JSON string
var tool = new JsonFormatTool();
string args = "{\"json\": \"{\\\"name\\\":\\\"Alice\\\",\\\"age\\\":30}\", \"mode\": \"pretty\"}";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
```

## Notes
- If JSON is invalid, the result begins with "Error: invalid JSON at line X, position Y".
- The input must contain a non-empty string property named 'json'; an optional 'mode' may be provided and must be one of: pretty, minify, validate.
- The maximum allowed length for the JSON string is 500,000 characters.
- The default mode is pretty when mode is omitted.
- This tool performs formatting/validation without side effects or I/O.