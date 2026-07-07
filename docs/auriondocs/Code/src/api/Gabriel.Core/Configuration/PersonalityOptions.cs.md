# PersonalityOptions

> **File:** `src/api/Gabriel.Core/Configuration/PersonalityOptions.cs`  
> **Kind:** class

```csharp
public class PersonalityOptions : IConfigSection<PersonalityOptions>
```


PersonalityOptions is a strongly-typed configuration container that defines the runtime persona and streaming tempo for Gabriel. It binds to the 'Personality' configuration section and is consumed to inject the persona name into system prompts and few-shot blocks, while also tuning how the assistant streams text. The Name field determines the display name used for the persona (default 'Gabriel'). MinThinkingDelayMs/MaxThinkingDelayMs model the initial 'thinking' pause before the first delta is forwarded, creating a natural cadence. MinCharsPerSecond/MaxCharsPerSecond define the target character throughput per second for streaming deltas, with per-turn jitter to mimic human-like typing speed.

## Remarks
Isolates persona data and streaming tempo from core messaging, enabling centralized tuning without touching runtime logic. The SectionName binding helps keep configuration discovery consistent and supports future per-project personality variants without code changes. Together these fields provide a simple, low-risk knob-set for operators to tailor voice and pacing per deployment.

## Notes
- MinThinkingDelayMs should be <= MaxThinkingDelayMs to avoid invalid timing.
- Units: delays are in milliseconds; throughput is chars per second; ensure unit consistency across the system.