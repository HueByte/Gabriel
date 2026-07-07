# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a collection of predefined per-mode bias blocks that get appended to the static persona block at runtime. It does not replace the base persona; instead, it re-weights the model's behavior by injecting mode-specific guidance based on Conversation.Mode (Chatty, Elaborative, Concise, Tutor). When building a prompt, a developer selects the appropriate Fragments constant and appends it to the persona before generating content.

## Remarks
Fragments centralizes mode-specific tonal nudges, enabling dynamic, consistent persona adjustments without modifying the core baseline. By isolating per-mode text in one place, it’s easy to tune or extend each mode and swap them per-turn based on Conversation.Mode. This separation also makes the prompt builder simpler and safer, since the mode block is a self-contained fragment that describes how the baseline should be weighted rather than rewriting it.

## Example
```csharp
// Example usage: choose the mode block based on current conversation mode and append to the base persona
string modeBlock = Conversation.Mode switch
{
    ConversationMode.Chatty => Fragments.ModeChatty,
    ConversationMode.Elaborative => Fragments.ModeElaborative,
    ConversationMode.Concise => Fragments.ModeConcise,
    ConversationMode.Tutor => Fragments.ModeTutor,
    _ => Fragments.ModeChatty
};

string prompt = basePersonaPrompt + "\n" + modeBlock;
```

## Notes
- The ModeChatty block is the default and intentionally minimal so the baseline persona drives behavior.
- Mode blocks include explicit banners to clearly delineate the active mode within the prompt.
- Blocks are raw strings meant to be appended to the existing prompt; ensure proper spacing/newlines to avoid formatting issues.