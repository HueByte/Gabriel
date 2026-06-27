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

Orchestrates a single agent turn: validates input, persists the user's message, selects the model for the turn, builds the system and per-turn prompts (persona/project/memory/tools), populates the scoped tool execution context, and then drives the provider call while streaming responses back as AgentEvent values. Use this service when you want a high-level, conversation-aware run loop that handles persistence, prompt construction, tool wiring, compaction decisions, post-processing and state updates — i.e., whenever you need a streamed agent reply rather than calling a provider directly.

## Remarks
AgentService centralizes orchestration responsibilities so callers receive a consistent, turn-scoped stream of AgentEvent items. It resolves the model once per turn and passes that selection through compaction, provider invocation and metrics to ensure consistent behavior. Tool execution is project-scoped via the populated IToolExecutionContext and external tool access (e.g., Gabriel integration) is created on demand; the bridge objects are instantiated per-call (AgentService retains the logger for them rather than exposing a factory).

A small, explicit retry loop handles a transient "provider finished Stop with empty text" hiccup that cannot be detected by HTTP resilience layers because the response is an otherwise-successful 200 stream. The retry is bounded (two retries in addition to the original attempt) with a linear backoff so transient blank stops are retried without significantly delaying real failures.

## Example
```csharp
// Stream agent events for a conversation turn and print each event as it arrives
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await foreach (var agentEvent in agentService.RunAsync(conversationId, "What is the status?", cts.Token))
{
    Console.WriteLine(agentEvent); // handle partial tokens, tool invocation requests, final reply, etc.
}
```

## Notes
- Persisting the user message and validating up-front is intentional: failures are surfaced before SSE headers are sent so the global exception handler can return 4xx ProblemDetails instead of starting a partially-formed stream.
- RunAsync returns `IAsyncEnumerable<AgentEvent>`; callers must enumerate (e.g., await foreach) to drive the turn. Provide a CancellationToken to stop the stream promptly.
- Empty-stop retry behavior: AgentService performs up to two additional attempts (three total attempts) with a linear backoff of N * 500ms per retry attempt index.

---

## TurnPrompts

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** record

Immutable container that bundles the per-turn prompt inputs used to construct an AgentContext. This private record groups the required persona prompt, optional project prompt and memory block, and the list of available tools so AgentContext.Build can assemble them in a single place instead of callers reordering or manipulating pieces directly.

## Remarks
This record exists to make assembly of the per-turn inputs explicit and atomic: callers create or pass a TurnPrompts instance and AgentContext.Build is responsible for composing those fields into the runtime context. Using a sealed record gives simple, immutable value semantics and prevents external code from depending on or reordering the individual pieces; any validation or transformation belongs in the AgentContext build process rather than here.

## Notes
- ProjectPrompt and MemoryBlock are nullable — callers should expect missing values and let AgentContext.Build handle defaults.
- Tools is an IReadOnlyList of ToolDescriptor and represents the available tools for the turn; it should be treated as immutable by consumers.
- This type is private to the service layer; do not rely on it from external code as its shape may change with AgentContext construction logic.

---

## AgentService

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** constructor

Constructs an AgentService by accepting and storing all of the service's required collaborators (repositories, provider/registry/catalog components, execution/context objects, utilities like token estimation and prompt building, options and loggers). A developer typically does not call this constructor directly in application code — it is intended to be resolved by the dependency injection container so all dependencies are provided automatically.

## Remarks
This constructor wires the many collaborators that AgentService uses to coordinate conversation handling, model/provider selection, memory access, tool execution, prompt construction, response post-processing and persistence. The long dependency list reflects the service's orchestration role: AgentService itself delegates specialized work to the injected repositories, registries, builders and utilities rather than implementing those concerns inline.

