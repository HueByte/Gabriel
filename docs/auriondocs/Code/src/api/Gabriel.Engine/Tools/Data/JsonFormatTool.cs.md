# JsonFormatTool

> **File:** `src/api/Gabriel.Engine/Tools/Data/JsonFormatTool.cs`  
> **Kind:** class

```csharp
public sealed class JsonFormatTool : ITool
```


Formats, validates, or minifies a JSON string using System.Text.Json. It's a pure function with no I/O or side effects: provide the JSON string and an optional mode (pretty, minify, or validate); it returns either the formatted JSON, a compact single-line string, or a validity message. If the input is invalid, it reports the error location (line and position) to help locate the problem. It also enforces a maximum input length (500,000 characters) to guard resource usage.

## Remarks
JsonFormatTool serves as a predictable JSON-handling primitive for tooling and diagnostics. By centralizing formatting behavior, it avoids ad-hoc string manipulation scattered throughout the codebase. The three modes cover common developer needs: pretty for human readability, minify for compact transmission or storage, and validate to verify syntax without transforming the data. Being a pure function makes it suitable for inclusion in pipelines and tests without hidden side effects.

## Notes
- Enforces a hard limit of 500,000 characters for the input JSON, producing an error if exceeded.
- Error messages report the location using 1-based line and position indices to aid debugging.
- Not designed for structural JSON mutations beyond formatting or syntax validation (no data reshaping or querying).