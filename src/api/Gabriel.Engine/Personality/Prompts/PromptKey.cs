namespace Gabriel.Engine.Personality.Prompts;

// String identifiers for every named prompt fragment the registry holds.
// Live as `const string` so they fold into switch arms / dictionary keys at
// compile time and a typo is an immediate build error.
//
// Grouped by topic (dot-separated). When a new mode or section lands, add
// its key here and the matching `Fragments.*` const it points at.
public static class PromptKey
{
    // Static persona block — Gabriel's identity / mode rules / hard prohibitions.
    public const string PersonaStatic = "persona.static";

    // The few-shot exchanges that anchor the model's register-mirroring.
    public const string PersonaFewShot = "persona.few-shot";

    // Memory-system guidance — when to save, what to save, format conventions.
    public const string PersonaMemory = "persona.memory";

    // Rendering surfaces available in the UI (markdown / mermaid / LaTeX) and
    // rules-of-thumb for when each is worth reaching for.
    public const string PersonaFormatting = "persona.formatting";

    // Per-mode behaviour snippets. Selected by Conversation.Mode and appended
    // to the static block per-turn so the same persona can be re-weighted
    // without rewriting the whole prompt.
    public const string ModeChatty      = "mode.chatty";
    public const string ModeElaborative = "mode.elaborative";
    public const string ModeConcise     = "mode.concise";
    public const string ModeTutor       = "mode.tutor";
    public const string ModeCritic      = "mode.critic";
}
