# Agent loop

## PURPOSE
The ReAct iteration in `Gabriel.Engine.Services.AgentService`: how one chat turn runs from request to final reply, including streaming, tool calls, the rolling compact, and regenerate.

## USE WHEN
- User asks how a chat turn works.
- User asks about streaming events, SSE wire format, `AgentEvent` shapes.
- User asks about regenerate, max iterations, or how the model decides to call a tool.
- User asks about context window / compact / summarization behavior.
- User reports a "stuck" or "empty" reply.

## QUICK REFERENCE

| Concept | Value |
| --- | --- |
| Entry points | `IAgentService.RunAsync(convId, userInput, ct)` and `RegenerateAsync(convId, assistantMessageId, ct)` |
| Max iterations | `AgentOptions.MaxIterations` (default `8`) |
| Compact threshold | `AgentOptions.CompactThreshold` (default `0.8` of provider context window) |
| Compact keep-last | `AgentOptions.CompactKeepLast` (default `6` messages) |
| Empty-stop retries | `AgentService.EmptyStopMaxRetries` (default `2`, so 3 attempts) |
| Empty-stop backoff | `AgentService.EmptyStopRetryDelayMs * attempt` (default `500ms × N`) |
| Token estimate | `⌈chars/4⌉ + 8` per message (naive; behind `ITokenEstimator`) |
| Tool transport | Per-model `LLMModel.ToolMode` ∈ {`Native`, `Emulated`, `None`}. `Emulated` routes through `GabrielToolBridge` (XML-tagged JSON in the assistant stream); `None` drops tool descriptors entirely. |
| Per-turn assembly | `AgentContext.Build(conversation, persona, projectPrompt, memoryBlock, tools)` → single value object passed to both the provider call and the metrics breakdown. |

## DETAILS

### Pre-flight validation (throws synchronously, before SSE headers)

| Condition | Exception | HTTP |
| --- | --- | --- |
| Empty user input | `DomainException` | 400 |
| No current user | `UnauthorizedAccessException` | 401 |
| Conversation not found | `NotFoundException` | 404 |
| Regenerate target not assistant or already inactive | `DomainException` | 400 |

In-stream failures cannot change HTTP status; they emit a final `AgentError` event on the SSE stream.

### Iteration loop (per turn)

1. Load conversation (user-scoped) via `IConversationRepository`.
2. (`RunAsync` only) Append user message, update `ConversationState` via `IConversationStateUpdater`, save.
3. Resolve `ModelSelection` from the user's preferences (provider + model + `ToolMode`).
4. Load per-turn prompt pieces via `LoadTurnPromptsAsync` (persona, project prompt, memory block, tool descriptors). `ToolMode.None` hands back an empty tool list at this step so the provider call doesn't advertise capabilities the model can't use.
5. Set the scoped `IToolExecutionContext` so project-aware tools know their project.
6. (`RunAsync` only) Wrap the stream with `RunStreamWithUserPreambleAsync` — yields `AgentUserMessagePersisted` first so the client can swap its `tmp-xxxxx` user-entry id for the real DB id without a follow-up GET.
7. `MaybeCompactAsync` runs **inside** `RunStreamAsync`, so it can yield `AgentCompactStart` / `AgentCompactDone` events to the SSE wire and the UI can show a "Compacting…" overlay (see "Rolling compact").
8. Build provider history via `AgentContext.ToProviderHistory()` (system prompt + project context + memory + summary + filtered active messages).
9. Loop up to `MaxIterations`:
   - `IChatProvider.StreamAsync(history, tools)` yields a stream of `ChatProviderEvent`. When `ToolMode == Emulated`, the registered provider is wrapped by `GabrielToolBridge`, which translates between the model's XML-tagged-JSON tool transport and the same `ChatProviderEvent` shape the native path uses — the loop sees no difference.
   - Forward text/reasoning deltas to the caller as `AgentEvent`.
   - On `FinishEvent.ToolCalls`: persist assistant tool-calls msg, execute each tool serially via `IToolRegistry`, persist each tool result, continue loop.
   - On `FinishEvent.Stop`: run `IResponsePostProcessor.Clean`, persist assistant text, yield `AgentAssistantMessage` + `AgentDone`, exit.
   - On `FinishEvent.Length` / `Error` / unexpected: yield `AgentError` + `AgentDone`, exit.
10. If max iterations hit without `Stop`: persist `"(stopped after N tool iterations)"`, yield `AgentDone`.

### Streaming events

Provider emits `ChatProviderEvent`:

