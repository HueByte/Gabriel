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
3. `MaybeCompactAsync` between turns (see "Rolling compact").
4. Build provider history via `ToProviderHistory` (system prompt + summary + filtered active messages).
5. Loop up to `MaxIterations`:
   - `IChatProvider.StreamAsync(history, tools)` yields a stream of `ChatProviderEvent`.
   - Forward text/reasoning deltas to the caller as `AgentEvent`.
   - On `FinishEvent.ToolCalls`: persist assistant tool-calls msg, execute each tool serially via `IToolRegistry`, persist each tool result, continue loop.
   - On `FinishEvent.Stop`: run `IResponsePostProcessor.Clean`, persist assistant text, yield `AgentAssistantMessage` + `AgentDone`, exit.
   - On `FinishEvent.Length` / `Error` / unexpected: yield `AgentError` + `AgentDone`, exit.
6. If max iterations hit without `Stop`: persist `"(stopped after N tool iterations)"`, yield `AgentDone`.

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
| `AgentTextDelta(Delta)` | Each text delta, unmodified. |
| `AgentReasoningDelta(Delta)` | Each reasoning delta. |
| `AgentToolCall(MessageId, ToolCallId, Name, ArgsJson)` | After persisting the assistant's tool-call msg. |
| `AgentToolResult(MessageId, ToolCallId, Content)` | After tool execution + persistence. |
| `AgentAssistantMessage(MessageId, Content, ReasoningContent?)` | Final assistant text. |
| `AgentError(Message)` | In-stream failure. |
| `AgentDone()` | Terminal. |

JSON discriminator is `type` (e.g., `"textDelta"`, `"toolCall"`). Webapp switches on it in `streamChat.ts`.

### Two reasoning channels (both can fire per iteration)

1. **Native CoT** (`reasoning_content`). Streamed as `ReasoningDeltaEvent`, persisted on `Message.ReasoningContent`. Ephemeral: NOT re-fed to the model on the next iteration.
2. **External ReAct reasoning**. Regular `content` emitted alongside a `tool_calls` message — the "Thought" in Thought→Action→Observation. Persisted on `Message.Content` of an assistant-with-tool-calls row. IS re-fed in subsequent iterations.

Never collapse these into one channel; the data model and UI treat them as separate.

### History assembly (`ToProviderHistory`)

1. Prepend per-turn system prompt from `ISystemPromptBuilder.Build(state)`.
2. Prepend rolling summary (if any) as a second system message.
3. Skip pre-summary messages when `Conversation.SummarizedThroughMessageId` is set.
4. Filter:
   - Non-tool messages: keep only `IsActiveVariant == true`.
   - Tool messages: keep only if their `ToolCallId` is referenced by an active assistant's `ToolCallsJson` (catches orphans from deactivated regen branches and legacy data).

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

Never compacts mid-iteration.

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
