# ToolCallStreamSplitter

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs`  
> **Kind:** class

Splits an incoming text stream into two flows: the portion that is safe to emit immediately (live/text typing feel) and a buffered tail that starts when the fixed marker "<tool_call>" is detected. Use this when you need to stream plain text deltas as they arrive but must stop immediate emission once a tool-call marker begins so the marker and the following block can be parsed as a unit.

## Remarks
This class exists to preserve a "typewriter" live-streaming experience for normal text while preventing streaming a partially-received marker that would later need to be retracted. It keeps a short candidate buffer for characters that might form the start of the marker; if the candidate completes the marker the splitter latches into buffer mode and moves the marker (and everything after it) into BufferedText for end-of-turn parsing. Until the marker is fully matched the splitter only holds at most the marker-length prefix and emits all other characters immediately.

## Example
```csharp
var splitter = new ToolCallStreamSplitter();

// Simulate incoming chunks from a streaming source
foreach (var chunk in incomingChunks)
{
    // Process returns the substring safe to emit immediately
    string live = splitter.Process(chunk);
    if (!string.IsNullOrEmpty(live))
    {
        // send live to clients as a streaming delta
        EmitLiveText(live);
    }

    if (splitter.InBufferMode)
    {
        // From here on, everything is being accumulated in splitter.BufferedText
        // for the block parser to handle at end of turn.
        break; // or stop streaming further live deltas
    }
}

// At end-of-stream, flush any short candidate that never became a marker
string trailing = splitter.Flush();
if (!string.IsNullOrEmpty(trailing)) EmitLiveText(trailing);

// The parser should examine splitter.BufferedText for complete <tool_call>...</tool_call> blocks.
```

## Notes
- Once the splitter enters buffer mode (InBufferMode == true) all subsequent characters are appended to BufferedText and will not be emitted as live text.
- Flush() only returns any held candidate prefix when the splitter has not entered buffer mode; it does not return buffered post-marker text — BufferedText must be consumed separately by the parser.
- The class is not synchronized; call sites must ensure single-threaded use or external synchronization if accessed concurrently.