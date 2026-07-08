# AgentService.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentService.cs`

## Contents

- [AgentService](#agentservice)
- [TurnPrompts](#turnprompts)
- [AgentService (constructor)](#agentservice-constructor)
- [AppendMemory](#appendmemory)
- [ExecuteToolSafelyAsync](#executetoolsafelyasync)
- [GenerateSummaryAsync](#generatesummaryasync)
- [GetContextMetricsAsync](#getcontextmetricsasync)
- [LoadMemoryBlockAsync](#loadmemoryblockasync)
- [LoadProjectSystemPromptAsync](#loadprojectsystempromptasync)
- [LoadTurnPromptsAsync](#loadturnpromptsasync)
- [MaybeCompactAsync](#maybecompactasync)
- [Preview](#preview)
- [RegenerateAsync](#regenerateasync)
- [ResolveModelSelectionAsync](#resolvemodelselectionasync)
- [RunAsync](#runasync)
- [RunStreamAsync](#runstreamasync)
- [RunStreamWithUserPreambleAsync](#runstreamwithuserpreambleasync)
- [SelectCompactCutIndex](#selectcompactcutindex)
- [SerializeToolCalls](#serializetoolcalls)
- [EmptyStopMaxRetries](#emptystopmaxretries)
- [EmptyStopRetryDelayMs](#emptystopretrydelayms)
- [LogPreviewLimit](#logpreviewlimit)

---

## AgentService
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** class

```csharp
public class AgentService : IAgentService
```


Coordinates the end-to-end agent turn: it validates and persists the incoming user message, resolves the model selection and stable prompt pieces (persona, project prompt, memory, tool descriptors), populates the scoped tool-execution context, and then drives the provider loop that emits a stream of AgentEvent values. Use this service when you need an orchestrated, repository-backed conversational turn that includes prompt building, tool integration, memory loading, compacting long context, and post-processing of provider output rather than calling a chat provider directly.

## Remarks
AgentService is the high-level coordinator that glues together conversation/project repositories, the provider registry, model selection, memory and tool subsystems, and response post-processing. It performs upfront validation and persists the user message and conversation state before starting the server-sent-event stream so errors surface as regular HTTP ProblemDetails (4xx) and the conversation timeline remains consistent if the client disconnects. The class also contains a small, bounded retry policy specifically for the provider "empty Stop" hiccup (a successful stream response with an empty final text) and constructs a GabrielToolBridge per-call for tool-related features; the bridge is stateless across calls and AgentService owns its logger.

## Example
```csharp
// Consume the streaming events produced by the agent for a single turn.
IAgentService agent = /* resolve from DI */;
Guid conversationId = /* existing conversation id */;
string userInput = "Summarize the project status.";

await foreach (var ev in await agent.RunAsync(conversationId, userInput))
{
    // AgentEvent is a discriminated record containing progress, partial text,
    // final assistant messages, tool call requests/results, etc.
    Console.WriteLine(ev);
}
```

## Notes
- Empty-stop retry: the service retries the agent loop a small number of times (EmptyStopMaxRetries = 2; linear backoff of N * 500ms) to recover when the provider finishes with an empty Stop token — this is intentionally implemented inside the agent loop because the HTTP resilience pipeline cannot detect an empty final text on a successful 200 stream.
- Validation and persistence happen before the SSE headers are sent; this lets the caller receive clean 4xx responses (ProblemDetails) from global exception handling instead of a broken stream.
- ToolMode and tool descriptors are resolved early so features like ToolMode.None can drop tool descriptors at the source; project-aware tools get a populated IToolExecutionContext so they do not need to infer project scope from model input.

---

## TurnPrompts
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** record

```csharp
private sealed record TurnPrompts(
        string PersonaPrompt,
        string? ProjectPrompt,
        string? MemoryBlock,
        IReadOnlyList<ToolDescriptor> Tools)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `PersonaPrompt` | `string` | — |
| `ProjectPrompt` | `string?` | — |
| `MemoryBlock` | `string?` | — |
| `Tools` | `IReadOnlyList<ToolDescriptor>` | — |


TurnPrompts is an internal, immutable bundle of inputs that guides per-turn prompt construction for the agent. It collects the mandatory PersonaPrompt, the optional ProjectPrompt, the optional MemoryBlock, and the list of Tools into a single snapshot that AgentContext.Build uses to assemble the actual prompt and history exposed to the providers.

Because it is private and sealed, callers cannot rearrange or replace its contents; the assembly happens within AgentContext.Build, ensuring a consistent context for token estimation, history provisioning, and tool discovery.

## Remarks
TurnPrompts serves as a boundary that keeps prompt composition concerns isolated from public APIs. By bundling persona, project, memory, and tools in one place, it simplifies maintaining a coherent per-turn context across the agent lifecycle and enables consistent token budgeting and provider history generation.

## Notes
- Private visibility means changes to this record shape are internal; external code should not rely on its structure.
- MemoryBlock and ProjectPrompt are nullable; null indicates absence.


---

## AgentService (constructor)
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** constructor

```csharp
public AgentService(
        IConversationRepository conversations,
        IProjectRepository projects,
        IChatProviderRegistry providerRegistry,
        IModelCatalog modelCatalog,
        IUserPreferences userPrefs,
        IMemoryService memories,
        IToolRegistry tools,
        IToolExecutionContext toolContext,
        IUnitOfWork uow,
        ITokenEstimator tokens,
        ICurrentUser currentUser,
        IConversationStateUpdater stateUpdater,
        ISystemPromptBuilder promptBuilder,
        IResponsePostProcessor postProcessor,
        IOptions<AgentOptions> options,
        ILogger<AgentService> logger,
        ILogger<GabrielToolBridge> toolBridgeLogger)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversations` | [`IConversationRepository`](../../Gabriel.Core/Repositories/IConversationRepository.cs.md) | — |
| `projects` | [`IProjectRepository`](../../Gabriel.Core/Repositories/IProjectRepository.cs.md) | — |
| `providerRegistry` | [`IChatProviderRegistry`](../Providers/IChatProviderRegistry.cs.md) | — |
| `modelCatalog` | [`IModelCatalog`](../Providers/IModelCatalog.cs.md) | — |
| `userPrefs` | [`IUserPreferences`](../../Gabriel.Core/Identity/IUserPreferences.cs.md) | — |
| `memories` | [`IMemoryService`](../../Gabriel.Core/Services/IMemoryService.cs.md) | — |
| `tools` | [`IToolRegistry`](../Tools/IToolRegistry.cs.md) | — |
| `toolContext` | [`IToolExecutionContext`](../Tools/IToolExecutionContext.cs.md) | — |
| `uow` | [`IUnitOfWork`](../../Gabriel.Core/Repositories/IUnitOfWork.cs.md) | — |
| `tokens` | [`ITokenEstimator`](ITokenEstimator.cs.md) | — |
| `currentUser` | [`ICurrentUser`](../../Gabriel.Core/Identity/ICurrentUser.cs.md) | — |
| `stateUpdater` | [`IConversationStateUpdater`](../Personality/IConversationStateUpdater.cs.md) | — |
| `promptBuilder` | [`ISystemPromptBuilder`](../Personality/ISystemPromptBuilder.cs.md) | — |
| `postProcessor` | [`IResponsePostProcessor`](../Personality/IResponsePostProcessor.cs.md) | — |
| `options` | `IOptions<AgentOptions>` | — |
| `logger` | `ILogger<AgentService>` | — |
| `toolBridgeLogger` | `ILogger<GabrielToolBridge>` | — |


The AgentService constructor acts as the wiring point for the engine's agent component. It gathers core collaborators—repositories for conversations and projects, a registry of chat providers, a model catalog, user preferences, memory and tool infrastructure, a tool execution context, unit of work, token estimation, current user context, state updater, system prompt builder, and a post-processor—and assigns them to internal fields for runtime use. It also materializes the AgentOptions from the provided `IOptions<AgentOptions>` so configuration knobs (e.g., MaxIterations, CompactThreshold) are available to the service. This constructor is intended to be resolved by the dependency-injection container, not called directly, and then used to orchestrate agent interactions within the Gabriel Engine.

## Remarks
Architecturally, this constructor models AgentService as an orchestrator that composes multiple cross-cutting concerns: persistence, tooling, prompting, and model interaction. By taking all dependencies up front, it enables easier testing via mocks and allows behavior to be tuned via AgentOptions without changing code. The explicit dependency surface also clarifies the integration points with Gabriel Engine's tooling and provider ecosystem.

## Notes
- No null checks are performed in the constructor; rely on the DI container to supply non-null instances.
- Accessing options.Value assumes a configured AgentOptions; ensure AgentOptions is configured in the host to provide required values (e.g., MaxIterations, CompactThreshold).

---

## AppendMemory
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private static void AppendMemory(StringBuilder sb, Core.Entities.MemoryEntry m)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `sb` | `StringBuilder` | — |
| `m` | `Core.Entities.MemoryEntry` | — |

**Returns:** `void`


AppendMemory is a private helper that renders a MemoryEntry as a Markdown fragment into the supplied StringBuilder. It formats a header line from the entry’s type (converted to lowercase in a culture-invariant manner) and the entry’s name, followed by the entry’s description and body, with blank lines separating these sections. This centralizes the formatting of memory entries when assembling documentation for the agent, guaranteeing a consistent presentation across all entries.

## Remarks
This abstraction exists to keep memory-entry rendering consistent and isolated from higher-level document assembly. By using ToString().ToLowerInvariant() on the entry type, headings remain stable across locales. The method relies on the MemoryEntry’s Type, Name, Description, and Body to produce a self-contained Markdown snippet that can be appended to a larger document, keeping the concerns of formatting separate from data retrieval.

## Notes
- No escaping is performed for m.Name, m.Description, or m.Body; any embedded Markdown or special characters will influence the final rendering.
- The method is private and static; it assumes the provided StringBuilder is properly scoped by the caller. It does not perform synchronization, so concurrent calls sharing the same StringBuilder require external synchronization.

---

## ExecuteToolSafelyAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<string> ExecuteToolSafelyAsync(ChatProviderToolCall call, Guid conversationId, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `call` | [`ChatProviderToolCall`](../Providers/ChatProviderToolCall.cs.md) | — |
| `conversationId` | `Guid` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<string>`


Executes a named tool in a safe, asynchronous wrapper. It looks up the tool by name from the internal registry; if the tool is not registered it returns an error string and logs a rejection. If found, it logs the start of the tool call, starts a stopwatch, and awaits tool.ExecuteAsync(call.ArgumentsJson, ct). After completion, it stops the timer, inspects the result: if the observation begins with \"Error\" (case-insensitive), it is treated as a soft failure and logged as a warning; otherwise it is logged as a successful invocation with timing and length metrics. The method returns the tool's observation (or an empty string if null). If the tool throws, the exception is logged as an error and a user-facing error message is returned containing the tool name and exception message.

## Remarks
Centralizes tool invocation inside the engine, providing uniform logging, timing, and error handling for all tool calls. It uses a short Preview of arguments and results to avoid flooding logs with potentially large payloads. It also distinguishes soft errors (strings starting with 'Error') from successful results, surfacing problems to operators without crashing the conversation flow.

## Notes
- Soft-error detection relies on the observation string starting with 'Error'; legitimate tool outputs that begin with 'Error' could be misclassified.
- Exceptions from tool execution are surfaced as 'Error executing {call.Name}: {ex.Message}', which may reveal internal details.

---

## GenerateSummaryAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<string> GenerateSummaryAsync(
        string? previousSummary,
        IReadOnlyList<Message> toSummarize,
        ModelSelection selection,
        CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `previousSummary` | `string?` | — |
| `toSummarize` | `IReadOnlyList<Message>` | — |
| `selection` | [`ModelSelection`](../../Gabriel.Core/Configuration/ModelSelection.cs.md) | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<string>`


Generates a concise, factual recap of a conversation by streaming a model provider with a system prompt and the turns to summarize, returning the accumulated text as a trimmed string. Use it internally when the agent needs to produce an up-to-date summary of a chat, optionally folding in a previous summary.

## Remarks
This method centralizes the conversation-summarization workflow behind a provider-agnostic streaming interface. It constructs a history consisting of a system prompt and the user-assembled content, then consumes the provider's delta stream until a FinishEvent to produce a coherent summary. It preserves existing context when a previous summary is supplied and uses a placeholder for messages that contain tool calls, deferring tool-output handling to the summarization layer. By delegating the actual summarization to the configured provider, this function remains agnostic to the underlying model and formats results consistently for downstream consumers.

## Notes
- Tool content placeholders: If a message includes ToolCallsJson, this method substitutes '(requested tools)' in the input to the provider since it passes an empty ToolDescriptor list; actual tool outputs are not fetched in this path.
- Streaming dependency: The final text is composed from delta events produced by the provider and stops when a FinishEvent is received; if no deltas are produced, the returned string may be empty after trim.
- Cancellation aware: A CancellationToken is passed through to the provider, so callers can cancel long-running summarization.

---

## GetContextMetricsAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
public async Task<ContextMetrics> GetContextMetricsAsync(
        Guid conversationId,
        CancellationToken ct = default)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversationId` | `Guid` | — |
| `ct` | `CancellationToken` | `default` |

**Returns:** `Task<ContextMetrics>`


GetContextMetricsAsync computes a snapshot of the contextual metrics used for a given conversation. It validates the current user, loads the conversation with its messages scoped to that user, resolves the active model selection, and loads the relevant prompts (persona, project, memory, tools). It then builds an AgentContext from these pieces, derives a token breakdown, and returns a ContextMetrics object containing total tokens, the contextual window size, the compact threshold in tokens and ratio, the number of messages after the cut, and whether the conversation was summarized, along with per-block token tallies (system, project, memory, tools, and the conversation). This method should be used whenever you need backend-determined context metrics for display, logging, or analytics instead of re-deriving them client-side.

## Remarks
This abstraction centralizes the end-to-end preparation of metrics around a conversation's context. It coordinates authentication, data retrieval, model selection, prompt loading, and token estimation into a single, testable surface, ensuring consistent metrics across UI and analytics layers. By design, it fails fast for unauthenticated users and missing conversations, guarding sensitive data and making the contract explicit.

## Notes
- Requires an authenticated user; otherwise UnauthorizedAccessException is thrown immediately.
- If the requested conversation cannot be found or accessed for the user, NotFoundException is thrown (resource: Conversation, key: conversationId).
- The computed CompactThresholdTokens are derived from the selected window and ratio; if the window is not positive, the threshold is 0.


---

## LoadMemoryBlockAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<string?> LoadMemoryBlockAsync(Guid? projectId, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `projectId` | `Guid?` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<string?>`


Formats saved memories into a single system message by collecting memories for a given conversation and organizing them into two sections: user-scope (global) memories and project-scope memories. If there are no memories, it returns null; otherwise it builds a block starting with [Saved memories] and delegates the formatting of each entry to AppendMemory, finally trimming trailing whitespace.

## Remarks
This abstraction centralizes how persistent user and project metadata is delivered to the model. By separating user-scope and project-scope memories, it gives the caller precise control over what facts are available globally versus within a project, reducing cross-project leakage. It also normalizes the memory format with a header and clear sections so downstream prompt machinery can rely on a consistent structure.

## Notes
- If no memories exist for the given projectId, the method returns null, so callers should guard against injecting an empty block.
- The final string is produced by appending each memory via AppendMemory and then trimming trailing whitespace; changes to AppendMemory can affect the layout or content of the block.

---

## LoadProjectSystemPromptAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<string?> LoadProjectSystemPromptAsync(Conversation conversation, Guid userId, CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversation` | [`Conversation`](../../Gabriel.Core/Entities/Conversation.cs.md) | — |
| `userId` | `Guid` | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<string?>`


Constructs a project-scoped system prompt for a conversation and returns null when no non-default project is attached. When a non-default project is present, it declares the project name, advises memory: save project-scoped facts with scope='project' and user-scoped facts otherwise, and appends the project's SystemPrompt if provided.

## Remarks
Centralizes per-project context into a single, testable unit. It hides the details of how a project is looked up and how its metadata and optional SystemPrompt shape the memory-scope guidance, so callers simply receive a ready-to-use prompt or null when no project context applies.

## Notes
- If the conversation lacks a ProjectId or the project is default or missing, the method returns null; callers must handle absence gracefully.
- The method delegates to _projects.GetByIdAsync and does not catch exceptions; callers should propagate or handle cancellation.

---

## LoadTurnPromptsAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<TurnPrompts> LoadTurnPromptsAsync(
        Conversation conversation,
        Guid userId,
        ModelSelection selection,
        CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversation` | [`Conversation`](../../Gabriel.Core/Entities/Conversation.cs.md) | — |
| `userId` | `Guid` | — |
| `selection` | [`ModelSelection`](../../Gabriel.Core/Configuration/ModelSelection.cs.md) | — |
| `ct` | `CancellationToken` | — |

**Returns:** `Task<TurnPrompts>`


Loads and composes the per-turn prompts needed to drive the model for the current conversation. It gathers the project/system prompt, the memory block tied to the project, and a persona prompt built from the conversation state and mode, then selects the available tool descriptors based on the requested ToolMode. The resulting TurnPrompts instance is consumed downstream by the streaming pipeline (e.g., RunStreamAsync) to ensure consistent context, metrics, and tool availability across the turn.

## Remarks

By centralizing turn-context assembly in AgentService, the system guarantees that all code paths share the same turn context regardless of whether the underlying transport uses Native or Emulated tool descriptors. The cancellation token is threaded through all load operations, ensuring cooperative cancellation. The persona, project, and memory elements are assembled in a single TurnPrompts, so downstream components observe a consistent snapshot of the turn's context.

## Notes

- Tool availability depends on ToolMode; ToolMode.None yields an empty ToolDescriptor list.
- All four components (personaPrompt, projectPrompt, memoryBlock, and tools) are assembled in a single TurnPrompts instance to guarantee consistency across streaming and metrics.
- Array.`Empty<ToolDescriptor>`() is used when there are no tools to advertise, avoiding allocations.

---

## MaybeCompactAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async IAsyncEnumerable<AgentEvent> MaybeCompactAsync(
        Conversation conv,
        TurnPrompts prompts,
        ModelSelection selection,
        [EnumeratorCancellation] CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conv` | [`Conversation`](../../Gabriel.Core/Entities/Conversation.cs.md) | — |
| `prompts` | `TurnPrompts` | — |
| `selection` | [`ModelSelection`](../../Gabriel.Core/Configuration/ModelSelection.cs.md) | — |
| `ct` | `CancellationToken` | — |

**Returns:** `IAsyncEnumerable<AgentEvent>`


Decides whether to trigger a rolling-summary (compact) of the current turn by comparing the full AgentContext breakdown against the configured threshold, and if so, generates and applies a new summary for the most relevant recent messages. It yields events to coordinate UI and persistence (AgentCompactStart and AgentCompactDone) and updates the stored conversation when compaction succeeds. If the context already fits within the threshold or no meaningful cut can be made, it yields nothing.

## Remarks
This method centralizes the logic for deciding when long conversations should be condensed to fit within model/window limits. It relies on a precise accounting of the full contextual breakdown (persona, project, memory, summary, and tools) to decide whether compaction changes the visible history. It also coordinates with the persistence layer and token estimation to ensure the UI reflects the ongoing operation and that the stored state is updated atomically after a successful summary.

## Notes
- If the compact decision criteria are not met, or there is no valid segment to summarize, the method yields no events and exits gracefully.
- When a summary is attempted, a preliminary AgentCompactStart event is emitted so the UI can show a compacting overlay, followed by either AgentCompactDone with the resulting token count or AgentCompactDone with zero tokens if the operation fails or returns an empty summary.


---

## Preview
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private static string Preview(string? text)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `text` | `string?` | — |

**Returns:** `string`


It returns a compact, single-line preview of the provided text for logging or quick UI presentation. If the input is null or empty, it returns "(empty)"; otherwise, it collapses newline characters to spaces and truncates long previews using a trailing ellipsis when the length exceeds LogPreviewLimit.

## Remarks
This helper centralizes how previews are generated, ensuring consistent, readable log output across the code path that renders tool or user-provided content. It relies on the LogPreviewLimit barrier to keep previews short, and indicates continuation with a Unicode ellipsis, which communicates truncation without introducing extra lines.

## Notes
- Truncation appends a single-character ellipsis …, so the final string may be one character longer than LogPreviewLimit.
- It uses string.IsNullOrEmpty, so whitespace-only strings are treated as non-empty and will be processed rather than returning "(empty)".

---

## RegenerateAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
public async Task<IAsyncEnumerable<AgentEvent>> RegenerateAsync(
        Guid conversationId,
        Guid assistantMessageId,
        CancellationToken ct = default)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversationId` | `Guid` | — |
| `assistantMessageId` | `Guid` | — |
| `ct` | `CancellationToken` | `default` |

**Returns:** `Task<IAsyncEnumerable<AgentEvent>>`


RegenerateAsync replays the assistant's previous reply for a given conversation by regenerating the message using the same user state, and it streams back AgentEvent items as the regeneration progresses. Use it when you want a fresh take on a past turn without resending the user's prompt.

## Remarks
This method centralizes the regeneration workflow in the engineering surface: it coordinates authentication, domain validation, state mutation, and the streaming of events from the regeneration pipeline. It encapsulates the nuance of deactivating the prior variant group and reusing the same group id to preserve UI navigation paths between alternatives. By returning an `IAsyncEnumerable<AgentEvent>`, it allows callers to progressively react to the regeneration steps as they are produced rather than awaiting a single result. It relies on the surrounding infrastructure (conversations repository, unit of work, and the streaming runner) to ensure the regeneration respects permissions and history integrity.

## Example
```csharp
// Example usage: stream regeneration events for a specific conversation message
await foreach (var evt in agentService.RegenerateAsync(conversationId, assistantMessageId, ct))
{
    // handle each AgentEvent as it is produced
}
```

## Notes
- Requires an authenticated user; otherwise an UnauthorizedAccessException is thrown.
- If the conversation or target message cannot be found, a NotFoundException is thrown; if the target is not an active assistant variant, a DomainException is thrown.
- The selected variant group is deactivated and the conversation state is updated and persisted; the regenerated turn uses the same variant group ID to preserve UI navigation across alternatives.


---

## ResolveModelSelectionAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async Task<ModelSelection> ResolveModelSelectionAsync(CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `ct` | `CancellationToken` | — |

**Returns:** `Task<ModelSelection>`


Centralizes the resolution of the active model by first loading the user's preferences and then delegating to the model catalog to produce a ModelSelection for the chosen provider and model. This ensures that a change to the user's preferred provider or model is reflected on the very next turn, without duplicating the resolution logic at every entry point.

## Remarks
By funneling all callers through ResolveModelSelectionAsync, the system gains a single source of truth for mapping user preferences to a concrete ModelSelection. It cleanly separates concerns: preference storage and the catalog-based resolution are decoupled from the call sites that need the active model. This setup also simplifies testing, as you can mock the prefs store and the catalog to verify that changes in preferences propagate to the resulting ModelSelection.

---

## RunAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
public async Task<IAsyncEnumerable<AgentEvent>> RunAsync(
        Guid conversationId,
        string userInput,
        CancellationToken ct = default)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversationId` | `Guid` | — |
| `userInput` | `string` | — |
| `ct` | `CancellationToken` | `default` |

**Returns:** `Task<IAsyncEnumerable<AgentEvent>>`


Runs a single user turn in the chat agent workflow. It validates input, ensures an authenticated user, loads the target conversation with its messages, persists the user message and updates the conversation state, resolves the model to use, loads the per-turn prompts, configures the tool-execution context, and finally streams the per-turn events via RunStreamWithUserPreambleAsync.

## Remarks
This method centralizes all per-turn preparation and streaming so every turn shares a single, consistent surface: the chosen model, the loaded prompts, and the scoped tool context. Persisting the user message before streaming guarantees the conversation timeline remains coherent even if the client disconnects mid-turn; the subsequent stream reflects a single, authoritative turn that downstream components can rely on for rendering and metrics.

## Example
```csharp
// Example: consume the streaming events for a user turn
await foreach (var evt in agentService.RunAsync(conversationId, userInput, cancellationToken))
{
    // forward or render the streaming event to the client
    Console.WriteLine($"Event: {evt?.GetType().Name}");
}
```

## Notes
- The method will throw DomainException when the input message is empty, and UnauthorizedAccessException if there is no authenticated user. If the conversation cannot be found, NotFoundException is raised with details about the resource.
- The returned value is an `IAsyncEnumerable<AgentEvent>`, representing a streaming sequence of events (including preambles and post-processing events) that the client can consume as the model generates its reply.

---

## RunStreamAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async IAsyncEnumerable<AgentEvent> RunStreamAsync(
        Conversation conversation,
        Guid? variantGroupIdOverride,
        TurnPrompts prompts,
        ModelSelection selection,
        [EnumeratorCancellation] CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversation` | [`Conversation`](../../Gabriel.Core/Entities/Conversation.cs.md) | — |
| `variantGroupIdOverride` | `Guid?` | — |
| `prompts` | `TurnPrompts` | — |
| `selection` | [`ModelSelection`](../../Gabriel.Core/Configuration/ModelSelection.cs.md) | — |
| `ct` | `CancellationToken` | — |

**Returns:** `IAsyncEnumerable<AgentEvent>`


RunStreamAsync streams the assistant’s output by repeatedly rebuilding the conversation history and querying the active chat provider, yielding AgentEvent-derived deltas (text and reasoning) as they arrive and handling tool calls when the provider emits ToolCallReadyEvent. When tools are emulated, the provider is wrapped with GabrielToolBridge to inject tool docs; a finite retry loop protects against intermittent empty finishes, preserving history across iterations.

## Remarks
This symbol acts as the streaming funnel between the agent's decision loop and the underlying chat provider, encapsulating the nuances of tool integration (via GabrielToolBridge) and history management. It centralizes iteration-level context recomposition so that tool calls and tool results are preserved across turns, while isolating resilience logic (empty-stop retries) from higher-level orchestration.

## Notes
- It relies on an empty-stop retry strategy; if the provider consistently returns FinishEvent with no data, the method will retry up to EmptyStopMaxRetries, potentially delaying error reporting.
- In Emulated tool mode, it expects compatible tool descriptors to ensure tool docs are injected correctly; misconfiguration can cause tool docs to be omitted.

---

## RunStreamWithUserPreambleAsync
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private async IAsyncEnumerable<AgentEvent> RunStreamWithUserPreambleAsync(
        Guid userMessageId,
        Conversation conversation,
        Guid? variantGroupIdOverride,
        TurnPrompts prompts,
        ModelSelection selection,
        [EnumeratorCancellation] CancellationToken ct)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `userMessageId` | `Guid` | — |
| `conversation` | [`Conversation`](../../Gabriel.Core/Entities/Conversation.cs.md) | — |
| `variantGroupIdOverride` | `Guid?` | — |
| `prompts` | `TurnPrompts` | — |
| `selection` | [`ModelSelection`](../../Gabriel.Core/Configuration/ModelSelection.cs.md) | — |
| `ct` | `CancellationToken` | — |

**Returns:** `IAsyncEnumerable<AgentEvent>`


Produces an asynchronous stream of AgentEvent that starts by emitting an AgentUserMessagePersisted event for the provided userMessageId and then yields the events produced by RunStreamAsync with the given parameters (conversation, variantGroupIdOverride, prompts, selection, ct). The cancellation token is forwarded to RunStreamAsync, so cancellation requests terminate the underlying streaming.

## Remarks

This helper composes persistence signaling with streaming into a single sequence, guaranteeing that the user message is recorded before any agent-generated events are observed. This ordering prevents a scenario where streaming begins without a corresponding persisted user message, and it centralizes the preamble behavior so call sites can opt into the pre-persist signal without duplicating boilerplate.

## Notes

- The first yielded item is AgentUserMessagePersisted; if RunStreamAsync later fails, the preamble has already been produced, which can influence downstream error handling.
- The method is private; external callers should use RunStreamAsync directly when the preamble is not required.
- Enumeration is lazy and cancellation is propagated through ct to the underlying stream.

---

## SelectCompactCutIndex
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private static int SelectCompactCutIndex(IReadOnlyList<Message> messages, int keepLast)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `messages` | `IReadOnlyList<Message>` | — |
| `keepLast` | `int` | — |

**Returns:** `int`


SelectCompactCutIndex determines where to cut a message history when creating a compacted conversation for model input. It preserves at least the last keepLast messages from the end, then walks backward to find the most recent User message and uses that position as the cut boundary. This anchoring ensures the model sees an assistant's tool invocation and the corresponding tool results together, avoiding splits that would separate related tool calls and results.

## Remarks
This small helper centralizes the invariant that tool invocations and their results must not be separated by history trimming. It relies on the Message and MessageRole abstractions to identify the boundary at the most recent User message, making the boundary calculation deterministic and consistent across calls. By isolating this logic, callers can reliably trim history without duplicating boundary logic or risking misalignment between tool usage and outcomes.

## Notes
- If keepLast is greater than or equal to messages.Count, the function returns 0.
- If there is no User message encountered before reaching the start of the list, the function returns 0.

---

## SerializeToolCalls
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
private static string SerializeToolCalls(IReadOnlyList<ChatProviderToolCall> calls)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `calls` | `IReadOnlyList<ChatProviderToolCall>` | — |

**Returns:** `string`


SerializeToolCalls takes a read-only list of ChatProviderToolCall and produces a JSON string that represents the calls. It projects each item to a compact representation containing id, type, and a nested function with the Name and ArgumentsJson of the call, then serializes the array using JsonSerializer. The resulting string is a stable, transportable representation of tool-call activity that can be consumed by clients or logged for auditing.

## Remarks
- It centralizes the JSON shape of tool-call events to ensure consistency across consumers. It defines a stable, forward-compat JSON schema for tool calls, decoupling the in-memory model from the serialized form.
- It relies on the assumption that ArgumentsJson already contains a JSON payload; it does not attempt to parse or reformat that content.

## Notes
- This method does not guard against null input; callers must pass a non-null `IReadOnlyList<ChatProviderToolCall>`.
- The serialized output uses the literal property names id, type, function, name, and arguments as defined by the code; any changes to the anonymous type shape will alter the produced JSON.

---

## EmptyStopMaxRetries
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopMaxRetries = 2
```


Provides a bounded retry budget for the “provider finished Stop with empty text” hiccup inside the agent loop. When the response arrives as a successful HTTP 200 stream with empty content, the retry must reside in the agent rather than the HTTP resilience pipeline. This constant allows up to two additional attempts (three total including the initial try) to ride through transient blanks without noticeably delaying a real failure path. The backoff is linear: the delay before the Nth retry is N × EmptyStopRetryDelayMs, where EmptyStopRetryDelayMs is defined in the dependency surface.

## Remarks
By isolating the retry cap in a single constant, the agent loop stays readable and testable, and the policy can be adjusted without reworking loop logic. It also documents the intent behind the three-attempt boundary, grounding behavior in a concrete, maintainable policy. This abstraction clarifies that the three-attempt cap is a policy decision for transient empty-text events the HTTP resilience path won't address.

## Notes
- If EmptyStopRetryDelayMs is non-positive, the backoff becomes zero or negative; ensure it's a positive duration.

---

## EmptyStopRetryDelayMs
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopRetryDelayMs = 500
```


Represents the fixed delay (in milliseconds) used between retry attempts for the empty-stop scenario in AgentService. With a value of 500, it prevents busy-waiting by introducing a short pause between retries, contributing to more predictable timing during retry loops.

## Remarks
Centralizing this timing value in a private constant makes the retry behavior consistent and avoids scattering numeric literals across the class. The private scope signals that this is an internal tuning knob used only by AgentService; if configurability is needed, consider exposing it or moving it to settings. The 500 ms value balances responsiveness with CPU efficiency for typical retry storms.

## Notes
- This constant is baked into the compiled code; changing it requires recompilation and retesting.
- Because it's private, other components cannot reuse this delay; if reuse is needed, refactor to expose via a configurable option or internal API.

---

## LogPreviewLimit
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int LogPreviewLimit = 240
```


LogPreviewLimit defines the maximum number of characters from tool arguments or results that may be included in a log message. By capping log length, it prevents large payloads from bloating log files while preserving enough information for debugging. It is used internally by the AgentService when composing logs to keep messages concise and avoid leaking excessive data.

## Remarks

Having this as a private, compile-time constant ensures a single, predictable logging boundary across the codebase. The value is fixed at compile time, so changing it requires recompilation of dependents, making the boundary intentional and explicit. This helps reduce the risk of logging sensitive or oversized data inadvertently.

## Example

```csharp
string payload = new string('x', 1000);
string truncated = payload.Length > LogPreviewLimit ? payload.Substring(0, LogPreviewLimit) : payload;
Console.WriteLine(truncated);
```

## Notes

- Substring with a fixed length can throw if the source is shorter than the requested length; guard with a length check or use a safe truncation pattern (as shown in the example).
- Changing the value requires rebuilding all consuming code since it is a const.

---