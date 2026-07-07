# ToolCallStreamSplitter

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallStreamSplitter
```


ToolCallStreamSplitter is an internal sealed helper that splits a single text stream into live-deliverable deltas and a buffered tail starting at the first <tool_call> marker. It preserves a typewriter-like streaming experience by never emitting a partial marker; instead, it buffers the portion after the open tag for the parser to consume as complete blocks. The splitter maintains a small candidate buffer used to detect the marker prefix and, upon seeing the full marker, latches into buffer mode and appends the marker and subsequent text to BufferedText for end-of-turn parsing. The candidate buffer is bounded to the marker length (11 characters) to minimize false emissions. The Process method returns only the portion of the input delta that can be emitted immediately, while subsequent data accumulate in the internal buffers. End-of-stream handling is performed by Flush: if no marker has been completed, it returns any trailing characters that never grew into a marker; if a marker has begun, Flush yields nothing until the parser consumes the buffered tail.

## Remarks
ToolCallStreamSplitter isolates the boundary between raw text and tool-call content. By latching into buffer mode when <tool_call> is detected, downstream parsing can operate on well-formed <tool_call>...</tool_call> blocks without risking emission of incomplete markers. This design supports a smooth typewriter-like feel while ensuring the parser only sees complete tool-call blocks.

## Notes
- Marker detection is exact and case-sensitive: the marker string is literally '<tool_call>'.
- The splitter assumes delta chunks arrive in order; out-of-order delivery could disrupt marker detection.
- If the stream ends before a full marker is formed, Flush returns trailing characters as normal text.
- The candidate buffer is limited to the marker length to bound latency before switching to buffered mode.