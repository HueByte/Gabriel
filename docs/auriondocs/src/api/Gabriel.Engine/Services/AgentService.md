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

Coordinates and runs a single conversational agent turn and streams its progress as AgentEvent values. Use this service when you need to validate and persist an incoming user message, resolve the model and turn-specific prompts, populate tool execution context, invoke the configured chat provider, and emit the streaming response events back to the caller (for example over SSE or a websocket).

## Remarks
AgentService is an orchestration layer that pulls together repositories, model selection, system-prompt construction, memory, tools, token estimation, provider lookup, and response post-processing. It commits to a single model selection per turn, persists the incoming user message (so the conversation timeline remains consistent even if the client disconnects), and performs upfront validation so failures surface as clean HTTP 4xx before streaming begins. The class also implements a small, bounded retry for a known provider hiccup where a provider returns a successful 200 stream that contains an empty stop; this retry lives inside the agent loop rather than the HTTP resilience pipeline.

## Example
```csharp
// Typical usage: begin an agent turn and stream events as they arrive.
var service = /* resolve IAgentService implementation */;
Guid conversationId = /* conversation id */;
CancellationToken ct = CancellationToken.None;

await foreach (var ev in service.RunAsync(conversationId, "Hello, can you summarize my project?", ct))
{
    switch (ev.Type)
    {
        case AgentEventType.Chunk:
            Console.Write(ev.Text);
            break;
        case AgentEventType.Finished:
            Console.WriteLine("\n--- Done ---");
            break;
        case AgentEventType.Error:
            Console.WriteLine($"Error: {ev.Error}");
            break;
    }
}
```

## Notes
- The service uses a bounded retry for the "empty stop" provider issue: it will attempt up to 2 retries (3 attempts total) with a linear backoff of EmptyStopRetryDelayMs per attempt.
- Validation and persistence of the incoming user message happen before the stream starts so a caller receives deterministic error responses and the conversation timeline remains correct if the client disconnects mid-stream.
- CancellationToken should be supplied by the caller to stop streaming; long-running streams rely on cooperative cancellation from the token.

---

## TurnPrompts

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** record

Container that groups the per-turn pieces used to build an AgentContext: the persona prompt, optional project prompt and memory block, plus the available tool descriptors. Use this internal record when assembling the final prompt state for a single turn; callers outside AgentContext.Build should not reorder or mutate these pieces.

## Remarks
This sealed record exists to keep prompt components grouped and immutable during construction of an AgentContext. AgentContext.Build is responsible for assembling these fields into the final prompt sequence, which prevents callers from accidentally changing ordering or mixing lifecycle concerns. ProjectPrompt and MemoryBlock are nullable because they may be absent for some turns, and Tools is exposed as an IReadOnlyList to discourage in-place modification.

## Example
```csharp
// Example usage inside the same assembly (AgentContext.Build or related factory):
var tools = new List<ToolDescriptor> { toolA, toolB };
var turn = new TurnPrompts(
    PersonaPrompt: "You are a helpful assistant.",
    ProjectPrompt: "Project: Roadmap v2",
    MemoryBlock: null,
    Tools: tools);
```

## Notes
- The record is private and sealed; it's an implementation detail of AgentService/AgentContext and not intended for public consumption.
- Tools is an IReadOnlyList to signal immutability, but the underlying collection may still be mutable; avoid mutating the list after construction.
- Equality for the record compares the Tools reference (the list object) rather than performing a sequence-equality of its elements, so two TurnPrompts with identical tool contents but different list instances will not be equal.

---

## AgentService

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** constructor

Initializes an AgentService by storing all required collaborators supplied through dependency injection. Reach for this constructor only when wiring the service into a DI container or creating the type in tests; it simply assigns the provided repositories, registries, services, builders, post-processors, options and loggers to the instance for later orchestration.

## Remarks
This constructor is an aggregator-style entry point that collects all dependencies the AgentService needs to coordinate conversations, model selection, memory, tools, token estimation, user/context information, and response post-processing. It exists to keep the AgentService focused on orchestration logic while delegating specific responsibilities (persistence, tooling, prompting, etc.) to dedicated collaborators injected here.

