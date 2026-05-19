using System.Globalization;
using System.Text.RegularExpressions;
using Gabriel.Core.Personality;
using Gabriel.Engine.Services;

namespace Gabriel.Engine.Personality;

// Pure-heuristic implementation. No LLM call, so per-turn overhead is microseconds.
// We accept some mood mis-classifications in exchange for predictability and zero
// cost. An LLM-backed updater is an obvious future swap (same interface).
public sealed class HeuristicConversationStateUpdater : IConversationStateUpdater
{
    private readonly ITokenEstimator _tokens;

    public HeuristicConversationStateUpdater(ITokenEstimator tokens)
    {
        _tokens = tokens;
    }

    // Window for "this user message was short". Tuned roughly against 4-chars-per-token.
    private const int ShortTokenThreshold = 10;

    // Task-mode / detail triggers. When the user is asking for a substantive
    // artifact (code, explanation, document, list, etc.) the post-processor uses
    // the higher length cap AND the system prompt swaps length-matching for
    // "deliver the full thing." Word-boundary anchored so we don't trip on
    // "explanation" or "written" inside unrelated prose.
    //
    // Two clusters:
    //   - Explain/teach verbs: explain, tell me about, how does, what is, why, describe, walk me through
    //   - Produce/deliver verbs: write, implement, build, code, create, generate, draft, compose,
    //     design, produce, refactor, fix, debug, make me, give me, show me, help me with
    private static readonly Regex DetailCueRegex = new(
        @"\b(explain|tell me about|tell me|how does|how do|what is|why does|why is|walk me through|describe|"
        + @"write|implement|build|code|create|generate|draft|compose|design|produce|refactor|fix|debug|"
        + @"do it|go ahead|send it|just do it|write it|make it|"
        + @"make me|give me|show me|help me with)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Catches short imperative requests like "bubble sort please" or "quicksort
    // pls" where there's no leading task verb but the noun + "please" is a clear
    // ask. Conservative — must end with please/pls preceded by at least one word.
    private static readonly Regex PleaseSuffixRegex = new(
        @"\w+\s+(please|pls)\s*\??\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PlayfulCueRegex = new(
        @"\b(lol|lmao|haha|hahaha|rofl)\b|!{2,}|\bxd\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SeriousCueRegex = new(
        @"^(honestly|seriously|look|listen|ok so)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Coarse negative-affect lexicon for venting detection. Intentionally small —
    // venting also needs profanity or "i (just )?" framing to register.
    private static readonly Regex VentingNegativeRegex = new(
        @"\b(hate|sucks|fuck|fucking|shit|exhausted|tired of|annoyed|frustrat\w+|stressed|overwhelm\w+)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Lightweight emoji detection: high surrogates cover most modern emoji (BMP
    // supplementary plane) and U+2600-U+27BF covers Misc Symbols + Dingbats which
    // is where ☀ ❤ ✨ etc. live.
    private static bool ContainsEmoji(string s)
    {
        foreach (var ch in s)
        {
            if (char.IsHighSurrogate(ch)) return true;
            if (ch >= '☀' && ch <= '➿') return true;
        }
        return false;
    }

    public ConversationState Update(ConversationState? current, string userMessage)
    {
        var state = current ?? ConversationState.Initial();
        var trimmed = userMessage.Trim();
        var tokens = _tokens.EstimateText(trimmed);

        var emaAvg = state.TurnCount == 0
            ? tokens
            : (state.AvgUserTokenCount * 0.7f) + (tokens * 0.3f);

        var newMood = ClassifyMood(trimmed, state.Mood, tokens);

        return state with
        {
            TurnCount = state.TurnCount + 1,
            LastUserTokenCount = tokens,
            AvgUserTokenCount = emaAvg,
            Mood = newMood,
            RecentTopics = ExtractTopics(trimmed, state.RecentTopics),
            LastMessageAt = DateTimeOffset.UtcNow,
            ConsecutiveShortMessages = tokens < ShortTokenThreshold
                ? state.ConsecutiveShortMessages + 1
                : 0,
            UserUsesEmoji = state.UserUsesEmoji || ContainsEmoji(trimmed),
            UserUsesLowercase = DetectLowercase(trimmed),
            UserAskedForDetail = DetailCueRegex.IsMatch(trimmed) || PleaseSuffixRegex.IsMatch(trimmed),
        };
    }

    private static Mood ClassifyMood(string message, Mood previous, int tokens)
    {
        if (PlayfulCueRegex.IsMatch(message)) return Mood.Playful;
        if (VentingNegativeRegex.IsMatch(message) && message.Contains("i ", StringComparison.OrdinalIgnoreCase))
            return Mood.Venting;
        if (SeriousCueRegex.IsMatch(message)) return Mood.Serious;
        if (tokens > 100 && message.Contains('?')) return Mood.Curious;
        if (tokens < 5 && !message.Any(char.IsPunctuation)) return Mood.LowEnergy;

        // Decay toward neutral when nothing else fires — sticky moods feel artificial.
        return previous == Mood.Neutral ? Mood.Neutral : DecayToward(previous);
    }

    // Decay step: keep the prior mood for one turn (no signal), but bias toward
    // neutral on subsequent silent turns. With the current heuristic this just
    // returns the prior; a more elaborate decay table is a future tweak.
    private static Mood DecayToward(Mood previous) => previous;

    // Very lightweight topic extraction: drop common stop words, take the longest
    // 3-4 remaining tokens, lowercase, dedupe. Good enough for the metadata block.
    // Replace with an LLM call later if we care about quality.
    private static IReadOnlyList<string> ExtractTopics(string message, IReadOnlyList<string> previous)
    {
        var keepLast = previous.TakeLast(5).ToList();
        var candidates = message
            .Split(new[] { ' ', '\t', ',', '.', '!', '?', ';', ':', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 5)
            .Select(t => t.ToLower(CultureInfo.InvariantCulture).Trim('"', '\'', '(', ')'))
            .Where(t => !StopWords.Contains(t))
            .OrderByDescending(t => t.Length)
            .Take(3)
            .ToList();

        foreach (var c in candidates)
        {
            if (!keepLast.Contains(c)) keepLast.Add(c);
        }
        return keepLast.TakeLast(5).ToList();
    }

    private static bool DetectLowercase(string message)
    {
        if (string.IsNullOrEmpty(message)) return false;
        var first = message.TrimStart().FirstOrDefault();
        if (first == default || !char.IsLetter(first)) return false;
        return char.IsLower(first);
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "about", "after", "again", "almost", "alone", "along", "also", "although", "always", "another",
        "anyone", "anything", "around", "before", "being", "below", "between", "could", "doing", "during",
        "either", "every", "first", "found", "given", "going", "great", "having", "least", "maybe",
        "might", "never", "often", "other", "really", "right", "since", "still", "their", "there",
        "these", "thing", "think", "those", "thought", "through", "together", "under", "until", "using",
        "value", "where", "which", "while", "would", "yours",
    };
}
