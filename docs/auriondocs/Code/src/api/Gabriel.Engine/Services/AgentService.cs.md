# AgentService.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentService.cs`

## Contents

- [AgentService_overview](#agentservice_overview)
- [TurnPrompts](#turnprompts)
- [AgentService](#agentservice)
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

## AgentService_overview
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** class

```csharp
public class AgentService : IAgentService
```


A long-lived service that orchestrates a single agent turn for a conversation: validating and persisting the incoming user message, selecting the model to use, assembling the turn-level system prompt (persona, project, memories, tools), creating a scoped tool execution context, and streaming the agent's events back to the caller via an `IAsyncEnumerable<AgentEvent>`. Reach for this service when you need to run a conversational agent turn end-to-end (including tool invocation, prompt construction, compaction and post-processing) rather than calling a provider or model directly.

## Remarks
The implementation centralizes per-turn responsibilities that must remain consistent across retries and across the streaming pipeline: model selection is resolved once up-front, stable prompt pieces are loaded once for the turn, and the conversation state + incoming user message are persisted before streaming begins so timeline semantics remain correct even on client disconnects. A small retry loop (bounded and linear-backoff) lives inside the agent loop specifically to work around a provider-level hiccup where the provider returns a successful stream with an empty final text; HTTP resilience layers cannot observe this because the HTTP response is already "successful".

AgentService also creates GabrielToolBridge instances on demand for a call; it holds the bridge logger so the lightweight bridge can be instantiated per-call without requiring a full factory abstraction.

## Example
```csharp
// Stream events from a single agent turn and handle them as they arrive.
await foreach (var agentEvent in agentService.RunAsync(conversationId, userInput, cancellationToken))
{
    switch (agentEvent.Type)
    {
        case AgentEventType.PartialResponse:
            // write partial text to client
            break;
        case AgentEventType.ToolInvocation:
            // execute tool and feed result back into the loop
            break;
        case AgentEventType.Complete:
            // finalize persistence / metrics
            break;
    }
}
```

## Notes
- The service implements a bounded retry for the specific case of a provider returning an empty final text: there are two additional retries (three attempts total) with a linear backoff (attempt N waits N * EmptyStopRetryDelayMs). This is deliberate and not intended as a generic retry policy.
- Validation and persistence happen before SSE/stream headers are sent so that failures surface as regular HTTP error responses (4xx/ProblemDetails) instead of an interrupted stream.
- Model selection, prompt pieces (persona/project/memory/tools), and tool-mode decisions are resolved once at turn start and flowed through helpers to ensure consistent behavior (compaction thresholds, metrics and provider calls) during the entire turn.

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


TurnPrompts is a private, immutable bundle of the inputs used to build a single turn of the AgentContext; it groups the persona prompt, optional project and memory blocks, and the list of available ToolDescriptor items so AgentContext.Build can assemble the turn without callers reordering pieces.

## Remarks
This internal type ensures the per-turn prompt components are captured atomically and cannot be shuffled by external code; grouping them into a private record preserves invariants and simplifies validation. The Tools collection is exposed as `IReadOnlyList<ToolDescriptor>`, signaling that the set of tools for a given turn is fixed at construction time; the actual list is prepared by the surrounding AgentContext.Build and should not be modified after capture.

## Notes
- Internal implementation detail: TurnPrompts is private and not part of the public API; its structure can change without warning across revisions.
- Nullable fields: ProjectPrompt and MemoryBlock are optional; consumers should account for potential null values when composing or interpreting a turn.
- Immutability discipline: Tools is an IReadOnlyList, which prevents direct mutation via the interface; treat the captured list as a snapshot and avoid mutating the underlying collection after creation.

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

**Returns:** `public`



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


Formats a Core.Entities.MemoryEntry as a Markdown fragment and appends it to the provided StringBuilder. It emits a header like "### [type] Name", with the memory type converted to lowercase and placed in brackets, followed by the memory's Description on the next line, a blank line, and then the memory's Body, followed by another blank line. This helper centralizes the rendering of memory entries when the AgentService builds its documentation, ensuring a consistent Markdown schema across entries.

## Remarks
This method encapsulates the presentation concern for memory entries, forcing a uniform Markdown layout so that all memory documentation produced by the AgentService looks and reads the same way. By pulling the formatting logic into a single place, changes to the header style, separators, or spacing affect every entry consistently and predictably.

## Example
```csharp
// Example of the fragment produced for a memory entry
// m.Type = MemoryType.Feature, m.Name = "Logging", m.Description = "Enables structured logging.", m.Body = "Detailed rules and examples..."
// Generated fragment (conceptual):
// ### [feature] Logging
// Enables structured logging.
//
// Detailed rules and examples...
```

## Notes
- The content is injected directly into the fragment (no escaping performed). If Description or Body contain Markdown syntax, it will be rendered as-is.
- This method assumes non-null Description and Body; callers should ensure MemoryEntry fields are populated to avoid empty lines in the output.


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


Documentation for symbol 'ExecuteToolSafelyAsync' has been submitted. The narrative describes its safe, logged, and timed tool invocation behavior, the soft-error heuristic, and relevant caveats around cancellation and null/unknown results.

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


Generates a concise, factual summary of a conversation by optionally folding in a previously produced summary and a collection of messages to summarize. It builds a system prompt and the serialized turns, then streams a final summary from a pluggable backend configured by ModelSelection and returns the accumulated text.

## Remarks
By decoupling summary generation from the caller through a pluggable provider registry, this method enables swapping AI backends or models without modifying the caller. It streams the summary as deltas, allowing progress to be consumed incrementally while still producing a complete string once finished. The prompt is tailored to produce a concise, factual recap that preserves key facts, decisions, ongoing threads, and user preferences, while omitting greetings and non-essential chatter. The method also cleanly folds an existing summary with new turns to maintain continuity.

## Notes
- Content placeholders: If a message's Content is null and ToolCallsJson is present, the text "(requested tools)" is inserted; otherwise the text "(no content)" is inserted. This can introduce placeholders into the final summary.
- No tools are supplied to the summarization provider (an empty ToolDescriptor array); if tool-assisted summarization is required, adjust the invocation accordingly.
- The method relies on a streaming provider that yields TextDeltaEvent and FinishEvent; cancellation or a missing FinishEvent can cause the operation to hang or be aborted unexpectedly.

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


Computes and returns a ContextMetrics object for the specified conversation. It validates the authenticated user, loads the conversation with its messages, resolves the current model selection and context window, calculates threshold tokens based on the window and the selected ratio, loads the turn prompts, builds an AgentContext, derives a token breakdown, and finally returns a ContextMetrics that exposes totals and per-group token counts along with metadata such as whether the conversation has been summarized and how many messages remain after context reduction.

## Remarks
By centralizing the construction of the context metrics, this method ensures consistent measurements across the UI and server logic. It ties together user access, conversation retrieval, prompt assembly, and token accounting, yielding a single authoritative snapshot of context usage for a given conversation. The calculation of threshold tokens is aligned with the UI trigger logic (matching MaybeCompactAsync) to avoid UI/backend drift.

## Example
```csharp
// Example usage
var metrics = await agentService.GetContextMetricsAsync(conversationId, cancellationToken);
// Access metrics such as metrics.CurrentTokens, metrics.ContextWindowTokens, metrics.CompactThresholdTokens, etc.
```

## Notes
- UnauthorizedAccessException is thrown when the caller is not authenticated.
- NotFoundException is thrown if the specified conversation cannot be found.
- If the selected window is zero or negative, CompactThresholdTokens will be zero; the threshold derives from either the selected CompactThreshold or the global option.


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


Formats the user's saved memories as a single system message. It loads memory entries for the specified conversation, then presents them in two sections: user-scope memories (apply everywhere) first, followed by project-scope memories (apply only within this project). Each entry is formatted by AppendMemory. If there are no memories to inject, the method returns null.

## Remarks
The abstraction centralizes how durable user memories are woven into prompts, ensuring consistent formatting and clear separation of global versus project-scoped context. By ordering user-scope memories before project-scope ones, it preserves broad continuity while respecting project boundaries. The actual formatting of each memory item is delegated to AppendMemory, keeping this method focused on orchestration and grouping.

## Example
```csharp
// Example output structure (structure shown for illustration; actual formatting is delegated to AppendMemory)
[Saved memories]
Durable facts the user has asked Gabriel to remember. Apply these unless the user contradicts them in the current conversation. Each entry has a scope (user = applies everywhere; project = only in this project), a type (user/feedback/project/reference), and a body.

## User-scope memories
- Remembered: The user prefers concise answers.
- Remembered: The user values privacy and wants data kept private.

## Project-scope memories
- Project Alpha: Deadline is 2025-12-31.
- Project Beta: Key stakeholder is Alice.
```

## Notes
- Returns null when there are no memories to inject; consumers must handle absence gracefully.
- The output order is always user-scope first, then project-scope.
- The per-entry formatting is delegated to AppendMemory; changing AppendMemory could affect the overall appearance.

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


Loads and returns an optional, project-scoped system prompt for the current conversation. If the conversation has a ProjectId, the method fetches the project; on success and if the project is not the default (legacy) entry, it builds a snippet starting with a line that declares the project name and a guidance line about memory scope (preferring scope='project' for project-specific facts and using scope='user' only when memory should persist beyond the project). If the project defines a SystemPrompt, that prompt is appended after a blank line. If there is no ProjectId, or the project cannot be found, or it is the default project, the method returns null.

## Remarks
Encapsulates the project-scoped prompt assembly logic, centralizing how project data influences memory prompts. By isolating the retrieval and assembly in one place, callers avoid duplicating project checks and guarantee consistent guidance across prompts. It also gracefully handles missing or default projects by returning null rather than failing.

## Notes
- Return value can be null; the caller must handle.
- If a non-default project exists but SystemPrompt is empty/whitespace, only the header line is included.
- This method is asynchronous and accepts a CancellationToken ct; callers should await and may cancel.

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


Loads and aggregates the per-turn prompts for a given conversation turn by fetching the project system prompt, memory block, and the persona prompt derived from the current conversation state and mode, then selecting tool descriptors according to ToolMode. The method returns a TurnPrompts object that bundles these four pieces so downstream steps (compact decision, streaming, and UI metrics) observe a single, coherent snapshot of context and capabilities for the turn.

## Remarks
By centralizing prompt composition, this method guarantees that all parts of the turn pipeline share the same inputs and decisions. It separates the concern of data gathering from the rendering or transport layers, reducing drift between the model's context and the available tools. The ToolMode switch is explicit: ToolMode.None yields an empty descriptor list to avoid advertising capabilities the model cannot use, while other modes provide the full set through descriptors.

## Example
```csharp
// Example usage within the agent's turn flow (note: method is private; shown for conceptual clarity)
var prompts = await LoadTurnPromptsAsync(conversation, userId, selection, cancellationToken);
```

## Notes
- If ToolMode.None, the returned tools list is empty, ensuring no tooling capabilities are advertised to the model.
- The prompts are loaded sequentially within this method; failures in any step propagate as exceptions to the caller.
- The memory block is retrieved using the project identifier from the conversation; ensure the project context is initialized prior to calling this method.

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


Decides whether to trigger a rolling-summary compact during the current turn. It builds the full AgentContext from the conversation, persona, project, memory, tools, and current summary, then compares the resulting token breakdown against a threshold derived from the model’s context window and the configured CompactThreshold. If the threshold is exceeded, it selects a block of recent messages to summarize and emits an AgentCompactStart event to drive a UI overlay while the potentially slow summarization runs. If summarization succeeds, it updates the conversation with the new summary, persists changes, and emits AgentCompactDone with the number of messages compacted and the summary’s token count; if the summarization fails or returns empty, it logs a warning and emits AgentCompactDone with zero tokens, allowing the turn to proceed un-compacted. The method is invoked as part of the engine's turn processing to balance prompt complexity and cost by keeping the visible conversation within the provider’s window size.



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


Produces a compact, single-line preview of the given text for logs or lightweight displays. If the input is null or empty, it returns the literal string "(empty)". It collapses newline characters into spaces so multi-line content stays on one line, then truncates the result to the configured LogPreviewLimit, appending an ellipsis when the text exceeds the limit.

## Remarks
Centralizes the preview formatting inside AgentService to ensure consistent handling of nulls, line breaks, and length-limiting across all callers. By encapsulating this logic in a single helper, changes to the preview behavior (such as a different limit or how newlines are collapsed) affect all usages in one place rather than scattering similar code throughout the class.

## Example
```csharp
// Example: multi-line text is collapsed to a single line
var input = "alpha\nbeta\r\ngamma";
var result = Preview(input); // e.g., "alpha beta  gamma" (single line, truncated with '…' if too long)
```

## Notes
- Null or empty inputs yield "(empty)".
- CR and LF characters are replaced with spaces; multiple spaces may appear for Windows CRLF sequences.
- The trailing ellipsis uses the Unicode character "…"; ensure your log viewer supports it.

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


RegenerateAsync re-creates a new assistant reply for a specific message in a conversation by reusing the existing user state and streaming the resulting events. It validates the caller, ensures the target message is an active assistant message, deactivates the current variant group so older variants are hidden from history, then loads prompts and the model selection before streaming the regenerated results via RunStreamAsync. This method is used when a user wants to iterate on an assistant turn and explore alternative variants without re-sending the user input.

## Remarks
Serves as a high-level orchestration that ties together authentication, data access, variant management, and streaming of results. By deactivating the variant group, it guarantees the UI and history assembly present only the active path while still allowing navigation to other alternatives within the same group. The regeneration is performed against the original user state, and compaction is performed inside RunStreamAsync to produce a continuous event stream.

## Example
```csharp
// Example usage: asynchronously stream regenerated events for a past assistant message
await foreach (var ev in agentService.RegenerateAsync(conversationId, assistantMessageId, ct))
{
    // Handle the streamed AgentEvent (e.g., render to UI, log, etc.)
}
```

## Notes
- Requires an authenticated user; unauthenticated calls throw UnauthorizedAccessException.
- The target message must be an active assistant message; otherwise a NotFoundException or DomainException is thrown.
- Deactivates the variant group and persists changes; callers should refresh their view accordingly.
- This method returns a streaming sequence; use await foreach and avoid eagerly enumerating to prevent buffering large results.

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


Centralised model-resolution helper: all entry points route through this private method to determine the active ModelSelection from the current user preferences. It fetches the preferences with _userPrefs.GetAsync using the provided CancellationToken and then delegates to _modelCatalog.Resolve to produce a ModelSelection based on prefs.PreferredProvider and prefs.PreferredModel. This design ensures that changes to user settings take effect on the very next turn by funneling resolution through a single, authoritative path.

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


Orchestrates one turn of the ReAct-powered assistant for a given conversation: it validates the input, persists the user message, resolves a model upfront, loads the per-turn prompts, establishes the project-scoped tool context, and streams the turn’s events as an `IAsyncEnumerable<AgentEvent>` via RunStreamWithUserPreambleAsync.

## Remarks
This symbol acts as the single turn orchestrator in the chat workflow, centralizing validation, persistence, model resolution, prompt assembly, and streaming so all subsystems share a consistent turn boundary. It ensures a clean 4xx response path by validating inputs before any streaming headers are emitted and by throwing domain/auth.exceptions early. The turn's selected model is committed up-front and propagated through the prompt loading, decision thresholds, and metrics, guaranteeing alignment across the rendering and evaluation steps. It also prepares a project-scoped tool context so tools like list/read project files operate within the correct project.

## Example
```csharp
// Most common usage: asynchronously consume the streaming events for a turn
await foreach (var evt in agentService.RunAsync(conversationId, userInput, ct))
{
    // Handle AgentEvent (e.g., render to UI, feed SSE, etc.)
}
```

## Notes
- Validation up-front ensures a clean 4xx ProblemDetails response before streaming headers are sent.
- The user message is persisted prior to streaming to keep the conversation timeline consistent even if the client disconnects mid-turn.
- A single model selection is determined and reused for thresholds, provider calls, and metrics to ensure consistent behavior throughout the turn.

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


Streams agent events by coordinating the conversation with the chosen chat provider, handling tool invocation, and sequencing context across iterations. It starts by compacting the current conversation to ensure the summarizer output is present in the history, then wraps the base provider in an Emulated tool bridge when needed. It then iterates up to MaxIterations, streaming from the provider and yielding AgentTextDelta and AgentReasoningDelta events as they arrive, while collecting ToolCallReadyEvent entries for potential tool invocations. If the provider hiccups and returns an empty turn (no text, no tool calls, and usually no reasoning), an internal short retry loop rebuilds the provider history and retries a bounded number of times with delays before surfacing an error. Each iteration rebuilds the AgentContext to incorporate the latest tool results and prior turns into the provider history, so subsequent streams see a complete, up-to-date conversation.

## Remarks
Acts as the streaming nucleus of the agent service, encapsulating the interplay between context creation, provider streaming, and tool integration. The Emulated mode path uses GabrielToolBridge to enrich prompts with tool docs and capture tool invocation details without changing the underlying provider. The per-iteration context rebuild guarantees that tool results and prior turns are reflected in the subsequent provider history, preserving coherence across the turn boundary.

## Notes
- Cancellation: The [EnumeratorCancellation] CancellationToken ct is observed by the streaming provider; canceling ct stops streaming promptly.
- Retry semantics: EmptyStopMaxRetries and EmptyStopRetryDelayMs implement resilience; repeated retries reuse the same history to recover from transient hiccups.

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


Streams a private async streaming helper that yields an initial AgentEvent to persist the user message and then streams the remainder of the conversation by delegating to RunStreamAsync. The first yielded event, AgentUserMessagePersisted, establishes the persistence side-effect prior to delivering subsequent events, ensuring a consistent and expected lifecycle for a user message as the stream unfolds. The method preserves the streaming contract of RunStreamAsync while guaranteeing that the user message is recorded before processing the rest of the stream, using the supplied userMessageId, conversation, optional variantGroupIdOverride, prompts, model selection, and cancellation token.

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


Walks back from the end keeping at least `keepLast` messages, then keeps walking until we land on a User message - that's our cut boundary. Doing this avoids ever cutting between an assistant's tool_calls and the matching tool results, which the model needs to see together.

This private static helper computes the boundary index into a sequence of messages. It begins from the position that would leave at least `keepLast` messages at the end and scans backward until it finds a User message. The returned index marks the cut point, ensuring that a user turn is preserved intact with any subsequent tool invocations and their results. If the history is too short or no User boundary is found, the method returns 0, effectively trimming from the start.


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


Serializes a collection of ChatProviderToolCall items into a JSON string that records each call as a uniform object. Each element includes the call's id, a type marker set to 'function', and a nested function descriptor containing the tool name and its arguments. The arguments are preserved as a JSON string (ArgumentsJson) so the caller can transport, store, or display the exact invocation data without re-parsing it here. This helper is used internally to serialize the sequence of tool invocations for auditing, replay, or UI presentation without requiring external callers to assemble the shape manually.

## Remarks
Internal helper centralizes serialization of tool-calls into a stable, consumable format. By fixing the outer shape and using 'type' = 'function', it enables downstream components to treat tool invocations uniformly, regardless of which specific tool was called. It also decouples the representation of arguments from the code that triggers the calls, since ArgumentsJson is already a JSON string.

## Notes
- ArgumentsJson is embedded as a string in the serialized payload; consumers should parse the outer JSON before inspecting function.arguments if they need to access the structured argument data.

---

## EmptyStopMaxRetries
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopMaxRetries = 2
```


EmptyStopMaxRetries bounds the number of additional retry attempts the agent will undertake for the "provider finished Stop with empty text" hiccup. Since the resilience pipeline cannot detect this as an error (the response is a 200 stream with empty content), the retry logic is implemented in the agent loop; two extra attempts are allowed, for a total of three tries, using a linear backoff where the wait on the N-th retry is N * EmptyStopRetryDelayMs.

## Remarks
This constant isolates a narrowly scoped retry policy for a rare edge-case, preventing it from influencing general error handling. It ensures transient blanks can be ridden out without delaying real failures, while keeping the overall retry behavior simple and predictable within the agent's operation.

## Notes
- Changing this constant requires recompilation and affects the retry cap for this edge-case across the class.
- It relies on EmptyStopRetryDelayMs to compute the actual wait time; if that value changes, the real backoff duration will scale accordingly, so keep them in sync for expected timing.

---

## EmptyStopRetryDelayMs
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopRetryDelayMs = 500
```


This private constant defines a fixed pause duration, in milliseconds, used by the AgentService when retrying stop-related logic. It provides a throttle to prevent busy-waiting in scenarios where a stop operation may require multiple attempts before completing.

## Remarks
This centralized delay keeps stop-retry pacing consistent and expresses the intent that retries should be spaced out rather than hammering resources. It also aids maintainability: changing a single value adjusts all stop-retry behavior within this class without scattering magic numbers.

## Example
```csharp
// Within a retry loop for stopping an agent
while (!StopConditionMet())
{
    Thread.Sleep(EmptyStopRetryDelayMs);
    AttemptStop();
}
```

## Notes
- Changing the constant affects all in-class stop-retry delays; tests that rely on timing may need adjustments.
- Because it is private, the exact value is an implementation detail; observe behavior (not the literal delay) when validating correctness.

---

## LogPreviewLimit
> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int LogPreviewLimit = 240
```


Defines the maximum number of characters included in log messages that preview tool arguments and results. It keeps log output concise by truncating large payloads—such as a fetched web page or a long file read—before writing to logs.

## Remarks
This constant enforces a single, consistent limit on how much of a payload is shown in logs, reducing noise while retaining enough context for troubleshooting. Being a private, compile-time constant, it is an internal implementation detail and not configurable at runtime. If you require more verbose previews, you must change the constant and rebuild. The preview reflects only the logged portion of the payload and does not alter the actual data processed by the system.

## Example
```csharp
// Example using the shared log preview limit
string preview = payload.Length > LogPreviewLimit
    ? payload.Substring(0, LogPreviewLimit) + "…"
    : payload;
logger.Log("Payload preview: " + preview);
```

## Notes
- Truncation is by character count, so it may cut through a surrogate pair or combining character in rare Unicode sequences; use a Unicode-safe truncation approach if your payloads include such content.
- The value is a compile-time constant and cannot be changed at runtime; to adjust it, modify the code and recompile.
- Avoid logging sensitive data even within the preview; redact or exclude secrets as a best practice.

---