| Type | Meaning |
| --- | --- |
| `TextDeltaEvent(Delta)` | Partial assistant text. |
| `ReasoningDeltaEvent(Delta)` | Partial native chain-of-thought (Grok 4 `reasoning_content`, etc.). |
| `ToolCallReadyEvent(Id, Name, ArgsJson)` | Fully-assembled tool call. |
| `FinishEvent(FinishReason)` | `Stop` / `ToolCalls` / `Length` / `Error`. |

Agent forwards as `AgentEvent` to the SSE wire:

| Type | When |
| --- | --- |
| `AgentUserMessagePersisted(MessageId)` | First event of every `RunAsync`-driven turn. Carries the real DB id of the just-persisted user message so the client can swap its optimistic `tmp-xxxxx` id in place. Not emitted by `RegenerateAsync` (no new user message). |
| `AgentCompactStart(MessageCount, CurrentTokens, ThresholdTokens)` | Before the rolling-summary LLM call, when the pre-compact total has crossed the threshold. Lets the UI swap to a "Compacting…" overlay. |
| `AgentCompactDone(MessageCount, SummaryTokens)` | After the summary call returns successfully. Always paired with a preceding `AgentCompactStart`; skipped entirely when summarization fails (the UI then sees a long thinking phase but no compact pair, which is fine). |
| `AgentTextDelta(Delta)` | Each text delta, unmodified. |
| `AgentReasoningDelta(Delta)` | Each reasoning delta. |
| `AgentToolCall(MessageId, ToolCallId, Name, ArgsJson)` | After persisting the assistant's tool-call msg. |
| `AgentToolResult(MessageId, ToolCallId, Content)` | After tool execution + persistence. |
| `AgentAssistantMessage(MessageId, Content, ReasoningContent?)` | Final assistant text. |
| `AgentError(Message)` | In-stream failure. |
| `AgentDone()` | Terminal. |

JSON discriminator is `type` (e.g., `"textDelta"`, `"toolCall"`, `"userMessagePersisted"`, `"compactStart"`). Webapp switches on it in `streamChat.ts`.

### Two reasoning channels (both can fire per iteration)

1. **Native CoT** (`reasoning_content`). Streamed as `ReasoningDeltaEvent`, persisted on `Message.ReasoningContent`. Ephemeral: NOT re-fed to the model on the next iteration.
2. **External ReAct reasoning**. Regular `content` emitted alongside a `tool_calls` message — the "Thought" in Thought→Action→Observation. Persisted on `Message.Content` of an assistant-with-tool-calls row. IS re-fed in subsequent iterations.

Never collapse these into one channel; the data model and UI treat them as separate.

### History assembly (`AgentContext.ToProviderHistory`)

Assembly is consolidated into the `AgentContext` value object so the live provider call and the metrics breakdown see exactly the same bytes (they used to diverge — metrics ignored persona / project prompt / memory / tools entirely). Order matters for prefix-caching:

1. Persona system message (`PersonaStatic` + `PersonaFormatting` + active mode fragment + per-turn `[Conversation metadata]` + few-shot).
2. `[Project context]` block, when the conversation is in a non-default project. Carries the project name, a directive to default `memory_save` to `scope='project'`, and the user's optional per-project `SystemPrompt`. Omitted for Default-project / standalone conversations.
3. `[Saved memories]` block — user-scope first, then project-scope (current project only).
4. `[Summary of earlier conversation]` block — the rolling summary, when present.
5. Filtered active messages (the conversation tail):
   - Skip messages at or before `Conversation.SummarizedThroughMessageId`.
   - Non-tool messages: keep only `IsActiveVariant == true`.
   - Tool messages: keep only if their `ToolCallId` is referenced by an active assistant's `ToolCallsJson` (catches orphans from deactivated regen branches and legacy data).

The order — system → tools (sibling field) → project → memory → summary → conversation — is fixed left-to-right because every provider we target caches on a prefix match. Reordering would invalidate the cache for every following turn.

### Rolling compact

Trigger between turns:

```
T_history = T_summary + Σ est(m_i)        for post-cut messages
fires when T_history ≥ θ × W_provider
θ = AgentOptions.CompactThreshold (default 0.8)
W = IChatProvider.ContextWindowTokens (e.g., 1_000_000 for grok-4.x, 8_000 for mock)
```

Cut-point: walk back from the end keeping ≥ `CompactKeepLast` messages, then keep walking until you land on a User-role message. If no such index exists (`cut ≤ 0`), skip.

Summarization uses the provider with an empty tool list and a fixed system prompt. Result stored on `Conversation.Summary` and `Conversation.SummarizedThroughMessageId`. Failure or empty summary → log warning and skip; retried next turn.

