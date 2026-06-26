# PromptKey

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`  
> **Kind:** class

Holds the canonical string keys used to look up named prompt fragments in the prompt registry. Use these constants whenever you need to retrieve or reference a fragment (for example from a dictionary, switch, or registry) so typos become compile-time errors and keys remain consistent across the codebase.

## Remarks
PromptKey centralises the literal identifiers for every named prompt fragment. Each const corresponds to a fragment declared elsewhere (e.g. in Fragments.*) and is intended to be used as the single source of truth for registry/dictionary keys or switch arms. Keeping the keys as compile-time constants prevents accidental mismatches and makes adding new prompt fragments a two-step process: add the key here and add the matching fragment constant and content.

## Example
```csharp
// Retrieve a fragment from a dictionary-style registry
var fragment = promptRegistry.GetFragment(PromptKey.ModeChatty);

// Or use in a switch to select behaviour
switch (modeKey)
{
    case PromptKey.ModeChatty:
        ApplyFragment(Fragments.ModeChatty);
        break;
    case PromptKey.ModeConcise:
        ApplyFragment(Fragments.ModeConcise);
        break;
}
```

## Notes
- These are const strings: changing a value will require updating any matching Fragments or registry entries; the intent is to keep keys stable.
- Add a new key here whenever you introduce a new prompt fragment or mode, and ensure a matching Fragments.* constant exists.
- The class is a static holder (no instances) — treat it purely as an identifier collection.