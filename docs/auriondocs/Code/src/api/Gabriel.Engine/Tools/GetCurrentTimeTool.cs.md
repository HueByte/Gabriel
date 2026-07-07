# GetCurrentTimeTool

> **File:** `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`  
> **Kind:** class

```csharp
public class GetCurrentTimeTool : ITool
```


GetCurrentTimeTool is a minimal ITool implementation that returns the current UTC time as an ISO 8601 string. It is useful in automation pipelines when a timestamp is needed without introducing external dependencies or custom formatting logic.

It ignores any input arguments and simply returns DateTimeOffset.UtcNow.ToString("o"), which yields a standard, ISO 8601 round-trip timestamp (including the Z suffix for UTC).

## Remarks
GetCurrentTimeTool serves as a tiny, stateless utility in the Gabriel.Engine tool ecosystem. By providing a consistent timestamp in a pluggable tool, it simplifies logging, metadata stamping, and time-based decision points in workflows without requiring clients to manage clock dependencies or formatting themselves. Because it relies on the system clock, its output is inherently time-varying and not suitable for deterministic tests unless the clock is abstracted or mocked at the call site.

## Notes
- Time is derived from the host system clock, so values change over time.
- Output is UTC in ISO 8601 round-trip format (e.g., 2024-12-31T23:59:59.9999999Z).
- The tool takes no input; the argumentsJson is ignored.