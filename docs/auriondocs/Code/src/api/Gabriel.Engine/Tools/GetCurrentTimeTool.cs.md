# GetCurrentTimeTool

> **File:** `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`  
> **Kind:** class

Returns the current UTC time as an ISO 8601 (round-trip) string. Use this tool when an agent or component in the tool framework needs a simple, dependency-free way to obtain the host's current UTC timestamp; the tool is intentionally minimal and designed for use as a building-block in ReAct-style tool chains.

## Remarks
This class implements a tiny ITool that exposes the tool name ("get_current_time"), a short description, an empty JSON schema for parameters, and an ExecuteAsync implementation that produces the current UTC time. It exists to provide a predictable, side-effect-free source of the system UTC time inside the tool-execution environment without requiring external services.

## Example
```csharp
var tool = new GetCurrentTimeTool();
Console.WriteLine(tool.Name); // "get_current_time"
var resultTask = tool.ExecuteAsync("{}", CancellationToken.None);
string isoUtc = await resultTask; // e.g. "2026-06-08T14:23:45.1234567+00:00"
```

## Notes
- The ExecuteAsync implementation ignores both the provided JSON arguments and the CancellationToken; it returns a completed Task with the current time.
- The timestamp is produced from the host system clock (DateTimeOffset.UtcNow); clock skew or system time configuration will affect the returned value.
- The string format uses DateTimeOffset.ToString("o") (round‑trip ISO 8601), which includes fractional seconds and the offset ("+00:00" or "Z").
- The implementation is non-blocking and thread-safe (no mutable state).