## Notes
- The constructor does not perform defensive null-checks; callers (typically the DI container) must provide non-null services. Passing null values will result in NullReferenceExceptions when those fields are used.
- The AgentService captures many collaborators — ensure their DI lifetimes are compatible (e.g., avoid registering AgentService as a singleton when injected services are scoped), otherwise you may capture a shorter-lived service in a longer-lived instance and cause runtime errors or incorrect behavior.
- The options parameter is unwrapped via options.Value; ensure `IOptions<AgentOptions>` is configured in DI before resolving this constructor.

---

## AppendMemory

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Appends a Core.Entities.MemoryEntry to a StringBuilder using a simple Markdown-like layout. Use this helper when assembling combined text (for prompts, logs or reports) where each memory entry should appear as a headed block containing its type, name, description and body.

## Remarks
This private static helper centralizes the textual formatting for memory entries so callers don't repeat the same layout logic. It formats the entry as a level-3 Markdown heading that includes the memory type (lowercased) in brackets followed by the entry name, then writes the description and body separated by blank lines.

## Example
```csharp
var sb = new StringBuilder();
var memory = new Core.Entities.MemoryEntry
{
    Type = MemoryType.Fact, // enum or similar
    Name = "ProjectSummary",
    Description = "Summary of the project goals.",
    Body = "The project aims to..."
};
AppendMemory(sb, memory);
// sb now contains:
// ### [fact] ProjectSummary
// Summary of the project goals.
//
// The project aims to...
```

## Notes
- Neither sb nor the memory entry (or its properties) are null-checked; passing null will throw a NullReferenceException. Validate callers or add checks if needed.
- The method does not escape or sanitize Markdown; embedded Markdown in Name, Description or Body will be preserved and may affect rendering.
- The memory Type is converted with ToString().ToLowerInvariant(), so the textual representation depends on the Type's ToString implementation (commonly an enum name).

---

## ExecuteToolSafelyAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Wraps a ChatProviderToolCall execution with discovery, timing, structured logging, and error handling. This helper locates the named tool, runs it with the provided arguments and cancellation token, records elapsed time, and converts failures into structured log entries and a string result so callers always receive a non-null textual outcome.

## Remarks
This method centralizes cross-cutting concerns for invoking external tools: it maps a tool name to an implementation, emits consistent start/finish/error logs with elapsed time and result size, and normalizes both thrown exceptions and tool-level "soft errors" (tool results that begin with "Error") into string responses. By swallowing exceptions and returning an error string, it keeps tool invocation from bubbling exceptions up the call stack and ensures the rest of the agent can continue processing.

## Notes
- Soft-error detection treats any observation starting with the literal "Error" (ordinal, case-insensitive) as a warning-level condition; tools should avoid producing leading "Error" text for successful results to prevent misclassification.
- Exceptions (including cancellation) thrown by the tool are caught and converted into an error message string; callers should not rely on this method to propagate OperationCanceledException or other exceptions.
- The method never returns null: a null observation is converted to an empty string before returning.
- Logs include a preview of arguments/results via Preview(...); the previewing/sanitization behavior is handled elsewhere and only affects logging, not the returned value.

---

## GenerateSummaryAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Builds a concise, factual summary of a conversation by sending a system prompt plus the provided messages (and an optional previous summary) to a chat provider and streaming the model's text output. Use this when you want an updated summary that folds new turns into an existing summary rather than assembling a summary locally.

## Remarks
This method prepares a single user prompt that includes either an "Existing summary..." block followed by new turns or a plain "Conversation to summarize:" block, then resolves the configured chat provider and streams the model's output until a FinishEvent. It intentionally replaces missing message content with marker strings ("(requested tools)" or "(no content)") so the provider sees every turn's presence. Streaming the response allows the caller to receive partial output as the provider generates it and avoids buffering large responses in the provider implementation.

## Example
```csharp
// Typical usage inside an async context
var summary = await GenerateSummaryAsync(
    previousSummary: existingSummary, // or null
    toSummarize: recentMessages,
    selection: modelSelection,       // selects provider and model name
    ct: cancellationToken);

Console.WriteLine(summary);
```

