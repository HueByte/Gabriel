# GetCurrentTimeTool

> **File:** `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`  
> **Kind:** class

```csharp
public class GetCurrentTimeTool : ITool
```


GetCurrentTimeTool is a minimal ITool implementation that returns the current UTC time as an ISO 8601 string. It has no input parameters and returns DateTimeOffset.UtcNow.ToString("o") via ExecuteAsync, making it a convenient, pluggable time source for tool workflows where you want to avoid direct DateTime calls in business logic.

## Remarks
GetCurrentTimeTool is stateless and safe to reuse across concurrent executions. It supports testability and swap-ability in pipelines: you can replace it with a mock that returns a fixed timestamp during tests, keeping time concerns isolated from core logic. This abstraction helps separate concerns between time reporting and domain behavior within a tool-driven workflow.

## Example
```csharp
using System.Threading;

var tool = new GetCurrentTimeTool();
string timestamp = await tool.ExecuteAsync("{}", CancellationToken.None);
```

## Notes
- The returned value represents the moment of invocation and will differ between calls.
- ExecuteAsync completes synchronously (no I/O) by returning a completed Task.