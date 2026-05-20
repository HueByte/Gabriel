using System.Globalization;
using System.Text;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Personality;
using Gabriel.Engine.Personality.Prompts;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Personality;

// "Gabriel" persona: a natural-DM agent that mirrors the user's energy, avoids
// AI-isms, and never falls into bullet-list assistant mode. The persona text
// itself lives in Personality/Prompts/Fragments.* — this class is the
// assembly orchestrator that pulls fragments from IPromptRegistry, splices
// in per-turn ConversationState + per-conversation GabrielMode, and emits
// the final system message.
//
// Phase 8 (per-project personality) will replace this with a per-project
// SystemPrompt + per-project few-shot. For now the persona is hardcoded
// through Fragments.PersonaStatic.
public sealed class GabrielSystemPromptBuilder : ISystemPromptBuilder
{
    private readonly PersonalityOptions _options;
    private readonly IPromptRegistry _prompts;

    // Pre-substituted strings cached so we don't re-run Replace on every turn.
    private readonly string _staticBlock;
    private readonly string _formattingBlock;
    private readonly string _fewShotBlock;

    public GabrielSystemPromptBuilder(IOptions<PersonalityOptions> options, IPromptRegistry prompts)
    {
        _options = options.Value;
        _prompts = prompts;
        _staticBlock = SubstituteName(_prompts.Get(PromptKey.PersonaStatic), _options.Name);
        _formattingBlock = _prompts.Get(PromptKey.PersonaFormatting);
        _fewShotBlock = SubstituteName(_prompts.Get(PromptKey.PersonaFewShot), _options.Name);
    }

    public string Build(ConversationState? state, GabrielMode? mode = null)
    {
        var sb = new StringBuilder(_staticBlock.Length + 1024);
        sb.Append(_staticBlock);
        sb.AppendLine();
        sb.AppendLine();

        // What the UI actually renders — markdown surface (gfm + mermaid +
        // KaTeX). Sits between the static persona ("who you are") and the
        // mode snippet ("how to weight behaviour") because it's a medium
        // concern, not an identity or behaviour one.
        sb.AppendLine(_formattingBlock);
        sb.AppendLine();

        // Per-conversation mode snippet — appended right after the static
        // block so the model reads it as "additional rules layered on top of
        // the baseline persona". Always present so the prompt structure
        // doesn't change shape across modes.
        sb.AppendLine(_prompts.Get(ModeKey(mode)));
        sb.AppendLine();

        sb.AppendLine("[Conversation metadata]");
        sb.Append("Turn: ").Append(state?.TurnCount ?? 0).AppendLine();
        sb.Append("User's last message length: ~").Append(state?.LastUserTokenCount ?? 0).AppendLine(" tokens");
        sb.Append("Conversation mood: ").AppendLine((state?.Mood ?? Mood.Neutral).ToString().ToLower(CultureInfo.InvariantCulture));
        if (state?.UserUsesEmoji == true) sb.AppendLine("User uses emoji - light mirroring is allowed.");
        if (state?.UserUsesLowercase == true) sb.AppendLine("User writes in lowercase - match.");
        if (state?.ConsecutiveShortMessages >= 2) sb.AppendLine("Recent messages have been very short - don't force engagement.");
        if (state?.UserAskedForDetail == true) sb.AppendLine("User is in TASK MODE - they want a substantive artifact (code, doc, explanation).");
        if (state?.UserAskedForDetail == true && state.ConsecutiveShortMessages >= 1)
        {
            sb.AppendLine("⚠ STALL WARNING: user has been sending short follow-ups while waiting for the artifact. Your previous replies were too short. PRODUCE THE FULL ARTIFACT IN THIS REPLY. No more confirmations.");
        }
        sb.AppendLine();
        sb.AppendLine("[Guidance]");
        sb.AppendLine(LengthGuidance(state));
        sb.AppendLine(MoodGuidance(state?.Mood ?? Mood.Neutral));
        sb.AppendLine();
        sb.Append(_fewShotBlock);
        return sb.ToString();
    }

    private static string ModeKey(GabrielMode? mode) => (mode ?? GabrielMode.Chatty) switch
    {
        GabrielMode.Elaborative => PromptKey.ModeElaborative,
        GabrielMode.Concise     => PromptKey.ModeConcise,
        GabrielMode.Tutor       => PromptKey.ModeTutor,
        GabrielMode.Critic      => PromptKey.ModeCritic,
        _                       => PromptKey.ModeChatty,
    };

    // Fragments carry `{name}` as a literal placeholder; substitute once at
    // construction so the per-turn Build call doesn't repeat the work.
    private static string SubstituteName(string template, string name)
        => template.Replace("{name}", name);

    private static string LengthGuidance(ConversationState? state)
    {
        // Task mode short-circuits everything else: when the user asks for
        // code / a document / an explanation, length-matching is actively
        // harmful. Their imperative "write it" might be 2 tokens but the
        // correct reply is the full artifact, not "1-8 words".
        if (state?.UserAskedForDetail == true)
        {
            return "They asked you to PRODUCE something concrete (code, doc, explanation, list). " +
                   "Deliver the full artifact in one reply - length-matching does NOT apply. " +
                   "Don't preface with 'alright, here's a basic X' or 'sure thing' - just write it. " +
                   "Markdown formatting (fenced code blocks especially) is fine and expected here.";
        }

        return (state?.LastUserTokenCount ?? 0) switch
        {
            // Truly tiny ('lol', 'k', 'fair') - mirror in scale, but if there's even a sliver of substance, one punchy sentence with a hook.
            <= 5  => "User went very short. If it's pure noise ('lol', 'k') mirror it. Otherwise, ONE punchy sentence with personality - a take, a callback, a reaction with actual flavor. 'yeah ok' is failing.",
            <= 20 => "1-3 sentences. Match their casual register but BRING SOMETHING - an opinion, a curious question, a small observation, a callback to earlier. Bare acknowledgments ('yeah, details help') are a fail.",
            <= 60 => "3-5 sentences. Engage with the substance, add your angle, push the conversation forward.",
            <= 150 => "Match their depth - a short paragraph that actually engages, not just summarizes back.",
            _ => "They wrote something substantial. Be thorough; stay under ~250 words.",
        };
    }

    private static string MoodGuidance(Mood mood) => mood switch
    {
        Mood.Playful => "Keep it light. Jokes, banter, and short quips land well - but still bring an angle, not flat one-liners.",
        Mood.Venting => "Listen more than advise. Validate, don't fix. Short empathetic reactions WITH genuine warmth, not 'damn that sucks' canned-style.",
        Mood.Serious => "Drop the jokes. Be direct, thoughtful, and substantive.",
        Mood.Curious => "They're exploring an idea. Engage with it, add your take, ask one thing if genuinely curious.",
        Mood.LowEnergy => "They're not super engaged right now. Keep it brief, but make the brief reply count - one good sentence beats two flat ones.",
        _ => "Neutral - bring an angle, a take, or a curious question. 'Match the room' does NOT mean strip personality.",
    };
}
