using System.Text;

namespace Gabriel.Engine.Providers.ToolBridge;

// Splits a single text stream into "live-streamable" deltas vs a buffered
// tail starting at the first <tool_call> marker. GabrielToolBridge feeds each
// incoming text delta through Process and yields whatever it returns as
// TextDeltaEvent; once the splitter latches into buffer mode, everything
// from that point on is held in BufferedText for end-of-turn parsing.
//
// The whole point of this thing is to keep the typewriter feel for pure-text
// turns (the common case) without ever streaming a half-marker that we'd
// then have to retract. The candidate buffer holds at most "<tool_call>"
// worth of characters (11) before committing one way or the other.
internal sealed class ToolCallStreamSplitter
{
    private const string Marker = "<tool_call>";

    // Characters that COULD be the start of the marker but we don't know yet.
    // Held back from emission until either:
    //  - the next char extends the prefix to the full marker → switch to
    //    buffer mode (move candidate into _buffered)
    //  - the next char diverges from the marker prefix → flush as live text
    private readonly StringBuilder _candidate = new();

    // Once we've matched the open marker, every subsequent char accumulates
    // here regardless of what it is. End-of-stream hands this to the block
    // parser, which extracts <tool_call>{...}</tool_call> pairs.
    private readonly StringBuilder _buffered = new();

    private bool _inBufferMode;

    public bool InBufferMode => _inBufferMode;
    public string BufferedText => _buffered.ToString();

    // Feed one delta chunk. Returns the substring safe to emit as live text
    // (may be empty if everything is currently held / buffered).
    public string Process(string delta)
    {
        if (string.IsNullOrEmpty(delta)) return string.Empty;

        var emit = new StringBuilder();
        foreach (var ch in delta)
        {
            if (_inBufferMode)
            {
                _buffered.Append(ch);
            }
            else
            {
                ProcessChar(ch, emit);
            }
        }
        return emit.ToString();
    }

    // Call once at end-of-stream. Returns any held-but-unemitted text that
    // never grew into a marker (e.g. the stream ended in the middle of "<to"
    // — those chars should be flushed as normal text). BufferedText holds
    // the post-marker tail; flush there is the parser's job, not ours.
    public string Flush()
    {
        if (_inBufferMode || _candidate.Length == 0) return string.Empty;
        var trailing = _candidate.ToString();
        _candidate.Clear();
        return trailing;
    }

    private void ProcessChar(char ch, StringBuilder emit)
    {
        if (_candidate.Length == 0)
        {
            if (ch == Marker[0])
            {
                _candidate.Append(ch);
            }
            else
            {
                emit.Append(ch);
            }
            return;
        }

        _candidate.Append(ch);
        var candStr = _candidate.ToString();

        if (candStr == Marker)
        {
            // Full marker assembled. Move it into the buffer (the parser
            // looks for <tool_call>...</tool_call> pairs, so the open tag
            // needs to be present in BufferedText) and latch into buffer
            // mode for the rest of the turn.
            _buffered.Append(candStr);
            _candidate.Clear();
            _inBufferMode = true;
            return;
        }

        if (Marker.StartsWith(candStr, StringComparison.Ordinal))
        {
            // Still a valid prefix - keep holding.
            return;
        }

        // Diverged. The candidate isn't a marker; emit it as text. Watch one
        // edge case: the last char we just appended could itself be a fresh
        // marker start (e.g. candidate was "<x<" - we want to flush "<x" and
        // hold the trailing "<" as a new candidate).
        var tail = candStr[^1];
        if (tail == Marker[0] && candStr.Length > 1)
        {
            emit.Append(candStr.AsSpan(0, candStr.Length - 1));
            _candidate.Clear();
            _candidate.Append(tail);
        }
        else
        {
            emit.Append(candStr);
            _candidate.Clear();
        }
    }
}