## Notes
- If a Message has null Content, the code substitutes "(requested tools)" when ToolCallsJson is present, otherwise "(no content)"; callers should expect these markers in the prompt seen by the model.
- The returned string is Trim()-ed; it may be empty if the provider returns only whitespace.
- CancellationToken is passed to the provider stream; operation can be aborted by cancelling the token. Exceptions from provider resolution or streaming propagate to the caller.

---

## GetContextMetricsAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns a comprehensive set of token and context metrics for a specific conversation so callers (typically UI telemetry or rendering code) can show how close the conversation is to model limits and whether the backend would trigger compaction/summarization. Use this when you need the current token counts, the model's context window and compact threshold (in both ratio and token count), and a breakdown of tokens by source (system, project, memory, tools, conversation, summary).

## Remarks
This method enforces that a user is authenticated (throws UnauthorizedAccessException) and that the requested conversation exists and is accessible to that user (throws NotFoundException). It resolves the active model selection to obtain the context window and compact threshold, computes the exact token threshold using the same formula MaybeCompactAsync uses (so UI and backend thresholds match), loads any persona/project/memory/tool prompts, builds an AgentContext, and returns a token breakdown produced by the internal token counter.

## Example
```csharp
// Typical usage from an API controller or UI backend:
try
{
    var metrics = await _agentService.GetContextMetricsAsync(conversationId, cancellationToken);
    // use metrics.CurrentTokens, metrics.CompactThresholdTokens, metrics.IsSummarized, etc.
}
catch (UnauthorizedAccessException)
{
    // return 401/redirect to login
}
catch (NotFoundException)
{
    // return 404
}
```

## Notes
- Throws UnauthorizedAccessException if there is no authenticated current user.
- Throws NotFoundException when the conversation cannot be found for the current user.
- CompactThresholdTokens is computed as (int)(ContextWindowTokens * CompactThresholdRatio) when ContextWindowTokens > 0; otherwise it is 0. This matches the backend compaction trigger calculation so UI indicators align with server behavior.

---

## LoadMemoryBlockAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Formats saved user memories into a single system-message block suitable for injecting into a conversation prompt. Use this when building the system prompt for an agent so the model can apply durable facts the user asked the system to remember; the method separates global (user-scope) memories from project-specific ones and returns null when there are no memories to inject.

## Remarks
This method queries the memory store (_memories.ListForConversationAsync) for entries relevant to the given projectId and produces a human-readable block with a short guidance header followed by two optional sections: user-scope memories (apply everywhere) and project-scope memories (apply only to the provided project). The separation helps the model distinguish which facts are globally applicable versus confined to a single project. The method returns null when the store has no entries for the conversation, and it trims trailing whitespace from the resulting block.

## Example
```csharp
// Build conversation messages and inject saved memories if present
var memoryBlock = await LoadMemoryBlockAsync(projectId, cancellationToken);
if (memoryBlock != null)
{
    conversationMessages.Add(new Message(Role.System, memoryBlock));
}
// continue building user/assistant messages...
```

## Notes
- Returns null (not an empty string) when there are no memories — callers should check for null before injecting into system messages.
- The method preserves the ordering provided by the memory store; it does not sort entries itself, so ordering semantics come from ListForConversationAsync.
- Large memory blocks may contribute to model context usage; callers should be mindful of prompt size limits when injecting many or large memories.

---

## LoadProjectSystemPromptAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Builds a project-scoped system prompt block for the current conversation when the conversation belongs to a non-default project. Use this when assembling the full system prompt sent to the agent so the model receives the project's name, a short guidance sentence about memory_save scope selection, and (if present) the project's custom SystemPrompt.

## Remarks
This helper centralizes project-level context so callers don't need to duplicate the memory scope guidance or the logic that decides when project context is applicable. It returns null for conversations with no ProjectId or for the special/default project (including legacy data with no ProjectId), which signals callers to skip injecting project-specific instructions. The method fetches the project using the provided userId (so project resolution is performed in the caller's authorization context) and is asynchronous because it performs I/O.

