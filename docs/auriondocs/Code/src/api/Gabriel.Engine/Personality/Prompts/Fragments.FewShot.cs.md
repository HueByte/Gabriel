# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a utility container that collects prompt fragments used to shape a model's conversational persona. It exposes a single public constant, PersonaFewShot, which holds a multi-line, runtime-substitution-friendly prompt block. The template demonstrates both chat-mode and task-mode exchanges and is designed to calibrate behaviors such as register-mirroring and length-conscious responses. The {name} placeholder is substituted at runtime, just as the static block is substituted, enabling per-prompt persona customization in the prompting pipeline.

## Remarks

Centralizing these few-shot exemplars provides a single source of truth for style calibration across chat and task prompts. The static, immutable nature of the template ensures consistent behavior at startup, while the partial class designation signals extensibility—additional fragments can be added in other files to extend or tailor prompts without changing existing code paths.

## Example

```csharp
// Example usage: bind a persona name into the few-shot template
string persona = "Alex";
string promptTemplate = Fragments.PersonaFewShot.Replace("{name}", persona);
// `promptTemplate` can be prepended or integrated into the final prompt payload
```

## Notes

- Always replace the {name} placeholder before using the template in a prompt to avoid leaking the placeholder to end users.
- The PersonaFewShot value is defined as a C# 11 raw string literal (triple-quoted); ensure your project targets a compatible language version.
- The embedded examples cover both chat-mode and task-mode scenarios; adapt tone, length, and content to your app's policy and UX guidelines.
