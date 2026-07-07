# AgentOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentOptions.cs`  
> **Kind:** class

```csharp
public class AgentOptions : IConfigSection<AgentOptions>
```


AgentOptions defines a typed configuration surface for tuning an agent's runtime behavior. It is intended to be bound from configuration to adjust tool usage (MaxIterations), summarization timing (CompactThreshold), and continuity preservation (CompactKeepLast) without code changes.

## Remarks
This abstraction relies on the [`IConfigSection<T>`](IConfigSection.cs.md) pattern to centralize tuning in a single, strongly-typed section and on a static SectionName to anchor configuration binding to an "Agent" section. The defaults provide sensible behavior out of the box, while the properties map directly to runtime controls that influence performance, cost, and user-perceived continuity. Tying CompactThreshold to the provider's ContextWindowTokens ensures summarization behavior remains aligned with the model's actual context capacity, making the configuration robust to provider changes. Together with its dependencies, AgentOptions serves as a cohesive tuning surface that keeps operational parameters separate from implementation details.

## Notes
- Changing MaxIterations affects how aggressively the agent pursues tool-usage within a single turn; setting it too low may truncate tasks, while too high can increase cost or risk runaway loops.
- CompactThreshold is evaluated against the model's ContextWindowTokens; if the provider's window changes, re-tuning may be necessary to maintain desired summarization cadence.
- CompactKeepLast controls how many of the most recent messages are preserved verbatim during compaction; increasing this helps mid-conversation continuity but may limit summarization efficiency.