## Example
```csharp
// Assemble system prompt for an agent call
var projectContext = await LoadProjectSystemPromptAsync(conversation, userId, cancellationToken);
var systemBuilder = new StringBuilder();
if (!string.IsNullOrEmpty(projectContext))
{
    systemBuilder.AppendLine(projectContext);
}
systemBuilder.AppendLine("Base system instructions here...");
var fullSystemPrompt = systemBuilder.ToString();
```

## Notes
- The method returns null for the Default project or when conversation.ProjectId is missing; callers must handle a null result.
- If a project's SystemPrompt exists it is appended verbatim (no additional sanitization is performed here).
- The caller must pass a CancellationToken; the operation is asynchronous and performs a project lookup which can fail or be delayed.

---

## LoadTurnPromptsAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Loads the set of prompt pieces and tool descriptors that are stable for a single conversational turn: the persona (from conversation state and mode), the project-level system prompt, the saved memory block, and the list of tool descriptors filtered by the provided ModelSelection. Use this when preparing the inputs for a single turn so the same prompt components and tool metadata are used by compaction, streaming, and metrics.

## Remarks
This method centralizes retrieval of all prompt-related inputs that should remain constant during one turn. It intentionally resolves the project system prompt, memory block, and persona once and returns them together as a TurnPrompts value so callers (for example, compaction and the streaming provider) observe a consistent set of inputs. Tool descriptors are derived from the current ModelSelection: if ToolMode is None the method returns an empty descriptor list so downstream logic and metrics treat the turn as having no tools available.

## Notes
- The method performs asynchronous I/O (project prompt and memory load) and honors the provided CancellationToken; callers should pass a token and handle cancellation.
- When selection.ToolMode == ToolMode.None the returned Tools list is empty — this both prevents advertising tool capabilities and yields zero in the tools metrics bucket.
- Persona content is produced from the conversation state and mode at call time; call this once per turn and treat the returned TurnPrompts as the immutable prompt snapshot for that turn.


---

## MaybeCompactAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Decides whether the conversation should be rolled up into a compact summary for the current turn and, when appropriate, performs that summarization via an asynchronous LLM call. Consumers enumerate the returned `IAsyncEnumerable<AgentEvent>` to receive lifecycle events (AgentCompactStart followed by AgentCompactDone). The method compares the full AgentContext token breakdown (persona, project, memory, summary, tools and conversation) against a model-specific or global compact threshold; if the current token usage exceeds the threshold it selects an earlier cut index, avoids re-summarizing already-covered messages, emits a start event so a UI can show a compaction overlay, calls GenerateSummaryAsync to produce the new summary, updates the Conversation with the new summary, persists changes, and finally emits a done event with the summary size.

## Remarks
This routine exists to keep the provider-visible context inside a model's effective window by proactively summarizing older conversation history. Unlike estimators that consider only the existing summary plus recent messages, it uses the full AgentContext breakdown so large project prompts or memory blocks also contribute to the compaction decision. The method yields events so callers can react (e.g., show/hide a UI overlay) while the potentially slow LLM summarization runs.

## Example
```csharp
// Typical consumer: enumerate events and react to start/done so UI can show progress.
await foreach (var evt in agentService.MaybeCompactAsync(conv, prompts, selection, cancellationToken))
{
    switch (evt)
    {
        case AgentCompactStart start:
            // show compaction overlay, e.g. indicate how many messages will be summarized
            ShowOverlay(start.MessageCount, start.CurrentTokens, start.Threshold);
            break;
        case AgentCompactDone done:
            // hide overlay and update UI with new summary token estimate
            HideOverlay();
            UpdateSummaryDisplay(done.SummaryTokens);
            break;
    }
}
```

## Notes
- Enumeration required: nothing happens until the returned IAsyncEnumerable is iterated.
- Short-circuits: if the model window is <= 0, current tokens are below threshold, there are no messages to summarize, or the cut index would re-cover already summarized messages, the method yields nothing.
- Failure handling: failures or empty summaries from the LLM are caught; a AgentCompactDone with zero summary tokens is emitted so callers can clear any UI overlay and the turn proceeds without compaction. The conversation is only updated and persisted when a non-empty summary is returned.

