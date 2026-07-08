# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments serves as a container for prompt fragments that seed the model's behavior. PersonaFewShot provides a curated set of exchanges illustrating register-mirroring and length-matching in practice, with {name} replaced at runtime to personalize the persona.

## Remarks
These anchors centralize how the system demonstrates a talking style to the model, ensuring consistent tone and back-and-forth rhythm across prompts. By hosting them in a single, reusable constant, the engine can swap personas or adjust the style without changing prompt-building logic. They also demonstrate how to preserve lowercasing, abbreviations, and verbiage that the persona uses, which helps guide the model's responses toward a natural, human-like conversation.

## Example
```csharp
// Example: incorporate the small few-shot persona into a live prompt
string persona = "Nova";
string prompt = Fragments.PersonaFewShot.Replace("{name}", persona) + "\nUser: can you help me with this task?\nNova: ";
```

## Notes
- The PersonaFewShot block is large; including it increases prompt size; only include when necessary to set tone.