## Example
```csharp
// Typical registration so the DI container will call the constructor for you
services.AddScoped<AgentService>();

// Manual construction (useful in tests) — note Options.Create to provide IOptions<AgentOptions>
var agent = new AgentService(
    conversationsMock.Object,
    projectsMock.Object,
    providerRegistryMock.Object,
    modelCatalogMock.Object,
    userPrefsMock.Object,
    memoriesMock.Object,
    toolsMock.Object,
    toolContextMock.Object,
    uowMock.Object,
    tokensMock.Object,
    currentUserMock.Object,
    stateUpdaterMock.Object,
    promptBuilderMock.Object,
    postProcessorMock.Object,
    Options.Create(new AgentOptions { /* configure as needed */ }),
    loggerMock.Object,
    toolBridgeLoggerMock.Object);
```

## Notes
- The constructor accesses options.Value directly; passing a null `IOptions<AgentOptions>` (or an IOptions instance whose Value is null) will cause a NullReferenceException — prefer resolving via DI or use Options.Create in tests.
- Many dependencies increase test/setup complexity; prefer using the DI container or lightweight test doubles/helpers rather than constructing manually in production code.
- Be careful with service lifetimes: avoid injecting scoped services into singletons (or otherwise mismatched lifetimes) when registering AgentService in the DI container.

---

## AppendMemory

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Appends a single memory entry to the provided StringBuilder using a small Markdown-like layout. Use this helper when building a textual representation of memory entries (for example, when composing an agent prompt) so all entries share a consistent header, description, and body formatting.

## Remarks
This private helper centralizes the text layout used for memory entries: it writes a third-level header that contains the memory type (lowercased inside square brackets) and the entry name, then emits the description, a blank line, the body, and a trailing blank line. Keeping formatting here ensures all callers produce identical, markdown-friendly output suitable for inclusion in prompts or logs.

## Example
```csharp
var sb = new StringBuilder();
var memory = new Core.Entities.MemoryEntry
{
    Type = Core.Entities.MemoryType.Fact, // example enum
    Name = "API Limits",
    Description = "Limits and quotas for the public API.",
    Body = "The API allows 1000 requests per hour."
};
AppendMemory(sb, memory);

// sb.ToString() now contains something like:
// ### [fact] API Limits
// Limits and quotas for the public API.
//
// The API allows 1000 requests per hour.
//
```

## Notes
- The method does not validate arguments: sb and m must not be null (otherwise a NullReferenceException occurs).
- Field values are written as-is; the memory Type is converted via ToString() and lowercased with ToLowerInvariant(), so the output depends on the enum names and is not localized.
- The helper deliberately emits trailing blank lines between sections; callers should account for that when concatenating multiple entries.

---

## ExecuteToolSafelyAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Runs a tool call from the service's tool registry with centralized logging, timing and error handling. Validates the tool exists, invokes its ExecuteAsync method with the provided cancellation token, records elapsed time, treats returned strings that start with "Error" (case-insensitive) as soft errors (logged at Warning), and converts thrown exceptions into a standardized error string.

## Remarks
This method centralizes the common concerns around invoking external or plugin tools: existence checks, structured logging at meaningful levels (start/info, soft-error/warning, exception/error), elapsed-time measurement, and normalizing the return value so callers always receive a string. It intentionally treats a tool-returned string beginning with "Error" as a semantic failure (soft error) rather than an exception to give tool implementations a simple error signaling mechanism without throwing. Exceptions from the tool are caught, logged with the exception details, and converted into a user-facing "Error executing <tool>: <message>" string.

## Notes
- Soft-error detection uses observation?.StartsWith("Error", StringComparison.OrdinalIgnoreCase). Outputs that legitimately begin with the word "Error" will be classified as soft errors even if they are not failures.
- The method logs arguments and results via Preview(...); avoid passing sensitive secrets in tool arguments or results because they may be written to logs.
- If a tool returns null, the method returns an empty string; callers expecting null should account for this normalization.

---

## GenerateSummaryAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Generates a concise, factual summary of the provided conversation turns by sending a system prompt and the assembled conversation text to the configured chat provider and reading the provider's streamed text deltas. When a previous summary is supplied it is included as "Existing summary of even earlier turns:" and new turns are presented to the provider so the returned summary folds recent conversation into the earlier summary. Use this when you need an up-to-date, incremental summary produced by the external chat provider rather than constructing summaries locally.

