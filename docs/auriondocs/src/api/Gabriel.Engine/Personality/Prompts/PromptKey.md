# PromptKey

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`  
> **Kind:** class

Holds the canonical, compile-time string identifiers for every named prompt fragment the registry exposes. Use these constants when selecting, looking up, or switching on prompt fragments so callers get a single source of truth (and typos surface as build errors).

## Remarks
This class centralizes the keys that map into the prompt fragments (see Fragments.*). Each key is a public const string so it folds into switch arms and dictionary keys at compile time; that makes lookups efficient and ensures accidental typos become compilation failures rather than runtime mismatches. Keys are grouped by topic using a dot-separated convention (e.g. "persona.static", "mode.chatty"); when adding a new mode or fragment, add its key here and the corresponding Fragments constant.

## Example
```csharp
// Common patterns: dictionary registry or switch-based selection
var registry = new Dictionary<string, string>
{
    [PromptKey.PersonaStatic] = Fragments.PersonaStatic,
    [PromptKey.ModeChatty]   = Fragments.ModeChatty
};

switch (key)
{
    case PromptKey.ModeChatty:
        ApplyChattyMode();
        break;
    case PromptKey.ModeConcise:
        ApplyConciseMode();
        break;
}
```

## Notes
- These constants are identifiers only; they do not contain fragment content — ensure a matching Fragments.* constant exists for any key you add.
- Because values are compile-time consts, changing a key's text is a breaking change for any persisted or external references; prefer adding new keys over renaming.
- The class is immutable and safe for concurrent use, but calling code relies on exact string equality — avoid duplicating literal strings outside this central set.