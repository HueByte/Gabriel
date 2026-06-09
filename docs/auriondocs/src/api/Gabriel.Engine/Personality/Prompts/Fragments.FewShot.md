# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs`  
> **Kind:** class

Provides a compile-time constant prompt fragment that primes the model with persona few-shot examples. Use Fragments.PersonaFewShot when assembling prompts that must make the assistant adopt a consistent, informal voice (register-mirroring, abbreviations, verbal tics, question-back behavior) and when you want both conversational (chat-mode) and task-oriented examples included; the {name} placeholder is substituted at runtime.

## Remarks
This fragment centralizes the few-shot examples used to teach the assistant a particular speaking style and behavior across the codebase. It contains both chat-mode exchanges (short, informal back-and-forths demonstrating register and tone) and task-mode examples (concise code answers). Declared as a public const string, it is intended to be referenced by prompt-building code rather than modified at runtime; changes require recompilation.

## Notes
- The {name} token is a runtime placeholder and is not interpolated by C#; consumers must substitute it when constructing the final prompt.
- The value is a raw multi-line string literal: its newlines, spacing and punctuation are significant and preserved — do not trim or reflow unless you intend to change the examples the model sees.
- The examples deliberately include informal language and mild profanity to teach register-mirroring; sanitize or avoid using this fragment when passing prompts to models constrained by stricter content policies.