---

## Preview

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Returns a single-line, truncated preview of the provided text suitable for logging. Null or empty input yields the literal "(empty)"; newlines are collapsed to spaces and the result is cut to the configured LogPreviewLimit with a Unicode ellipsis appended when truncation occurs.

## Remarks
Use this helper when you need a concise, single-line representation of potentially multi-line or very long strings (for example, tool output or user-provided text) so log lines remain readable and consistent. The visible length is governed by the LogPreviewLimit constant in the same class; changing that value adjusts how aggressively previews are truncated.

## Example
```csharp
// Typical usage
var a = Preview(null);                   // "(empty)"
var b = Preview("");                     // "(empty)"
var c = Preview("first line\nsecond"); // "first line second" (may be truncated if over LogPreviewLimit)

// If LogPreviewLimit is 10:
var d = Preview("0123456789ABC");       // "0123456789…"
```

## Notes
- Returns the literal "(empty)" for null or empty input instead of an empty string.
- Newline characters '\n' and '\r' are replaced with spaces; consecutive line breaks can produce adjacent spaces (no further normalization is performed).
- Truncation appends a single Unicode ellipsis (…), not three ASCII dots.

---

## RegenerateAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Regenerates an assistant reply for a past assistant message in a conversation and returns a streaming sequence of AgentEvent produced by the new run.

Call this when a user requests a regenerated assistant response for an existing assistant message (for example, a "Regenerate" action in the UI). The method validates the caller, ensures the target message is an active assistant variant, deactivates the current variant group (so the old alternatives become inactive), persists that change, then re-runs the turn using the same conversation state and variant group id. The returned `IAsyncEnumerable<AgentEvent>` yields the streaming events produced by the new assistant execution.

## Remarks
This method intentionally does not create a new user message — it reuses the original turn state so the regenerated reply is produced against the same history/state that produced the original reply. Deactivating the existing variant group before running ensures the old variant(s) are marked inactive and the new reply re-uses the same variantGroupId so UI pickers can correlate alternatives.

Errors thrown by the method indicate precondition failures: UnauthorizedAccessException when there is no authenticated user, NotFoundException when the conversation or message cannot be found, and DomainException when the target is not an assistant message or is not the active variant.

## Example
```csharp
// Trigger regeneration and consume streaming events
var streamTask = agentService.RegenerateAsync(conversationId, assistantMessageId, cancellationToken);
await foreach (var agentEvent in await streamTask.WithCancellation(cancellationToken))
{
    // render partial text updates, tool calls, or final result
    HandleAgentEvent(agentEvent);
}
```

## Notes
- Side effects: the method updates the conversation to deactivate the existing variant group and persists that change before streaming begins.
- The regeneration runs against the same conversation state as the original reply; no new user message or state mutation is introduced by this method itself.
- CancellationToken is honored by underlying operations; callers should pass a token to cancel long-running generation.
- Consumers must handle the streaming semantics of IAsyncEnumerable (exceptions from the run may be raised while enumerating).

---

## ResolveModelSelectionAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Resolves the effective ModelSelection for the current user by reading persisted user preferences and asking the model catalog to map those preferences to a concrete selection. Use this when handling a request turn where the agent must choose which provider/model to use according to the user's current settings; calling this method ensures any preference changes are respected immediately.

## Remarks
This method centralizes model-resolution logic so all agent entry points obtain a consistent, up-to-date ModelSelection. It first fetches the user's persisted preferences (via _userPrefs.GetAsync) and then delegates to _modelCatalog.Resolve to produce the ModelSelection. Because it reads preferences on every call, changes made in settings take effect on the next turn without needing to restart or rehydrate state.

## Example
```csharp
// inside an agent request handler
var ct = CancellationToken.None; // or a real token from the request
var selection = await ResolveModelSelectionAsync(ct);
// selection now contains the provider/model resolved for the current user
```

