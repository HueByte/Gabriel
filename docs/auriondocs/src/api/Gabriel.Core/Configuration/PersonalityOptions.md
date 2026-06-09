# PersonalityOptions

> **File:** `src/api/Gabriel.Core/Configuration/PersonalityOptions.cs`  
> **Kind:** class

Holds persona metadata and pacing parameters used to simulate a human-like streaming response. Bind this class to the "Personality" configuration section when you need to adjust the persona display name (injected into system prompts / few-shot examples) or tune the simulated streaming tempo (initial thinking delay and characters-per-second ranges).

## Remarks
This POCO centralizes two concerns: the persona identity (Name) and the streaming-tempo controller (min/max thinking delay and chars-per-second). The runtime chooses a random value per turn from the configured ranges to add natural jitter to the bot's streamed output. The SectionName constant is provided so the options can be bound cleanly from an IConfiguration source.

## Example
```csharp
// Bind from IConfiguration (Microsoft.Extensions.Configuration)
var personality = configuration.GetSection(PersonalityOptions.SectionName).Get<PersonalityOptions>();

// Pick a per-turn thinking delay and chars-per-second within the configured ranges
var thinkingMs = Random.Shared.Next(personality.MinThinkingDelayMs, personality.MaxThinkingDelayMs + 1);
var charsPerSecond = Random.Shared.Next(personality.MinCharsPerSecond, personality.MaxCharsPerSecond + 1);

// Use values to pace streamed output (pseudo-code)
await Task.Delay(thinkingMs);
await StreamTextWithRateLimit(text, charsPerSecond);
```

## Notes
- Units: MinThinkingDelayMs/MaxThinkingDelayMs are in milliseconds; MinCharsPerSecond/MaxCharsPerSecond are in characters per second.  
- Ensure min <= max; code that samples the range may assume that and produce unexpected results if violated.  
- These options are mutable configuration values—changing them at runtime will only affect new decisions made by code that reads the bound instance; existing in-flight streams won't be retroactively adjusted.  
- Name is currently global (used in system prompts and few-shot blocks); comments indicate a future plan for per-project personalities.