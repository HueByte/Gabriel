# PersonalityOptions

> **File:** `src/api/Gabriel.Core/Configuration/PersonalityOptions.cs`  
> **Kind:** class

```csharp
public class PersonalityOptions : IConfigSection<PersonalityOptions>
```


PersonalityOptions is a configuration section that defines the assistant's persona and its streaming behavior. It exposes a display name (Name) to be injected into the system prompt and the few-shot block, and timing controls that shape how the assistant’s streaming deltas are presented. The class is identified as the 'Personality' section via SectionName and implements [`IConfigSection<PersonalityOptions>`](IConfigSection.cs.md) to integrate with the configuration system. Together, these settings let you tailor who the assistant sounds like and how quickly it responds, without changing the response-generation logic itself. The streaming controls include an initial thinking delay (MinThinkingDelayMs/MaxThinkingDelayMs) and a per-turn typing pace (MinCharsPerSecond/MaxCharsPerSecond), enabling a natural, human-like delivery of text.