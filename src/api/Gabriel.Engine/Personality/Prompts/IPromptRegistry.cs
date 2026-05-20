namespace Gabriel.Engine.Personality.Prompts;

// Read-only access to every named prompt fragment that feeds the system
// prompt. The interface exists so storage can evolve independently from
// consumers — today everything is `const string` in `Fragments.*` partials,
// but the same shape works for embedded markdown resources or external .md
// files if either becomes useful later.
//
// Fragments may carry placeholder tokens (e.g. `{name}`); substitution is
// the caller's job — keeps the registry parameter-free.
public interface IPromptRegistry
{
    string Get(string key);
}
