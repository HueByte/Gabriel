# AgentService.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentService.cs`

## Contents

- [AgentService](#agentservice)
- [TurnPrompts](#turnprompts)
- [AgentService](#agentservice-1)
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


Orchestrates a single assistant turn for a conversation and exposes it as a streaming sequence of AgentEvent values. AgentService validates input and conversation state, resolves the model selection and per-turn prompts (persona, project system prompt, memory, tools), populates the tool execution context, runs the provider streaming loop, persists the user and assistant messages and updates conversation state, and applies compaction and response post-processing. Use this service when you want the full end-to-end agent behavior (prompt building, tool wiring, provider streaming, state persistence and post-processing) rather than calling lower-level providers or repositories directly.

## Remarks
This class centralises the high-level agent workflow: it coordinates repositories (conversation, project), the provider registry and model catalog, memory and tool services, the system-prompt builder, and the response post-processor. Two implementation details are exposed as design decisions: (1) a bounded retry policy for the provider "empty stop" condition (handled inside the agent loop because the HTTP layer receives a successful 200 streaming response), and (2) AgentService retains an `ILogger<GabrielToolBridge>` so it can instantiate GabrielToolBridge on demand for each call without needing a factory service.

## Notes
- RunAsync validates and persists the incoming user message up-front and may throw before any streaming begins; callers should expect synchronous validation errors (e.g. 4xx) to happen before the SSE/stream is opened.
- The agent implements a small, bounded retry loop for transient provider responses that finish with an empty assistant text: EmptyStopMaxRetries controls extra attempts (two extra attempts = three total) and retries use a linear backoff of N * EmptyStopRetryDelayMs.
- GabrielToolBridge is instantiated per-call and is treated as stateless across invocations; AgentService keeps only the bridge logger and creates the bridge when needed.

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


TurnPrompts is an internal, private record that bundles the per-turn prompts used to build an AgentContext. It captures the essential inputs—PersonaPrompt, optional ProjectPrompt, optional MemoryBlock, and the Tools collection—so AgentContext.Build can assemble the final prompts in one place rather than callers rearranging pieces.

## Remarks
Private, sealed and internal to the wiring for AgentContext.Build, TurnPrompts acts as a single source of truth for the inputs that drive per-turn prompting. It keeps public API surface stable by preventing outsiders from reordering or injecting pieces; the assembly happens at AgentContext.Build. The fields map directly to runtime inputs: a required PersonaPrompt, optional ProjectPrompt, optional MemoryBlock, and a Tools list of ToolDescriptor.

## Notes
- Do not rely on TurnPrompts from outside its containing type; it is an internal implementation detail and may change without notice.

---

## AgentService
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


Initializes an AgentService by wiring together its numerous collaborators via dependency injection and storing them on internal fields. This constructor does not perform work itself but establishes the runtime composition that enables the agent to access conversations, projects, tooling, memory, prompts, and logging during operation.

## Remarks
This constructor acts as the composition root for the Gabriel Engine's agent, centralizing the cross-cutting services needed at runtime such as tool invocation, memory, and prompt generation. By taking `IOptions<AgentOptions>` and unwrapping Options.Value, it ensures configuration is provided consistently and available to downstream components. This pattern also improves testability by allowing mocks or fakes of the collaborators to be supplied in unit tests.

## Notes
- Ensure all dependencies are registered in the DI container; missing registrations will throw during construction.
- Accessing options.Value assumes a configured AgentOptions, so configuration must occur at startup.

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


Formats a MemoryEntry into a Markdown block and appends it to the supplied StringBuilder. It writes a header that shows the memory's type (lowercased) and its name, then includes the memory's description and body, separated by blank lines. This helper consolidates the rendering logic for memory entries so the same style is used wherever documentation is produced.

## Remarks
Centering the formatting in one place ensures consistent presentation of memory entries across the generated docs and makes future styling changes straightforward. It operates purely on the MemoryEntry's Type, Name, Description, and Body and relies on the caller to supply the target StringBuilder. Because it is private, this method is an implementation detail of the surrounding document-generation path rather than a reusable API for external callers.

## Notes
- The method does not perform input validation; it assumes non-null arguments. Passing a null MemoryEntry or null properties can yield incomplete output or a NullReference at runtime.
- This is a private helper used by the document-generation path; external code should not rely on calling it directly.

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


Executes a named tool safely by locating it in the internal registry, invoking its asynchronous ExecuteAsync with cancellation support, and returning the textual observation. It handles missing tools gracefully by returning a standard error and logs the event; it distinguishes soft errors (strings starting with 'Error') from successful results and catches exceptions, converting them to user-facing error messages.

## Remarks
At its core, this method acts as an orchestration wrapper around tool invocations. It centralizes cross-cutting concerns: lookups, latency measurement, structured logging, and consistent error signaling. By treating 'soft' errors as non-fatal warnings, it preserves the normal flow while making failures observable for operators and diagnostics. The cancellation token is threaded to the underlying tool, enabling cooperative cancellation.

## Notes
- Soft-errors are detected by observing if the tool's result starts with the prefix "Error" (case-insensitive). If a tool returns an error string that does not begin with this prefix, it will be treated as a non-error result.
- If the requested tool name is not registered, the method returns the string "Error: tool '<name>' is not registered." and logs a warning.
- The method never throws; it catches exceptions from tool execution, logs them as errors with elapsed time, and returns a user-friendly error string like "Error executing <tool>: <message>". Also, if the tool returns null, the method yields an empty string.


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


Generates a concise, per-call summary by constructing a structured chat prompt and streaming the assistant’s response. It accepts an optional previously computed summary, a list of messages to summarize, a model/provider selector, and a cancellation token. The method defines a system prompt that enlists the model as a dedicated, factual summarizer and then builds the user prompt from either the existing summary plus new turns or a fresh list labeled as the conversation to summarize. Each message in the toSummarize collection is appended in the form [Role] content, using fallback labels like "(requested tools)" when tool calls are present or "(no content)" when content is missing. A history consisting of the system prompt and the user content is sent to the resolved provider, which streams delta updates until a FinishEvent is observed. The accumulated deltas form the final summary string, which is returned trimmed.

Dependencies: Message, ChatProviderMessage, ToolDescriptor, StringBuilder, MessageRole, Array

## Remarks
This method centralizes the summarization interaction behind a single, provider-agnostic surface. By standardizing the system prompt and the user payload, it ensures consistent behavior across different chat providers and models. The incremental capability (accepting an optional previousSummary) enables folding new turns into an existing summary without reprocessing the entire history, which is essential for long-running or interactive sessions.

## Notes
- The provider is resolved via _providerRegistry using the supplied ModelSelection; mismatched providers may yield different formatting or capabilities.
- Tool usage within summarized turns is signaled by placing "(requested tools)" when ToolCallsJson is present, since the actual tool results aren’t embedded in the summary payload here.
- The method streams text deltas until a FinishEvent, so partial results are collected progressively; cancellation via the token is respected. 

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


Retrieves a ContextMetrics snapshot for a specific conversation by validating the caller, loading the full conversation with its messages, resolving the active model selection and its context window, loading the relevant prompts, building the agent context, and computing a breakdown of token usage across all components. The returned metrics enable the UI to render trigger lines consistently with backend processing, show whether the conversation is summarized, and display token attribution for system, project, memory, tools, and the conversation.

## Remarks
This method centralizes authentication, data access, model resolution, and token accounting behind a single API boundary, so callers don't need to reimplement these concerns. By deriving the breakdown via AgentContext.ComputeBreakdown, it guarantees consistent token attribution across the various prompt origins (system, project, memory, tools) and the message history. Using the same threshold ratio as the backend's calculation path ensures the UI's trigger visualization lines up exactly with the actual processing boundary.

## Notes
- Throws UnauthorizedAccessException when the current user is not authenticated. 
- Throws NotFoundException if the specified conversation cannot be found for the user. 
- CancellationToken ct is honored by all asynchronous operations and is propagated through the call chain.

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


Formats the user's saved memories as a single system message for the model. It groups memories by scope (user-scope first, project-scope after) so the model can tell which entries apply globally versus only within this project, and it returns null when there is nothing to inject. It fetches the memories, builds a memory block starting with [Saved memories], describes each entry's scope, type, and body, and delegates the per-entry formatting to AppendMemory, placing user-scope memories under '## User-scope memories' and project-scope memories under '## Project-scope memories' in that order.

## Remarks
Centralizes how saved memories are transformed into a system prompt, ensuring a consistent structure across callers. It separates global (user-scope) memories from project-scoped ones to preserve the intended applicability, and it delegates the actual entry formatting to AppendMemory, so changes to memory rendering stay isolated from retrieval logic.

## Notes
- Returns null when there are no memories to inject; callers must handle the null to avoid injecting an empty system block.
- The formatting of individual memories is delegated to AppendMemory; changes to that helper will affect the appearance of the memory block.

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


LoadProjectSystemPromptAsync is an internal helper that resolves a per-project system prompt for a conversation. When the conversation is associated with a non-default project, it loads the project data and builds a prompt that identifies the project and educates memory-scoping behavior, including a recommendation to scope memory to the project for project-specific facts and to use the user scope otherwise. If the project defines a SystemPrompt, that content is appended to the assembled prompt. If there is no ProjectId, or the project cannot be loaded, or the project is the default, the method returns null, signaling that no project-specific augmentation is needed.

## Remarks
This abstraction centralizes the construction of project-scoped context, ensuring consistent guidance for memory persistence across conversations tied to a given project. By returning null in non-applicable cases, it cleanly separates project-specific augmentation from generic, cross-project behavior and avoids injecting unnecessary context.

## Notes
- Returns null when there is no eligible project or when the project is the default.
- The method is private and intended to be used within its containing class to assemble project-scoped prompts consistently.
- It relies on a repository lookup to fetch project data and uses StringBuilder for efficient string construction.

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


Loads and assembles the per-turn prompts used to drive a single assistant turn. It asynchronously fetches the project-specific system prompt, the memory block for the current project, and a persona prompt built from the current conversation state and mode, then selects the available tool descriptors based on the active ToolMode. The method returns a TurnPrompts instance containing the persona prompt, the project prompt, the memory block, and the set of tool descriptors; this object is consumed by the provider call and UI metrics pathways to ensure the turn reflects the latest context and capabilities.

## Remarks
Centralizes per-turn prompt composition to guarantee consistency across streaming and non-stream paths. By constraining the advertised tools to the current ToolMode, it prevents exposing capabilities the model can't use and aligns the runtime prompts with user intent.

## Notes
- The fetches are performed sequentially; parallelizing them could reduce latency but would require ensuring a single, consistent snapshot of prompts.

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


Decides whether to trigger a rolling-summary compact this turn by comparing the full agent context token footprint against the configured compact threshold. It builds a complete breakdown of persona, project, memory, summary, tools, and the current conversation, and uses either the per-model CompactThreshold or the global AgentOptions.CompactThreshold to determine if trimming is warranted. When the current token load exceeds the threshold, it selects a cut point through the conversation (respecting CompactKeepLast and previously summarized ground), gathers the messages to summarize, and requests a new summary via GenerateSummaryAsync. The method frontloads an AgentCompactStart event so the UI can display a compacting overlay, then performs the potentially slow summarization. If the summary call fails or returns an empty result, it emits AgentCompactDone with zero tokens and lets the turn proceed un-compacted, while logging the issue. On success, it updates the conversation with the new summary, persists changes, logs the outcome with token counts, and emits AgentCompactDone with the number of messages compacted and the resulting summary token count. This symbol therefore centralizes the decision and orchestration for rolling context maintenance, ensuring long conversations stay within provider limits while keeping the visible summary up to date.

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


Produces a compact, single-line preview of the provided text for logging or concise display. If the input is null or empty, it returns the literal string '(empty)'. The method collapses newline characters to spaces so multi-line content stays on a single line, then returns either the full flattened string or a truncated version up to the configured LogPreviewLimit, appending a Unicode ellipsis to indicate omitted content.

## Remarks
Internal helper that centralizes how textual previews are produced for logs and UI fragments within the service. It guarantees that multi-line input does not blow up log lines and that nulls are represented by a clear placeholder rather than an empty line. By delegating this logic to a single method, all call sites share a consistent representation.

## Notes
- Truncation relies on the LogPreviewLimit threshold; longer text is cut off and suffixed with '…'.
- The length calculation is character-based, not bytes, so surrogate pairs or combining characters can affect when truncation happens.
- Null or empty input yields '(empty)' rather than an empty string.

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


RegenerateAsync orchestrates a server-side regeneration of a previous assistant reply within a specified conversation by replaying the same user state and selecting a fresh model path for that turn. It validates authentication, loads the target conversation with its messages, and ensures the target is an active assistant message before deactivating its variant group and persisting the change. After determining the current model selection, it loads the necessary prompts for regeneration, resets the per-conversation tool context, and streams the new events via RunStreamAsync, yielding a refreshed assistant reply while keeping the original variant group identity so the UI can navigate between alternatives.

## Remarks
RegenerateAsync exists to provide a non-destructive way to refine an assistant turn by re-running the same user input against the model pipeline while preserving the surrounding history. It coordinates state transitions around variant groups to ensure future history assemblies don't resurrect the old reply and to keep the UI's variant-picker coherent. By reusing the same groupId, the client can continue to present alternative replies side-by-side.

## Example
```csharp
// Example usage: stream regeneration results for a given turn
await foreach (var evt in agentService.RegenerateAsync(conversationId, assistantMessageId, ct))
{
    // Process each AgentEvent as it arrives (e.g., update UI, log, etc.)
}
```

## Notes
- The method may throw UnauthorizedAccessException if there is no authenticated user, NotFoundException when the conversation or target message cannot be found, or DomainException if the target message is not an assistant message or is not an active variant.
- The regeneration does not introduce a new user message and relies on the original turn's state; compaction happens inside RunStreamAsync as the stream yields events.
- This operation streams AgentEvent objects, allowing callers to react to updates in real time as the regeneration unfolds.

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


Resolves the current ModelSelection by reading the user's preferences asynchronously and translating them through the model catalog. This centralized helper ensures all code paths select the same model configuration, so changes made on the settings page take effect on the very next turn.

## Remarks

By funneling provider and model choice through a single resolution point, this method decouples storage of preferences from how the runtime resolves which model to use. It guarantees a consistent mapping from the user's PreferredProvider and PreferredModel to a runtime ModelSelection, avoiding divergent behavior across entry points. The comment in code notes that changes propagate on the next turn, and this method enforces that timing.

## Notes

- OperationCanceledException may be thrown if ct is canceled or if underlying calls fail; this method does not swallow exceptions and callers should handle them accordingly.

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


Runs a single turn of the per-conversation agent by validating input, loading the relevant conversation context, persisting the user message, updating state, resolving the model to use, assembling stable turn prompts, and then streaming the turn’s events as an `IAsyncEnumerable<AgentEvent>`.

This method validates inputs up-front to allow a global error handler to return a 4xx with ProblemDetails for bad user content, requires an authenticated user, and loads the target conversation along with its messages. It persists the user message before streaming so the timeline remains consistent even if the client disconnects during the turn. The per-turn model selection is resolved once and passed through all helpers to ensure consistent behavior across the compacting, provider call, and metrics calculations. Turn prompts (persona, project scope, memory, and tools) are loaded once and reused for the entire turn, and a scoped tool context is established so project-scoped tools operate against the correct project.

Finally, the method delegates to RunStreamWithUserPreambleAsync to produce the streaming sequence. The wrapper yields a preamble that includes the persisted user message’s real DB id and then streams the model’s events, including any compacting progress (e.g., CompactStart/CompactDone) as the response is generated.

In short: this is the high-level orchestrator for a single chat turn that yields real-time, event-based feedback to the client rather than a single, blocking reply.

## Remarks
This symbol serves as the orchestration hub for a turn in the interactive agent experience. By centralizing validation, authentication, persistence, state evolution, model selection, and prompt assembly, it guarantees a consistent lifecycle for every user turn and a predictable stream of AgentEvent objects. The selected model and the assembled prompts are threaded through all downstream steps, ensuring alignment between decision boundaries, tool descriptors, and the resulting stream. The streaming pattern supports responsive UIs via SSE-like clients, enabling users to see interim progress while the model generates the final reply.

## Notes
- If userInput is null or whitespace, a DomainException is thrown to map to a 4xx response; callers should handle DomainException in the API pipeline.
- If there is no authenticated user, an UnauthorizedAccessException is thrown, signaling a 401 response at the API boundary.
- If the conversation cannot be found for the given user, a NotFoundException is thrown with the conversation resource and id, signaling a 404 response.
- The method returns an IAsyncEnumerable; consumers must iterate the sequence to trigger the streaming process and side effects.


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


Runs a streaming session against a chat provider to produce AgentEvent updates while coordinating tool execution and context history. It first emits any pre-existing compacted events, then selects the active provider (wrapping it in GabrielToolBridge when tool emulation is enabled). It iterates up to MaxIterations, rebuilding the conversation history for every pass and streaming events from the provider, translating them into AgentEvent payloads (text deltas, reasoning deltas) and queuing tool calls that the agent can execute. If the provider returns an empty result accompanied by a Stop finish reason, it retries a bounded number of times with delays to recover from transient hiccups. The method yields events as they arrive, and the per-iteration context ensures that tool calls and their results become part of the next history.

## Remarks
This method acts as the orchestration boundary between the high-level conversation and the tool-augmented provider. It supports both native and emulated tool modes by optionally wrapping the base provider with GabrielToolBridge to inject tool documentation into the system prompt. Rebuilding the history per iteration ensures that tool calls and results from earlier iterations are preserved in subsequent invocations, preserving causal flow across the interactive loop.

## Notes
- This is a private method; its usage is internal to the agent service and not part of the public API.
- CancellationToken ct is threaded through to the provider stream; canceling it will terminate the streaming loop.
- The retry-on-empty behavior relies on EmptyStopMaxRetries and EmptyStopRetryDelayMs to recover from transient hiccups, which can affect latency under flaky provider responses.

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


Emits a preamble into the agent event stream by first yielding a AgentUserMessagePersisted event for the given userMessageId, and then streaming the remainder from RunStreamAsync. This pattern ensures the consumer observes that the user's message has been persisted before processing subsequent agent events produced for the conversation.

## Remarks
By separating the persistence acknowledgement from the streaming logic, this wrapper centralizes the preamble behavior and guarantees ordering: the preamble appears before any events from RunStreamAsync. The method delegates the heavy lifting to RunStreamAsync, so changes to streaming behavior remain isolated to that implementation. The ct cancellation token is passed through, ensuring consumers can cancel the whole sequence gracefully.

## Notes
- The first element of the returned IAsyncEnumerable is a persistence event, not a reply event; downstream code should handle it accordingly.

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


SelectCompactCutIndex determines where to trim a message history by keeping at least keepLast messages from the tail and then stepping backward to the nearest User message; the resulting index marks a cut boundary. This prevents splitting an assistant's tool_calls from the corresponding tool results, ensuring the model sees both sides of a tool interaction in one prompt.

## Remarks
By centralizing this boundary logic, the function encodes a subtle but important rule for history truncation: preserve user-facing context while keeping tool interactions whole. The cut index is used by higher-level code to produce a compacted history without breaking the linkage between a tool call and its tool results, which improves the model's ability to reason about the outcome.

## Notes
- Caller must ensure keepLast >= 1; a value of 0 would result in indexing messages[Count] and can throw.
- If no User message is found before the start of the list, the function returns 0, effectively cutting at the start.

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


Serializes a collection of ChatProviderToolCall objects into a compact JSON array. For each call, it emits an object with id, a fixed type of 'function', and a nested function descriptor carrying the call's Name and ArgumentsJson. This helper is useful when you need a transport-friendly, platform-agnostic representation of tool invocations rather than the full rich object.

---

## EmptyStopMaxRetries
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopMaxRetries = 2
```


Bounded retries for the 'provider finished Stop with empty text' hiccup. This field defines how many additional retry attempts are performed inside the agent loop when an empty response is observed after a Stop operation. The HTTP resilience pipeline cannot catch this edge because the response is a 200 stream with empty content, so the retry has to live in the application layer. With EmptyStopMaxRetries set to 2, there are three total attempts (the initial try plus two retries). The retries use a linear backoff: on the N-th retry, the delay is N times EmptyStopRetryDelayMs.

## Remarks
This small constant encodes a resilience strategy: it bounds the cost of a rare, non-fatal anomaly while preserving responsiveness to real failures. It pairs with EmptyStopRetryDelayMs to tune the latency of recovery and to keep the retry logic localized in the agent loop rather than leaking into higher layers. In short, it prevents an endless loop on transient blanks while avoiding punitive delays on persistent failures.

## Notes
- The effective number of attempts is 1 + EmptyStopMaxRetries (three total when EmptyStopMaxRetries is 2).
- Misinterpreting this value as the total allowed attempts can lead to under- or over-retrying; adjust with care.

---

## EmptyStopRetryDelayMs
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopRetryDelayMs = 500
```


This private constant defines the delay, in milliseconds, between consecutive stop-retry attempts in the AgentService. A value of 500 corresponds to half a second between retries, helping to balance responsiveness against busy-wait risk when stopping the service under load.

## Remarks
Centralizes retry pacing to a single, named constant rather than scattering the magic number in the stop logic; this makes it easier to tune behavior without changing control flow.
Because it is declared as const, the value is baked into the assembly at compile time, signaling that this is an internal tuning lever rather than runtime configurability.

## Notes
- Changing the value changes how long a stop retry may take during idle or contention, so adjust with care and validate shutdown latency.
- It's a compile-time constant; to change it, a rebuild is required, and there is no runtime configuration exposed by this symbol.

---

## LogPreviewLimit
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int LogPreviewLimit = 240
```


Defines the maximum number of characters included in log messages that describe tool arguments or results, preventing log files from ballooning with large payloads. Developers rely on it to produce a concise preview of potentially big data during AgentService logging, rather than dumping full payloads.

## Remarks
This private constant centralizes the log-preview limit for the AgentService's logging logic, ensuring consistent truncation across all payload previews. By capping previews, it helps protect sensitive data from being inadvertently logged and preserves log readability and performance. Because the value is compile-time and private, changes are isolated to this class, avoiding unintended ripple effects on public APIs.

## Example
```csharp
// Example usage within the AgentService class
string payload = "some potentially long payload text that should be truncated for logs";
string preview = payload.Length > LogPreviewLimit
    ? payload.Substring(0, LogPreviewLimit) + "…"
    : payload;
```

## Notes
- The constant is private; it cannot be modified at runtime. To change the limit, the code must be updated and the project rebuilt.
- 240 characters is a practical default to balance useful context with log size and privacy considerations; adjust with awareness of logging and compliance constraints. 

---