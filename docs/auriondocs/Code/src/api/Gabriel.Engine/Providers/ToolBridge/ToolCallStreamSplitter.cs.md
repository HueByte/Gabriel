# ToolCallStreamSplitter

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs`  
> **Kind:** class

```csharp
internal sealed class ToolCallStreamSplitter
```


ToolCallStreamSplitter is a small, stateful helper that splits a continuous text delta stream into live, emit-ready text and a buffered tail starting at the first <tool_call> marker. It preserves a typewriter-like flow for ordinary text while ensuring no partial tool-markers are emitted; once the marker is detected, the remainder is buffered for downstream parsing of complete <tool_call> blocks. The splitter tracks the next possible marker prefix in a candidate buffer (limited in length to the marker) to minimize re-emission and correctly handle delta boundaries.

## Remarks
This abstraction separates streaming text from the parsing of embedded tool-calls. It solves the problem of markers potentially being split across deltas by buffering the open tag and all subsequent data, enabling a downstream parser to reliably extract <tool_call> blocks without corrupting the live text stream. It exposes simple state information (InBufferMode and BufferedText) so callers can inspect progress and the buffered tail without threading through internal state.

## Example
```csharp
var splitter = new ToolCallStreamSplitter();

// normal text arrives and is emitted immediately
string live1 = splitter.Process("Intro: ");
Console.Write(live1); // Intro: 

// a marker start arrives in the same delta
string live2 = splitter.Process("<tool_call>doSomething</tool_call> done");
Console.Write(live2); // (empty)

bool inBuffer = splitter.InBufferMode;        // true
string buf = splitter.BufferedText;            // "<tool_call>doSomething</tool_call>"
```

## Notes
- Not thread-safe; the instance maintains mutable state and should be used from a single thread unless external synchronization is applied.
- The marker value is a fixed constant ("<tool_call>"); the behavior depends on recognizing this exact string, so changes to the marker would require corresponding adjustments in usage.
- If end-of-stream is reached without encountering a full marker, call Flush to emit any trailing text that did not form a marker; otherwise that text remains un-emitted.
