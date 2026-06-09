# ToolCallStreamSplitter

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallStreamSplitter.cs`  
> **Kind:** class

Splits an incoming text stream into two flows: immediate "live" text that is safe to emit as-is, and a buffered tail that begins at the first occurrence of the literal marker "<tool_call>". Use this when you need to stream plain text character-by-character (to preserve a typewriter-like experience) but must never emit a partial marker that would later need retracting. GabrielToolBridge feeds each incoming delta through Process and emits whatever string Process returns; once the splitter detects the full marker it latches into buffer mode and collects everything that follows into BufferedText for end-of-turn parsing.

## Remarks
This splitter exists to bridge streaming character deltas with block-oriented parsing for tool calls. It maintains a very small candidate buffer that holds only the prefix needed to decide whether the recent characters form the start of the marker. If the prefix becomes the full marker the splitter moves into buffer mode and every subsequent char is accumulated in BufferedText (including the opening marker). If the candidate diverges from the marker prefix it is flushed as live text so normal streaming continues without delay.

## Example
```csharp
var splitter = new ToolCallStreamSplitter();

// feed incremental deltas (e.g. from a typewriter-like stream)
foreach (var delta in incomingDeltas)
{
    var emit = splitter.Process(delta);
    if (!string.IsNullOrEmpty(emit))
    {
        // send emit to the live consumer
        Console.Write(emit);
    }

    if (splitter.InBufferMode)
    {
        // from now on, content is in splitter.BufferedText for the parser
        // and Process(...) will no longer return live text for those chars
    }
}

// end of stream: any held-but-uncommitted prefix should be flushed
var trailing = splitter.Flush();
if (!string.IsNullOrEmpty(trailing)) Console.Write(trailing);

// BufferedText (if any) contains the marker and the rest of the tail
var buffered = splitter.BufferedText;
// hand `buffered` to whatever block parser extracts <tool_call>...</tool_call>
```

## Notes
- The marker is the fixed literal "<tool_call>" and is matched using ordinal comparison (case-sensitive).
- Once the splitter enters buffer mode it remains latched: Process will append subsequent chars to BufferedText and will not emit further live text.
- Flush only returns any candidate prefix that never became a marker; it does not flush BufferedText (parsing/consumption of BufferedText is the caller's responsibility).
- ToolCallStreamSplitter is not synchronized for concurrent use; callers should ensure single-threaded access or external synchronization.