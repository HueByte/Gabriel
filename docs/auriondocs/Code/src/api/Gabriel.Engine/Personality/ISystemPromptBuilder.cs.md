# ISystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/ISystemPromptBuilder.cs`  
> **Kind:** interface

```csharp
public interface ISystemPromptBuilder
```


ISystemPromptBuilder builds the system prompt for a turn by stitching a static persona with a per-mode bias and dynamic guidance derived from the current ConversationState. If mode is null, the GabrielMode.Chatty baseline is used, and if the state is null, no dynamic guidance is added; the interface is stateless and safe to reuse across calls.

## Remarks
This interface centralizes the logic for interpreting ConversationState and mode into a single prompt string, decoupling prompt assembly from the rest of the system. It enables swapping prompt strategies (e.g., different per-mode biases) without altering call sites, and it supports testing with mock or varied ConversationState inputs. By relying on Fragments to supply the static persona and bias, it keeps concerns separated and makes it straightforward to extend prompts for new GabrielMode values.

## Notes
- Null mode falls back to the Chatty baseline; supply a non-null GabrielMode when you want a different tone.
- Null ConversationState yields no dynamic guidance, so provide a ConversationState when you want personalization.