## Notes
- The method awaits _userPrefs.GetAsync with the provided CancellationToken; cancellation will stop preference retrieval and propagate (e.g., OperationCanceledException).
- There is no in-method caching: preferences are fetched each time to ensure immediacy of preference changes — consider broader caching if this becomes a performance bottleneck.
- _modelCatalog.Resolve is synchronous here; ensure the catalog handles unknown or invalid preference values appropriately (defaults or errors) since this method does not perform additional validation.

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


Starts a single ReAct-style agent turn for the given conversation and returns a streaming sequence of AgentEvent items that represent the turn's lifecycle (compaction, tool calls, partial responses, final reply). Use this when you want to run an agent turn and stream incremental updates (for example over SSE) instead of waiting for a single, fully-rendered reply.

## Remarks
This method performs several important preparatory steps synchronously/early: it validates input and user authentication (so callers get clean 4xx responses before any streaming headers are emitted), loads and persists a new user message in the conversation timeline, resolves the model selection for the turn, loads stable prompt pieces (persona, project, memory, tools), and sets a scoped tool execution context. Persisting the user message and saving the unit of work before the stream starts keeps the conversation timeline consistent even if the client disconnects mid-stream. The returned IAsyncEnumerable is created by RunStreamWithUserPreambleAsync and will emit a preamble event (AgentUserMessagePersisted) so clients can replace any temporary client-side message id with the real DB id.

## Example
```csharp
// Consume the streaming events (e.g. in an SSE or WebSocket handler)
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
await foreach (var ev in agentService.RunAsync(conversationId, "Hello, can you help?", cts.Token).WithCancellation(cts.Token))
{
    // Inspect and dispatch events to your UI. The first event will be
    // AgentUserMessagePersisted so the client can swap a temporary id for
    // the persisted message id.
    HandleAgentEvent(ev);
}
```

## Notes
- Throws DomainException if userInput is null/empty; this validation happens before any SSE/stream headers are sent so the global exception handler can return ProblemDetails (4xx).
- Throws UnauthorizedAccessException when there is no authenticated user, and NotFoundException when the conversation doesn't exist or isn't accessible to the user.
- The method persists and saves the user message before streaming; clients should not assume the absence of persisted state if the stream is interrupted.
- The IAsyncEnumerable must be enumerated to start the agent turn; cancellation is supported via the provided CancellationToken.

---

## RunStreamAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Streams a single agent turn as an asynchronous sequence of AgentEvent values. Consumers use this when they need incremental output (text deltas and internal "reasoning" deltas), to observe when the provider requests tool calls, and to receive tool results — for example when driving a UI with streaming model output or when coordinating tool execution during a turn.

## Remarks
This method first runs a compaction step so the conversation's summary is up to date before building the provider context. It then selects and possibly wraps a chat provider (GabrielToolBridge when ToolMode.Emulated) and enters an iteration loop (bounded by the configured MaxIterations). Each iteration rebuilds AgentContext from the conversation and prompts so that tool calls and results produced in previous iterations become part of the history sent to the model. The provider's streaming events are translated into AgentEvent subtypes (text deltas, reasoning deltas, tool-call readiness, finish reasons) and yielded immediately to the caller. The implementation includes a bounded retry for the case where a provider returns an empty Stop/Finish (no text, no tool calls), which is treated as a transient hiccup.

## Example
```csharp
await foreach (var evt in RunStreamAsync(conversation, variantOverride, prompts, selection, cancellationToken))
{
    switch (evt)
    {
        case AgentTextDelta t: // model text streaming
            AppendToOutput(t.Delta);
            break;

        case AgentReasoningDelta r: // internal chain-of-thought or reasoning markers
            LogReasoning(r.Delta);
            break;

        case AgentToolCall c: // provider requested a tool call
            await ExecuteToolAsync(c);
            break;

        case AgentToolResult res: // result of a previously executed tool
            ApplyToolResultToConversation(res);
            break;
    }
}
```

