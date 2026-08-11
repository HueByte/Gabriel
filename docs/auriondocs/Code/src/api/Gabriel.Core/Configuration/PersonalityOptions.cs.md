# PersonalityOptions

> **File:** `src/api/Gabriel.Core/Configuration/PersonalityOptions.cs`  
> **Kind:** class

```csharp
public class PersonalityOptions : IConfigSection<PersonalityOptions>
```


PersonalityOptions is a strongly-typed configuration class that defines the system persona and its streaming tempo. It controls the persona display name (Name) injected into prompts and few-shot blocks, and it tunes the simulated typing rhythm used when forwarding delta text through the stream (MinThinkingDelayMs/MaxThinkingDelayMs and MinCharsPerSecond/MaxCharsPerSecond).

## Remarks
Implemented as [`IConfigSection<PersonalityOptions>`](IConfigSection.cs.md), it binds under the 'Personality' section to centralize persona-related settings. The default values provide a sensible out-of-the-box persona ('Gabriel') and modest streaming cadence, while allowing runtime tweaks without changing code. This abstraction keeps concerns about identity and user experience separate from composition logic, enabling consistent behavior across components that render or stream messages.

## Example
```csharp
// Example: customize the persona and typing behavior
var options = new PersonalityOptions
{
    Name = "Nova",
    MinThinkingDelayMs = 200,
    MaxThinkingDelayMs = 800,
    MinCharsPerSecond = 60,
    MaxCharsPerSecond = 90
};
```

## Notes
- MinThinkingDelayMs should not exceed MaxThinkingDelayMs to avoid negative delays.
- MinCharsPerSecond should not exceed MaxCharsPerSecond to avoid unrealistic speeds.