## Remarks
This method centralizes the logic for preparing the summarization prompt and consuming a streaming response from a chat provider. It formats message entries with role markers, substitutes placeholders for missing content or tool calls, and relies on the provider's StreamAsync interface to deliver incremental text (TextDeltaEvent) and a terminating FinishEvent. The result is trimmed before being returned.

## Example
```csharp
// Prepare messages to summarize
var messages = new List<Message>
{
    new Message { Role = "user", Content = "We need to decide on the release date." },
    new Message { Role = "assistant", Content = "Suggested targets: next Tuesday or the following Friday." }
};

// ModelSelection selection = ...; CancellationToken ct = ...; string? previous = null;
string summary = await GenerateSummaryAsync(previous, messages, selection, ct);
Console.WriteLine(summary);
```

## Notes
- The method resolves a provider from _providerRegistry; if the provider key in ModelSelection is invalid this will fail.
- It depends on the provider emitting a FinishEvent to stop streaming; if a provider never sends FinishEvent the call may hang until cancelled.
- Messages with null Content are represented as "(requested tools)" when ToolCallsJson is present or "(no content)" otherwise — callers should ensure important content is present on Message.Content if they expect it to appear verbatim.

---

## GetContextMetricsAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns detailed token and context metrics for a given conversation as seen by the currently authenticated user. Call this when you need to know the current token usage, the context window size, the compacting threshold (both ratio and absolute tokens), how many messages remain after any trimming, and a breakdown of where tokens are spent (system, project, memory, tools, conversation, summary).

## Remarks
This method enforces that an authenticated user is present and that the user has access to the requested conversation; it will throw UnauthorizedAccessException or NotFoundException otherwise. It resolves the active model selection (which provides the context window and optional compact threshold), loads the assembled prompts for the next turn, builds an AgentContext and asks that context to compute a token breakdown via the tokenizer (_tokens). The computation intentionally matches the backend's compacting logic (see MaybeCompactAsync) so that UI indicators (the "trigger line") align exactly with when the backend would decide to compact.

## Example
```csharp
// In a controller or service where agentService is available
try
{
    var metrics = await agentService.GetContextMetricsAsync(conversationId, cancellationToken);
    // use metrics to render UI, display token counts, or decide whether to compact
}
catch (UnauthorizedAccessException)
{
    // handle missing authentication
}
catch (NotFoundException)
{
    // handle conversation not found or inaccessible
}
```

## Notes
- The compact threshold in tokens is computed as (int)(window * ratio); this truncates toward zero and can differ from a rounded value.
- If the context window is zero, CompactThresholdTokens is set to 0.
- MessagesAfterCut reflects the number of messages in the AgentContext after any trimming/cutting logic; IsSummarized is true when the conversation has a SummarizedThroughMessageId.
- Token counts depend on the tokenizer implementation used by _tokens; changes to tokenization will change all reported values.

---

## LoadMemoryBlockAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Formats saved "memories" into a single system message string that can be injected into the model’s context. Call this when preparing conversation context for a specific project (pass a projectId) or for the user in general (pass null); the method returns null when there are no memories to inject.

## Remarks
This method groups entries into two sections — user-scope first and project-scope second — so the model can distinguish facts that apply globally to the user from those that apply only within the current project. It fetches the memories via the underlying repository call and delegates per-entry formatting to AppendMemory. The resulting string is trimmed of trailing whitespace before being returned.

## Example
```csharp
// Example output string produced by LoadMemoryBlockAsync
@"[Saved memories]
Durable facts the user has asked Gabriel to remember. Apply these unless the user contradicts them in the current conversation. Each entry has a scope (user = applies everywhere; project = only in this project), a type (user/feedback/project/reference), and a body.

## User-scope memories
- (user) preference: Prefers email notifications

## Project-scope memories
- (project) reference: Use API endpoint https://api.example.com/v1 for this project"
```

