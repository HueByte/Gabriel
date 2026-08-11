# AgentOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentOptions.cs`  
> **Kind:** class

```csharp
public class AgentOptions : IConfigSection<AgentOptions>
```


AgentOptions defines the configuration section for the Agent component, encapsulating tunable runtime policies that influence how the agent interacts with tools during a turn. It implements [`IConfigSection<AgentOptions>`](IConfigSection.cs.md), and its SectionName is "Agent", signaling it can be loaded from a configuration source. The class exposes three configurable properties with sensible defaults: MaxIterations limits how many tool-call iterations are allowed per user turn (default 8) to prevent runaway loops and excessive spend; CompactThreshold expresses the fraction of the model's ContextWindowTokens at which a rolling-summary compact is triggered (default 0.8); and CompactKeepLast specifies how many of the most recent messages should be retained verbatim when producing a compacted summary (default 6).