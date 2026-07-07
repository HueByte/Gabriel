# AgentOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentOptions.cs`  
> **Kind:** class

```csharp
public class AgentOptions : IConfigSection<AgentOptions>
```


AgentOptions is a configuration container used by the agent runtime to tune how it operates during conversations. It exposes three tunable properties that govern (1) how many tool-call iterations the agent is allowed per user turn, (2) when to perform a rolling summary of the conversation history, and (3) how many of the most recent messages should be kept verbatim through that summarization. This type participates in the [`IConfigSection<AgentOptions>`](IConfigSection.cs.md) pattern and is bound under the SectionName "Agent", enabling configuration sources to wire these knobs into the agent's execution.

## Remarks

By isolating these knobs, the policy governing tool usage and context management is decoupled from the agent’s core decision logic. Operators can adjust cost, latency, and continuity without code changes, and the compacting behavior leverages ContextWindowTokens to balance historical context with summarization overhead. The CompactKeepLast setting ensures a slice of recent history remains intact through a compact pass, preserving mid-conversation coherence even as older content is summarized.

## Notes

- There is no explicit runtime validation in this type; ensure configured values are sane (e.g., non-negative, 0 <= CompactThreshold <= 1) at the configuration boundary to avoid undefined behavior.
- If your configuration system supports dynamic reload, review whether changes to AgentOptions apply immediately or only after application restart, depending on the hosting environment.
- Increasing MaxIterations can raise latency and cost due to longer tool-search loops; tune this value to balance responsiveness with thoroughness.