## Notes
- The method returns null (not an empty string) when there are no memories; callers should check for null before injecting into the prompt.
- Memories are grouped but not otherwise sorted or deduplicated here — ordering comes from the repository result and per-entry formatting is handled by AppendMemory.
- It's asynchronous and accepts a CancellationToken; the underlying ListForConversationAsync call may perform I/O and can be cancelled.

---

## LoadProjectSystemPromptAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns a project-scoped system prompt fragment for the given conversation when the conversation is associated with a non-default project. Use this method when assembling the final system prompt sent to an agent so the assistant is aware of project context and the preferred memory_save scope; it returns null for the default project or when the conversation has no ProjectId.

## Remarks
This method centralizes the generation of project-level context that should be injected into an agent's system prompt. It enforces two behaviors: (1) avoid adding context for the default or missing project (returns null), and (2) prepend guidance about memory_save scope so downstream code and the assistant default to project-scoped memory for project-specific facts. It fetches the project through the projects repository using the caller's userId to ensure the returned prompt reflects the caller's authorized view of the project.

## Example
```csharp
// Assemble the system prompt for an agent run, including project context when present
var projectSystemPrompt = await LoadProjectSystemPromptAsync(conversation, userId, cancellationToken);
var systemPromptBuilder = new StringBuilder();
if (projectSystemPrompt != null)
{
    systemPromptBuilder.AppendLine(projectSystemPrompt);
}
systemPromptBuilder.AppendLine("You are a helpful assistant...");
var finalSystemPrompt = systemPromptBuilder.ToString();
```

## Notes
- Returns null when conversation.ProjectId is null, when the referenced project is missing, or when the project is the default project.
- The project.SystemPrompt is appended only if it contains non-whitespace content.
- The method is asynchronous and accepts a CancellationToken; any exceptions from the project repository (GetByIdAsync) will propagate to the caller.

---

## LoadTurnPromptsAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Creates a consistent, turn-scoped set of prompt pieces used to drive model behavior: the persona prompt (derived from the conversation state), an optional project-level system prompt override, the saved memory block for the conversation, and the list of tool descriptors appropriate for the chosen model selection. Call this once at the start of a turn so compaction, streaming, and telemetry all see the same snapshot of prompts and available tools.