## Notes
- The async enumerable honors the provided CancellationToken (EnumeratorCancellation) — cancel the token to stop the stream promptly.
- The conversation object is modified (compaction, appended tool calls/results and summary updates). Do not reuse the same Conversation instance concurrently from multiple threads or streams.
- Providers can emit an empty Finish/Stop; the method retries a bounded number of times to tolerate transient empty streams before surfacing an error.

---

## RunStreamWithUserPreambleAsync

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Emits a persisted-user preamble event for the given userMessageId, then forwards every event produced by RunStreamAsync for the same inputs. Use this when callers must observe a AgentUserMessagePersisted event before any subsequent agent stream events.

## Remarks
This method wraps the main streaming operation to guarantee a deterministic ordering: the AgentUserMessagePersisted event for the user message is yielded first, then the method relays all events coming from RunStreamAsync. It preserves cancellation and exception behaviour from the inner RunStreamAsync — the preamble is yielded before the inner stream is iterated, so consumers will observe the persisted-user event even if the inner stream fails immediately.

## Example
```csharp
await foreach (var evt in RunStreamWithUserPreambleAsync(userMessageId, conversation, variantGroupId, prompts, selection, ct))
{
    switch (evt)
    {
        case AgentUserMessagePersisted up:
            // handle persisted user message
            break;
        case SomeOtherAgentEvent e:
            // handle other events
            break;
    }
}
```

## Notes
- The method is private and intended for internal sequencing; callers outside the class should use the public APIs that reference it.
- Exceptions thrown while enumerating the inner RunStreamAsync propagate to the consumer and will terminate the async iteration after the preamble has been yielded.
- The CancellationToken passed in is used to cancel the async iteration (the parameter is annotated with EnumeratorCancellation to allow cancellation from the enumerator caller).

---

## SelectCompactCutIndex

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

```csharp
// Walks back from the end keeping at least `keepLast` messages, then keeps walking
    // until we land on a User message - that's our cut boundary. Doing this avoids
    // ever cutting between an assistant's tool_calls and the matching tool results,
    // which the model needs to see together.
    private static int SelectCompactCutIndex(IReadOnlyList<Message> messages, int keepLast)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `messages` | ``IReadOnlyList<Message>`` | — |
| `keepLast` | `int` | — |

**Returns:** `// Walks back from the end keeping at least `keepLast` messages, then keeps walking
    // until we land on a User message - that's our cut boundary. Doing this avoids
    // ever cutting between an assistant's tool_calls and the matching tool results,
    // which the model needs to see together.
    private static int`


Determines where to cut a conversation when compacting history: it preserves at least keepLast messages at the end, then walks backward until it finds a User message and returns that message's index. Use this when trimming history for a compact model context while ensuring you don't split an assistant's tool call from its corresponding tool results.

## Remarks
This routine ensures the compacted window starts at a user-turn boundary so assistant responses (including tool calls and their results) remain intact. It is used by the agent's history-trimming logic to prefer the most recent user prompt while still guaranteeing a minimum number of trailing messages are preserved.

## Example
```csharp
// Keep at least the last 3 messages, but start the compacted history at the nearest prior User message
int start = SelectCompactCutIndex(messages, 3);
// Use messages[start..] as the compacted context
```

## Notes
- The returned index is inclusive: callers should keep messages from that index to the end. If no User message is found before the keep-last boundary the method returns 0 (start of history).
- The implementation assumes keepLast >= 1 when messages contains items. Passing keepLast == 0 with a non-empty messages list can lead to an IndexOutOfRangeException because the code may access messages[messages.Count]. Ensure callers pass a positive keepLast or validate before calling.
- If messages is empty or keepLast >= messages.Count the method will return 0.

---

## SerializeToolCalls

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** method

Serializes a sequence of ChatProviderToolCall objects into a JSON array where each element has the shape { id, type: "function", function: { name, arguments } }. Use this helper when you need the tool-call payload formatted as JSON for a chat provider or LLM that expects a list of function/tool invocations.

## Remarks
This method projects each ChatProviderToolCall into an anonymous object and relies on System.Text.Json.JsonSerializer to produce the final JSON string. It intentionally preserves the caller-provided ArgumentsJson value as a string rather than parsing or merging it into the resulting JSON object, avoiding assumptions about argument structure.

## Example
```csharp
var calls = new List<ChatProviderToolCall>
{
    new ChatProviderToolCall { Id = "1", Name = "doThing", ArgumentsJson = "{\"x\":1}" },
    new ChatProviderToolCall { Id = "2", Name = "setFlag", ArgumentsJson = "{\"enabled\":true}" }
};

