# GetCurrentTimeTool

> **File:** `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`  
> **Kind:** class

Returns the current UTC time as an ISO 8601 string and exposes it through the ITool contract. Use this when an agent or test harness needs a simple, dependency-free way to obtain the current UTC timestamp in a standardized string format (for example, during ReAct loop demos or integration tests where tools are represented by ITool implementations).

## Remarks
This class is a minimal, stateless implementation of ITool intended as a trivial starter tool for agent/tool integration tests and examples. It does not parse or use the provided JSON arguments and returns a task already completed with the current UTC time formatted using the round-trip ("o") ISO 8601 pattern. The CancellationToken passed to ExecuteAsync is ignored.

## Example
```csharp
var tool = new GetCurrentTimeTool();
string result = await tool.ExecuteAsync("{}", CancellationToken.None);
Console.WriteLine(result); // e.g. "2026-06-09T15:04:05.0000000+00:00"
```

## Notes
- The tool always reports UTC (DateTimeOffset.UtcNow); it does not return local time.
- The ParametersJsonSchema is an empty object (no input expected); any argumentsJson is ignored.
- CancellationToken is not observed by ExecuteAsync — the method returns a completed Task synchronously.
- The returned string uses the "o" (round-trip) format, which includes the offset (typically "+00:00" for UTC).