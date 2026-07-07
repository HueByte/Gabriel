# GetCurrentTimeTool

> **File:** `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`  
> **Kind:** class

```csharp
public class GetCurrentTimeTool : ITool
```


GetCurrentTimeTool is a tiny, stateless utility that implements ITool to return the current UTC time as an ISO 8601 string. It accepts no input and immediately returns a timestamp, making it handy when you need a canonical, timezone-agnostic time value for logging, tracing, or inter-tool coordination.

## Remarks
It is stateless and safe to call concurrently, because all it does is read the system clock and format the value. By using DateTimeOffset.UtcNow and the \"o\" format, it yields a round-trippable timestamp that is compatible across systems and cultures.

## Notes
- ArgumentsJsonSchema indicates an empty options object; the implementation ignores the input.
- The return value is not constant and depends on the time of invocation.