string json = SerializeToolCalls(calls);
// json => "[{\"id\":\"1\",\"type\":\"function\",\"function\":{\"name\":\"doThing\",\"arguments\":\"{\\\"x\\\":1}\"}},{\"id\":\"2\",\"type\":\"function\",\"function\":{\"name\":\"setFlag\",\"arguments\":\"{\\\"enabled\\\":true}\"}}]"
```

## Notes
- The method does not validate inputs; passing a null calls collection will throw a NullReferenceException.
- ArgumentsJson is treated as a plain string and will be JSON-escaped in the resulting payload. If the consumer expects a nested JSON object for arguments, parse ArgumentsJson into a JsonDocument or build a structured object before serialization.
- System.Text.Json default options are used (no indentation, default naming), so adjust serialization options elsewhere if a different output format is required.

---

## EmptyStopMaxRetries

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

Specifies the number of additional retry attempts the agent will make when it observes a provider "Stop" event with an empty response body. Reach for this constant when reasoning about how many transient empty-stop hiccups the agent tolerates before treating the stop as final (the constant represents extra retries beyond the original attempt).

## Remarks
This constant exists because the HTTP resilience pipeline cannot detect a logically empty "Stop" payload when the HTTP response itself is a successful 200 stream; the retry logic must therefore live in the agent loop. The chosen value (2) yields two extra attempts — three total attempts including the initial one — which is intended to recover from transient empty responses without noticeably slowing down genuine failures. Retries use a simple linear backoff: attempt N waits N * EmptyStopRetryDelayMs.

## Notes
- The value counts extra retries; total attempts = EmptyStopMaxRetries + 1 (initial attempt + extras).
- It's a private compile-time constant (not runtime-configurable); changing it requires a code change and redeploy.
- Backoff timing depends on EmptyStopRetryDelayMs, so both constants together determine overall retry delay.

---

## EmptyStopRetryDelayMs

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

```csharp
private const int EmptyStopRetryDelayMs = 500
```


A small, fixed backoff value (in milliseconds) used internally by AgentService when a stop operation retries after observing no work to perform. It prevents tight busy-wait loops by introducing a short pause between retry attempts and centralizes the retry timing so it can be adjusted in one place.

## Remarks
This constant exists to balance responsiveness with CPU usage during stop/retry loops: a short, fixed delay reduces spinning while still allowing the service to re-check conditions frequently. Keeping the value as a named constant makes the backoff policy easy to tune and documents the intent (avoid busy-wait) for future maintainers.

## Notes
- Value is in milliseconds; use with Task.Delay or Thread.Sleep accordingly.
- Because it is a compile-time constant, changing it requires rebuilding the assembly.
- Reducing the value can increase CPU usage; increasing it delays responsiveness to stop conditions.

---

## LogPreviewLimit

> **File:** `src/api/Gabriel.Engine/Services/AgentService.cs`  
> **Kind:** field

Maximum number of characters to include when embedding tool arguments or results in a log message. Use this constant to bound how much of a tool's input or output is written to logs so large payloads (for example, fetched web pages or large file contents) do not bloat the log files and obscure useful diagnostics.

## Remarks
Centralizes the preview-size policy for AgentService logging so truncation behavior is consistent for both tool arguments and results. Keeping the value small preserves readability and storage; increase it only when more context in logs is required for debugging.

## Notes
- This value is a character count (not a byte count); multibyte encodings may occupy more bytes than the character length suggests.
- It's a compile-time constant — changing it requires rebuilding and will affect all AgentService log previews.
- The constant only limits what is written to logs; it does not alter the underlying tool inputs or outputs.

---