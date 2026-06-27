# PersonalityOptions

> **File:** `src/api/Gabriel.Core/Configuration/PersonalityOptions.cs`  
> **Kind:** class

Holds configuration for the assistant's persona and the simulated streaming/typing tempo used when forwarding partial text (deltas). Reach for this type when you need to configure the persona display name that is injected into system prompts or to tune the timing behavior used to simulate thinking and human-like typing during streaming responses.

## Remarks
This class centralizes two related concerns: the persona identity (Name) and the runtime pacing parameters for streaming output. It is intended to be bound from configuration (it implements IConfigSection) so the persona and tempo can be adjusted without code changes. The timing properties provide a simple, randomized simulation by choosing values from the configured ranges per turn: an initial "thinking" pause and a characters-per-second target used to pace deltas.

## Example
```csharp
// Create or bind from configuration and tweak defaults
var p = new PersonalityOptions
{
    Name = "Gabriel",
    MinThinkingDelayMs = 300,
    MaxThinkingDelayMs = 900,
    MinCharsPerSecond = 50,
    MaxCharsPerSecond = 80
};

// Typical usage (pseudocode):
// var delay = RandomBetween(p.MinThinkingDelayMs, p.MaxThinkingDelayMs);
// var cps = RandomBetween(p.MinCharsPerSecond, p.MaxCharsPerSecond);
// use delay and cps to pace streamed text deltas.
```

## Notes
- Units: MinThinkingDelayMs / MaxThinkingDelayMs are in milliseconds; MinCharsPerSecond / MaxCharsPerSecond are characters per second.
- Ensure Min values are <= corresponding Max values and that values are non-negative; the class does not validate ranges itself.
- Name is injected into the system prompt and few-shot block; changing it alters the persona presented to downstream systems.
- This configuration is currently global; comments indicate a future plan for per-project personality scoping.