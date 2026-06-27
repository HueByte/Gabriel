# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`  
> **Kind:** class

Holds short, per-mode prompt fragments that are appended to the persona block at each turn to bias the assistant's behaviour according to Conversation.Mode. Use these constants when constructing a turn-level prompt so the prompt builder can always append a mode block (ModeChatty is intentionally minimal so no conditional is required).

## Remarks
This class centralises the human-readable mode biases (chatty, elaborative, concise, tutor, etc.) so they can be kept small, reviewable, and versioned separately from the main persona text. Each fragment is a focused adjustment to the baseline persona rather than a full rewrite — the persona still defines core style and constraints while these fragments re-weight length, depth and stance.

## Example
```csharp
// Compose a prompt for the current conversation turn by appending the mode fragment.
string prompt = personaBase + "\n\n";

switch (conversation.Mode)
{
    case Mode.Elaborative:
        prompt += Fragments.ModeElaborative;
        break;
    case Mode.Concise:
        prompt += Fragments.ModeConcise;
        break;
    case Mode.Tutor:
        prompt += Fragments.ModeTutor;
        break;
    default:
        prompt += Fragments.ModeChatty; // default, intentionally minimal
        break;
}

// Then send `prompt` to the model as the system/persona block for the turn.
```

## Notes
- These are compile-time constants: changing the text requires a rebuild and deploy.
- Keep fragments short — long fragments increase token usage and can dilute the primary persona.
- ModeChatty exists so callers can unconditionally append a mode block; do not remove or replace it with an empty null/conditional pattern.