namespace Gabriel.Engine.Personality.Prompts;

// Default IPromptRegistry — maps every PromptKey to the matching `Fragments.*`
// const string. Singleton; the dictionary is built once at construction.
//
// To add a new fragment:
//   1. Add a `const string` on the `Fragments` partial (under
//      `Personality/Prompts/Fragments.*.cs`).
//   2. Add a `PromptKey.*` constant pointing at the same identifier.
//   3. Add the mapping below.
//
// Three coordinated edits. The compiler enforces the wiring after that.
public sealed class PromptRegistry : IPromptRegistry
{
    private readonly IReadOnlyDictionary<string, string> _fragments;

    public PromptRegistry()
    {
        _fragments = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PromptKey.PersonaStatic]     = Fragments.PersonaStatic,
            [PromptKey.PersonaFewShot]    = Fragments.PersonaFewShot,
            [PromptKey.PersonaMemory]     = Fragments.PersonaMemory,
            [PromptKey.PersonaFormatting] = Fragments.PersonaFormatting,

            [PromptKey.ModeChatty]       = Fragments.ModeChatty,
            [PromptKey.ModeElaborative]  = Fragments.ModeElaborative,
            [PromptKey.ModeConcise]      = Fragments.ModeConcise,
            [PromptKey.ModeTutor]        = Fragments.ModeTutor,
            [PromptKey.ModeCritic]       = Fragments.ModeCritic,
        };
    }

    public string Get(string key)
    {
        if (_fragments.TryGetValue(key, out var fragment))
        {
            return fragment;
        }

        throw new KeyNotFoundException(
            $"No prompt fragment registered under key '{key}'. " +
            "Register it via PromptRegistry's constructor + add a Fragments.* const.");
    }
}