Compaction runs **inside** `RunStreamAsync` (not before it) so it can yield `AgentCompactStart` / `AgentCompactDone` events to the SSE wire. The UI then shows a "Compacting…" overlay while the summarization LLM call burns its 5-30s before the real turn starts, instead of staring at a silent HTTP request. The pair always brackets the work: `AgentCompactStart` first, `AgentCompactDone` only on success — failure paths skip `AgentCompactDone` and the loop continues without a fresh summary.

Never compacts mid-iteration.

### Tool transport modes

Each model in the catalog declares a `ToolMode` (default `Native`):

| Mode | What happens |
| --- | --- |
| `Native` | Provider's first-class `tools` field carries the descriptors; provider emits `ToolCallReadyEvent`s as structured JSON. The straight path. |
| `Emulated` | `GabrielToolBridge` decorates `IChatProvider` at registration. The bridge: (a) injects tool documentation into the system message at the system→conversation boundary, (b) translates persisted assistant `ToolCallsJson` into inline `<tool_call>{...}</tool_call>` blocks in the assistant content stream and translates `Tool`-role messages into synthetic user "[Tool result: name]" messages, (c) runs a prefix-aware lookahead splitter over the model's content stream to detect `<tool_call>` blocks, parse them into `ToolCallReadyEvent`s, and forward everything else as `TextDeltaEvent`s. The agent loop sees the same `ChatProviderEvent` shape either way. On malformed JSON inside a block, the bridge retries the turn up to `MaxParseRetries` (default 2) with a "fix the JSON" prompt before bubbling the failure up. |
| `None` | Tool descriptors are dropped at `LoadTurnPromptsAsync`; the metrics breakdown reports `ToolsTokens = 0` and the provider call never advertises a capability the model can't use. For chat-only models. |

The choice is per-model, set in `Providers:Grok:Models[*].ToolMode` (or equivalent for future providers). The user never picks transport — they pick a model, the model declares how its tools should be transported. |

### Empty-Stop retry

If a provider stream completes with `FinishEvent.Stop`, no text, no reasoning, no tool calls — agent retries that iteration (same history) up to `EmptyStopMaxRetries` times with linear backoff. Any text delta or any tool call commits the attempt and disables retry for that iteration.

### Save-vs-stream split

- Streaming wire ships **raw** model text via `AgentTextDelta`.
- DB persists the **cleaned** version (after `IResponsePostProcessor.Clean`). Cleaner only strips AI-ism openers/closers; it does not truncate.
- If cleaner empties the text, persistence falls back to the raw text.

### Regenerate flow

1. Load conversation, find target assistant message.
2. Validate it is assistant + `IsActiveVariant`.
3. `Conversation.DeactivateVariantGroup(target.VariantGroupId)` — every message in that group flips to `IsActiveVariant = false`.
4. Save.
5. `MaybeCompactAsync` (standard).
6. Run the regular ReAct loop with `variantGroupIdOverride = target.VariantGroupId`, so every new message inherits the original group id.

After completion, the variant group has ≥ 2 sibling turn-sequences (old inactive, new active).

`RegenerateAsync` does NOT update state — the existing state already reflects the user message being re-answered.

## INVARIANTS

- Tool calls within an iteration run **serially**, never in parallel.
- Compact never fires mid-iteration; only between turns.
- `MaxIterations` is a hard ceiling. Past it, the loop bails with a placeholder.
- `Message.ReasoningContent` is persisted for transparency but never added to `ToProviderHistory`.
- `RegenerateAsync` does not re-run `IConversationStateUpdater`.
- A tool message in history is only legal if a referenced `ToolCallId` exists in an active assistant's `ToolCallsJson`. Otherwise it is filtered out.

## PITFALLS

- Don't claim "Gabriel re-feeds its own chain-of-thought" — only the visible `Content` channel (including ReAct "Thought" preludes) is re-fed.
- The user sees raw text live; reload shows cleaned text. They're nearly identical, but if a user notices a difference, this is why.
- "Why did the reply stop early?" is more often `MaxIterations` than `Length`. Check `iters` in the structured log line at turn-end.
- An "empty" reply that the user re-sends usually means the empty-stop retry already exhausted its budget.

## SEE ALSO

- `personality.md` — what the system prompt looks like per turn.
- `tools.md` — the tools the loop dispatches to.
- `variants.md` — `VariantGroupId` mechanics in regen / delete.
- `config.md` — agent / provider knobs.
- Human-prose companion: `Gabriel.Engine/agent-loop.md`.
