using System.Text.RegularExpressions;
using Gabriel.Core.Personality;
using Gabriel.Engine.Services;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Personality;

// Markdown stripping was intentionally removed - the persona allows discord-style
// inline emphasis (bold, italic, code, quotes) because users expect it. Only
// AI-ism openers/closers and a length cap remain as safety nets; the persona
// prompt is the primary defense.
public sealed class ResponsePostProcessor : IResponsePostProcessor
{
    private readonly ITokenEstimator _tokens;
    private readonly PersonalityOptions _options;

    public ResponsePostProcessor(ITokenEstimator tokens, IOptions<PersonalityOptions> options)
    {
        _tokens = tokens;
        _options = options.Value;
    }

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

        // AI-ism openers / closers.
        foreach (var rx in OpenerRegexes) text = rx.Replace(text, "");
        foreach (var rx in CloserRegexes) text = rx.Replace(text, "");

        text = text.Trim();

        // Length cap. Multiplier of the user's last message tokens, raised to
        // DetailResponseTokenCap when the user asked for elaboration.
        var lastUserTokens = state?.LastUserTokenCount ?? 0;
        var cap = state?.UserAskedForDetail == true
            ? _options.DetailResponseTokenCap
            : Math.Min(
                (int)(lastUserTokens * _options.MaxResponseMultiplier),
                _options.MaxResponseTokenCap);

        // Don't cap to zero - short user messages still allow a minimum reply.
        cap = Math.Max(cap, 30);

        var currentTokens = _tokens.EstimateText(text);
        if (currentTokens <= cap) return text;

        return TruncateAtSentenceBoundary(text, cap);
    }

    // Walk back from the cap-estimated char position to the nearest sentence-end
    // punctuation. Falls back to a hard char cut if no boundary is found within
    // a sensible window.
    private string TruncateAtSentenceBoundary(string text, int targetTokens)
    {
        // 4 chars per token is the same approximation the estimator uses; good enough.
        var approxCharLimit = Math.Min(targetTokens * 4, text.Length);
        if (approxCharLimit >= text.Length) return text;

        // Try to land on a sentence terminator (. ! ?) within the last ~80 chars
        // of our budget. If none, hard-cut and add an ellipsis.
        var searchStart = Math.Max(0, approxCharLimit - 80);
        var lastTerminator = -1;
        for (var i = approxCharLimit - 1; i >= searchStart; i--)
        {
            var c = text[i];
            if (c == '.' || c == '!' || c == '?')
            {
                lastTerminator = i;
                break;
            }
        }

        if (lastTerminator > 0) return text[..(lastTerminator + 1)].TrimEnd();
        return text[..approxCharLimit].TrimEnd() + "…";
    }
}
