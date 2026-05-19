using System.Text.RegularExpressions;
using Gabriel.Core.Personality;

namespace Gabriel.Engine.Personality;

// Markdown stripping was intentionally removed - the persona allows discord-style
// inline emphasis (bold, italic, code, quotes) because users expect it. Only
// AI-ism openers/closers remain as a cosmetic safety net; the persona prompt is
// the primary defense.
//
// The previous length cap was removed too: it truncated the persisted message
// to less than the live-streamed text, which made the response look "cut off"
// after reloading the conversation from the DB.
public sealed class ResponsePostProcessor : IResponsePostProcessor
{
    // AI-ism opener patterns - checked at the very start of the response (case-insensitive).
    // We strip the matching prefix and trim, leaving the substantive content.
    private static readonly Regex[] OpenerRegexes =
    [
        new(@"^that['']?s a (great|really good|fantastic|interesting) question[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^i think you['']?ll find that[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^here['']?s what i think[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^to answer your question[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^i appreciate you sharing[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^certainly[,.\s!]+", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^absolutely[,.\s!]+", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^i['']?d be happy to help[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^let me break this down[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^here['']?s the thing[,.\s]*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    // AI-ism closer patterns - checked at the very end. Match optionally trailing
    // punctuation so "Hope that helps!" gets caught.
    private static readonly Regex[] CloserRegexes =
    [
        new(@"\s*let me know if (you['']?d like to know more|you have any questions)[.!?]*\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\s*feel free to ask[.!?]*\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\s*hope (that|this) helps[.!?]*\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\s*does that make sense[.!?]*\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    public string Clean(string raw, ConversationState? state)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;

        var text = raw.Trim();

        foreach (var rx in OpenerRegexes) text = rx.Replace(text, "");
        foreach (var rx in CloserRegexes) text = rx.Replace(text, "");

        return text.Trim();
    }
}
