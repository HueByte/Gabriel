# ToolCallStreamSplitter

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallStreamSplitter
```


ToolCallStreamSplitter is an internal helper that separates a live text delta stream from a buffered tail that starts at the first <tool_call> marker. It is used by GabrielToolBridge to feed each incoming text delta through Process and emit only the live text until a complete <tool_call> marker is observed; once the marker is seen, all subsequent characters are buffered in BufferedText for end-of-turn parsing. This preserves the typing feel of streaming text while guaranteeing that no partial marker is emitted as live text. The candidate buffer is constrained to the length of the marker (11 characters), and the decision to emit or buffer is made as soon as a full marker is detected or the prefix diverges.

## Remarks
This abstraction decouples streaming output from the marker-based parser, ensuring downstream parsing always starts from a complete <tool_call> boundary. It solves the problem of potentially emitting a partial marker mid-stream and provides a predictable hand-off point where the rest of the turn's content is consumed by the parser via BufferedText.