## Remarks
This method consolidates the four prompt components that must remain stable for the duration of a single turn. By loading the project system prompt and memory block from storage and building the persona from the conversation state, it produces a single TurnPrompts instance that is passed through compaction and streaming paths. The selection.ToolMode controls the tool descriptor list: ToolMode.None returns an empty list (so providers and telemetry don't see any tool capabilities), while other modes return the full descriptor list and rely on the provider boundary to handle transport differences.

## Example
```csharp
// Typical use at the start of handling a user turn
var selection = new ModelSelection { ToolMode = ToolMode.Native /* or ToolMode.Emulated or ToolMode.None */ };
var ct = CancellationToken.None;
TurnPrompts prompts = await LoadTurnPromptsAsync(conversation, userId, selection, ct);
// pass `prompts` into compaction and streaming logic so every component uses the same snapshot
```

## Notes
- The method performs asynchronous operations (loading project prompt and memory block) and accepts a CancellationToken — callers should propagate cancellation.
- The persona is built from conversation.GetState(); because ConversationState updates only on user input, the persona reflects the last committed state at the time of the call.
- If selection.ToolMode == ToolMode.None, the returned tools list is empty; this prevents advertising tool capabilities to providers and keeps the "Tools" telemetry bucket at zero for the turn.
- Loading memory or project prompts may be I/O bound and potentially expensive; cache or reuse TurnPrompts for the single turn rather than calling repeatedly.


---

## MaybeCompactAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Decides whether the current conversation should be compacted (rolled up into a summary) before the next model turn and, when appropriate, performs that compaction. This method evaluates the full AgentContext (persona, project prompt, memory block, summary, tools and conversation) against the selected model's context window and compact threshold; if the estimated token usage exceeds the computed threshold it produces a short summary of older messages, updates the conversation summary, persists the change, and yields events that describe the compacting lifecycle so callers (UI or orchestrators) can react.

## Remarks
This is an asynchronous generator that yields AgentEvent values to signal compacting progress: it emits AgentCompactStart when the summary request begins, and AgentCompactDone when the operation finishes (successfully or not). The threshold used is selection.CompactThreshold when set, otherwise the global AgentOptions.CompactThreshold; the method uses the full AgentContext token breakdown so large persona/project/memory prompts count toward triggering compaction. It also avoids re-summarizing messages already included in a previous summary by checking conv.SummarizedThroughMessageId.

## Example
```csharp
// Consume the compacting events and react (UI update, logging, etc.).
await foreach (var ev in MaybeCompactAsync(conv, prompts, selection, ct))
{
    switch (ev)
    {
        case AgentCompactStart start:
            // show compaction UI overlay, e.g. spinner with start.MessageCount
            break;
        case AgentCompactDone done:
            // hide overlay, update token accounting with done.SummaryTokens
            break;
        default:
            // handle other AgentEvent types if added later
            break;
    }
}
```

## Notes
- The method is a generator: callers must enumerate the returned IAsyncEnumerable to execute compaction and receive events.
- If the summary call fails or returns an empty string, the method logs a warning and emits AgentCompactDone with zero summary tokens so the caller can clear any UI state; the conversation is not modified in that case.
- The method updates conv.Summary and persists changes via the unit-of-work; callers should expect the conversation to be mutated and saved when compaction succeeds.
- CompactThreshold is applied as (int)(ContextWindowTokens * ratio); a selection with ContextWindowTokens <= 0 causes an immediate no-op.


---

## Preview

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns a concise, single-line preview of the provided text suitable for logs or brief displays. Null or empty input becomes the literal "(empty)"; any carriage returns/newlines are replaced with spaces and the result is truncated to the configured LogPreviewLimit with a trailing ellipsis when necessary.

## Remarks
This helper exists to keep potentially large or multi-line tool outputs compact and single-line so they don't flood log files or UI list views. It collapses line breaks to spaces rather than removing them entirely to preserve word separation, and enforces a fixed maximum width (LogPreviewLimit) so callers can safely include previews without additional length checks. The method does not sanitize or redact content — treat it as a purely presentation-oriented helper.

## Example
```csharp
// Typical usage when writing a log entry
var p1 = Preview(null);                 // "(empty)"
var p2 = Preview("First line\nSecond line"); // "First line Second line"
var longText = new string('x', 200);
var p3 = Preview(longText);             // first LogPreviewLimit chars of longText followed by "…"
```

## Notes
- The method treats a whitespace-only string as non-empty (only null or empty string returns "(empty)").
- Newline characters ('\n' and '\r') are replaced with spaces; other whitespace (tabs, etc.) is preserved.
- Truncation can cut mid-word; the appended character is the Unicode ellipsis ("…").

---

## RegenerateAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Regenerates an existing assistant response within a conversation and returns a streamed sequence of AgentEvent produced by the regeneration run. Reach for this when you need to re-generate an assistant message (for example, to produce an alternative reply) while preserving the conversation state and allowing the UI to correlate new alternatives with the original variant group.

## Remarks
This method enforces that the caller is an authenticated user and that the target message exists and is an active assistant variant. It deactivates the target message's variant group (so the old reply and any sibling tool messages are marked inactive), saves that change, and then runs the generation pipeline using the same variant group id so the UI can present the regenerated alternatives alongside the original group. No new user message is created — the regeneration is performed against the same conversation state that existed when the original turn was produced.

## Notes
- Requires an authenticated user; throws UnauthorizedAccessException if no current user is available.
- Throws NotFoundException if the conversation or the specified message cannot be found for the user.
- Throws DomainException if the specified message is not an assistant message or is not the active variant.
- The method persists the deactivation of the variant group (calls SaveChanges) before starting the generation stream.
- Returns an `IAsyncEnumerable<AgentEvent>`; the generation runs asynchronously and yields events as they are produced. CancellationToken is accepted and forwarded to async operations.
- Because the method re-uses the original state, callers should not expect a new user turn to be added as part of regeneration.

---

## ResolveModelSelectionAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Resolves and returns the ModelSelection that should be used for the current operation by reading the latest user preferences and consulting the model catalog. Call this when you need the effective provider/model pair for the current turn so changes to a user's preferred provider or model take effect immediately.

## Remarks
This private helper centralises the logic for mapping a user's persisted preferences to a concrete ModelSelection. It first fetches the current user preferences from _userPrefs (so changes made in settings are honoured on the next call), then delegates to _modelCatalog.Resolve to obtain the concrete selection for the specified provider and model name. Keeping this logic in one place ensures consistent behaviour across entry points in the service.

## Notes
- The CancellationToken is forwarded to the preferences retrieval; cancelling the token will abort the async fetch.
- Exceptions from _userPrefs.GetAsync or _modelCatalog.Resolve are not caught here and will propagate to the caller; callers should handle or surface errors as appropriate.
- This method is lightweight and expected to be called per-turn (per operation) so that preference changes are observed immediately.

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

**Returns:** `Task<`IAsyncEnumerable<AgentEvent>`>`


Starts a single agent turn for the given conversation and streams back the sequence of AgentEvent items that represent the turn's lifecycle (compaction events, tool calls, partial model outputs, final reply, etc.). Use this when you need to persist the user's message, update conversation state, and return a server-sent-events (SSE) style stream of events for a single request/turn instead of performing a synchronous reply.

## Remarks
RunAsync performs several orchestration steps up-front so the streamed events can assume a consistent turn context: it validates input (throwing early so upstream exception handling can produce 4xx ProblemDetails), ensures an authenticated user, loads the conversation (or throws NotFound), appends and persists the user message, updates conversation state, resolves a single model selection for the entire turn, loads turn-wide prompt pieces (persona/project/memory/tools), and populates a scoped tool-execution context. The method then hands control to RunStreamWithUserPreambleAsync which yields events (including an initial AgentUserMessagePersisted event containing the real DB id) and runs compaction and the streaming loop. Committing the message and state before streaming keeps the conversation timeline consistent even if the client disconnects mid-stream.

## Example
```csharp
// Typical consumer in an async handler that wants to stream events to a client
var events = await agentService.RunAsync(conversationId, userInput, cancellationToken);
await foreach (var ev in events)
{
    // send each AgentEvent to the client (SSE / WebSocket / etc.)
    SendEventToClient(ev);
}
```

## Notes
- Validation and authentication failures are raised synchronously before streaming begins: DomainException for empty input, UnauthorizedAccessException if no authenticated user, and NotFoundException if the conversation doesn't exist.
- The user's message is persisted and SaveChangesAsync is called before the stream starts. The stream's first event allows the client to reconcile any temporary client-side message id with the real database id.
- A single model selection is resolved and used for the whole turn; compaction and provider calls use that same selection to keep behavior and metrics consistent.
- CancellationToken is accepted and should be honoured by callers; the stream may still emit partial events if the client disconnects mid-turn.

---

## RunStreamAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Streams the execution of a single agent turn as an async sequence of AgentEvent values. Use this when you need to drive a conversation turn incrementally (for UI updates, progressive logging, or interleaving tool calls/results) instead of waiting for a single completed response.

## Remarks
The method first runs a compaction/summarization pass so the conversation's Summary is available before the actual turn context is built. It resolves the configured chat provider and — when ToolMode.Emulated is selected — wraps it with GabrielToolBridge so tool documentation and <tool_call> markers are handled in the text stream. The core loop performs up to a configured maximum number of iterations; each iteration rebuilds the provider history (so prior assistant tool calls and tool results are included) and streams provider events. Text and reasoning are emitted as incremental delta events, tool calls are detected and buffered, and a finish reason is observed to decide iteration flow. There is a small retry loop that detects empty successful streams (a provider that returns Stop with no content) and retries the provider call a bounded number of times with delays before surfacing the situation.

## Example
```csharp
await foreach (var evt in RunStreamAsync(conversation, variantGroupId, prompts, selection, cancellationToken))
{
    switch (evt)
    {
        case AgentTextDelta t:
            // append to displayed assistant text
            Console.Write(t.Delta);
            break;

        case AgentReasoningDelta r:
            // render or log intermediate reasoning
            Console.Write(r.Delta);
            break;

        case AgentToolCall call:
            // invoke a tool and later feed results back into the conversation
            await InvokeToolAsync(call);
            break;

        case AgentToolResult result:
            // display or store tool output
            Console.WriteLine(result.Output);
            break;
    }
}
```

## Notes
- The CancellationToken is applied to the async enumerator (EnumeratorCancellation) — consumers should cancel the enumeration to stop the stream promptly.
- The method can emit partial text and reasoning deltas; consumers should treat these events as incremental updates rather than final content.
- The implementation contains a bounded retry path for empty successful responses from providers (i.e., Stop with no content); callers may see a small delay while retries occur.


---

## RunStreamWithUserPreambleAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Emits an AgentUserMessagePersisted event for the given userMessageId and then yields all events produced by RunStreamAsync for the same conversation and options. Use this wrapper when callers must observe a persisted-user-message event before receiving the subsequent run events.

## Remarks
This is a small orchestration wrapper: it guarantees a single preamble event (AgentUserMessagePersisted) is produced for the provided userMessageId and then delegates to RunStreamAsync to produce the remainder of the event stream. Ordering is preserved — consumers will always see the persisted-user-message event before any events from the inner stream. Cancellation is forwarded to the inner stream via the provided CancellationToken.

## Notes
- The first yielded event is an AgentUserMessagePersisted constructed with the userMessageId parameter.
- Cancellation is honored via the [EnumeratorCancellation] token; cancelling the enumerator will propagate to the inner RunStreamAsync enumeration.
- This method performs no argument validation (it is a private helper) and will propagate exceptions thrown by RunStreamAsync to the caller.

---

## SelectCompactCutIndex

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns an index suitable for trimming a conversation so that at least `keepLast` messages are retained and the cut point falls on the most-recent User message at or before the retention boundary. Reach for this when compacting message history before sending context to the model to avoid splitting an assistant's tool call from its corresponding tool results.

## Remarks
This method walks backward from messages.Count - keepLast to find the nearest preceding message with Role == MessageRole.User and returns its index. The goal is to ensure the cut boundary aligns with a user turn so assistant messages (particularly tool_call and tool result pairs) are not separated across the cut. It is a simple, linear backward scan and is intended to be used as part of history-trimming logic in the agent pipeline.

## Example
```csharp
// Given a message stream with roles, keep the last 3 entries but cut at the
// most recent User message that is no later than that boundary.
var messages = new List<Message> {
    new Message { Role = MessageRole.System },
    new Message { Role = MessageRole.User },     // index 1
    new Message { Role = MessageRole.Assistant },
    new Message { Role = MessageRole.User },     // index 3
    new Message { Role = MessageRole.Assistant },
    new Message { Role = MessageRole.Assistant } // index 5
};
int cutIndex = SelectCompactCutIndex(messages, keepLast: 3);
// cutIndex will point to the most-recent User message at or before messages.Count - 3
```

## Notes
- If there is no User message at or before the computed boundary the method returns 0; that means the returned index may not actually be a User message in that edge case.
- If keepLast >= messages.Count the method returns 0 (no trimming).
- Complexity is O(n) in the number of messages scanned (backwards);
  callers should avoid calling it in tight loops on very large histories without considering cost.


---

## SerializeToolCalls

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Serializes a sequence of ChatProviderToolCall objects into a JSON array shaped for function-style tool calls. Use this when you need a compact, provider-friendly representation (each item contains id, a fixed type of "function", and a function object with name and the raw ArgumentsJson value) for transmission to or logging of a chat/agent system.

## Remarks
This method projects each ChatProviderToolCall into an anonymous object with the properties expected by many function-calling chat providers: id, type (always "function"), and a nested function object with name and arguments. It uses System.Text.Json.JsonSerializer.Serialize with default options to produce the final JSON string and is intentionally minimal: there are no custom serialization options, validation, or error handling in this helper.

## Example
```csharp
// Example usage (ChatProviderToolCall is assumed to have Id, Name and ArgumentsJson properties):
var calls = new List<ChatProviderToolCall>
{
    new ChatProviderToolCall { Id = "1", Name = "getWeather", ArgumentsJson = "{\"city\":\"Seattle\"}" },
};

string json = SerializeToolCalls(calls);
// json will be a JSON array; note that ArgumentsJson is serialized as a string value
// (quotes inside ArgumentsJson are escaped by the serializer).
```

## Notes
- ArgumentsJson is treated as a string property: if it already contains JSON text, the serializer will escape it, embedding it as a JSON string rather than an inline JSON object. If you need the arguments to be serialized as JSON objects, parse ArgumentsJson into a JsonDocument/JsonElement (or a typed object) before serializing.
- The method does not validate inputs; passing null for calls will throw a NullReferenceException. Ensure the caller provides a non-null list.
- Serialization uses default System.Text.Json options (naming policy, converters, etc.). If custom formatting or converters are required, add explicit JsonSerializerOptions.

---

## EmptyStopMaxRetries

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

Defines the number of additional retry attempts the agent loop will perform when a provider finishes a Stop operation but returns an empty response. Adjust this constant to tune how aggressively the agent retries transient "empty stop" hiccups (this is the count of extra retries beyond the initial attempt).

## Remarks
This constant exists because the HTTP resilience pipeline cannot treat a successful 200 stream that contains an empty body as an error, so retries for this specific hiccup are implemented in the agent loop rather than in the HTTP layer. The value 2 represents two extra attempts (three total attempts including the initial one) which empirically balances recovering from transient blanks while avoiding excessive delay for genuine failures. Retries use a linear backoff that multiplies the attempt index by EmptyStopRetryDelayMs.

## Notes
- It is the number of additional retries; total attempts = 1 (initial) + EmptyStopMaxRetries.
- Backoff behaviour: retry attempt N waits N * EmptyStopRetryDelayMs (N starts at 1 for the first retry).
- This is a compile-time constant — changing it requires a rebuild and will affect overall latency for Stop operations when retries occur.

---

## EmptyStopRetryDelayMs

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

Milliseconds to wait between retry attempts when a stop operation finds nothing to process. This constant serves as an internal tuning parameter to avoid a tight busy-loop while the service retries an empty stop request.

## Remarks
This private constant provides a simple fixed backoff used by the agent stop/retry logic to reduce CPU usage and excessive log noise when stop attempts return immediately with no work. Choosing a moderate default (500 ms) balances responsiveness to newly-arriving stop conditions with minimizing polling overhead; it is intentionally small so retries remain reasonably prompt.

## Notes
- Value is in milliseconds; changing it requires recompilation because it is a compile-time constant.
- Lowering the value makes retries more responsive but can increase CPU and I/O churn; raising it reduces churn but delays retries.

---

## LogPreviewLimit

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

Maximum number of characters to include when the agent writes tool arguments or results into a log message. This limit is applied to preview large payloads (for example: fetched web pages or large file contents) so logs remain useful and do not become excessively large.

## Remarks
This constant centralises the truncation length used for tool input/output logging to ensure a consistent preview size and to avoid unbounded log growth. It is declared as a compile-time constant for predictable, low-overhead behavior; if you need to make this value configurable at runtime (for example, via configuration or feature flags), replace it with a configuration-backed value.

## Notes
- The limit is measured in .NET characters (string.Length, i.e. UTF-16 code units), not bytes or user-perceived grapheme clusters — surrogate pairs and combining characters can affect visual length.
- Because the field is private and const, changing it requires a code change and recompilation.
- If different components require different preview sizes or runtime tuning, consider introducing a configuration setting rather than a hard constant.

---