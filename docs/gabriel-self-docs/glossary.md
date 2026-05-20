# Glossary

## PURPOSE
Canonical definitions of project-specific terms used across the other docs.

## USE WHEN
- A term in another self-doc isn't immediately clear.
- A user uses one of these terms and you want to confirm its precise meaning.

## TERMS

**Agent loop** — the iterative ReAct cycle in `AgentService` that streams from `IChatProvider`, dispatches `tool_calls`, persists results, and loops until `Stop` or `MaxIterations`. See `agent-loop.md`.

**AgentEvent** — polymorphic DTO yielded by the agent to the SSE controller. Subtypes: `AgentTextDelta`, `AgentReasoningDelta`, `AgentToolCall`, `AgentToolResult`, `AgentAssistantMessage`, `AgentError`, `AgentDone`. JSON discriminator is `type`.

**AvatarSeed** — `long` on `Conversation` that drives the Gabriel Sequence palette family, pattern kind, and pattern parameters. Changing it ("reroll-avatar") regenerates the avatar.

**ChatProviderEvent** — transport-level event emitted by `IChatProvider.StreamAsync`. Subtypes: `TextDeltaEvent`, `ReasoningDeltaEvent`, `ToolCallReadyEvent`, `FinishEvent(FinishReason)`.

**Compact (rolling)** — between-turn summarization that fires when estimated history tokens ≥ `θ × W_provider`. The earliest portion of history is replaced by a summary stored on `Conversation.Summary`; `Conversation.SummarizedThroughMessageId` records the cut.

**Composite docs lookup** — `IDocsLookup` implementation that fans across an ordered list of inner sources. Primary: `LocalDocsLookup` (LLM-native folder). Fallback: `GitHubDocsLookup`.

**ConversationState** — domain value object (Core) holding `Mood`, `TurnCount`, length stats, user-style flags, and `UserAskedForDetail`. Persisted as JSON on `Conversation.StateJson`.

**DocsContent** — record returned by `IDocsLookup.ReadAsync`: `(Path, Content, CanonicalUrl)`. The `docs_read` tool wraps it with `=== BEGIN OFFICIAL GABRIEL DOC ===` markers and the `source` tag (local vs github).

**DocsEntry** — record returned by `IDocsLookup.ListAsync`: `(Path, Title)`. The composite source also surfaces a `Source` tag in the list output.

**External ReAct reasoning** — the model's plain-text "Thought" preamble emitted alongside a `tool_calls` message. Persisted on `Message.Content` and **re-fed** to subsequent iterations. Distinct from native CoT.

**FinishReason** — terminal status on a provider call: `Stop` / `ToolCalls` / `Length` / `Error`.

**Gabriel Sequence** — 64-frame 16×16 RGB avatar engine. Layers: DNA Core, Stable Traits, Context, Live State. Generated on demand from `(AvatarSeed, ConversationState)`. See `sequence.md`.

**Heuristic state updater** — `HeuristicConversationStateUpdater`. Regex-driven, zero-LLM-cost classifier of mood / task-mode / user style flags.

**IDocsLookup** — interface in `Gabriel.Engine.Tools.Docs` exposing `ListAsync` + `ReadAsync`. Implementations live in `Gabriel.Infrastructure.Tools.Docs`.

**Inactive variant** — a `Message` with `IsActiveVariant == false`. Still in the DB; filtered out of `ToProviderHistory` and the API messages array.

**Live State** — frames 48..63 of a Gabriel Sequence. Only layer modulated by `ConversationState`. Palette window + intensity + phase nudge are mood-driven.

**LLM-native self-docs** — the `docs/gabriel-self-docs/` folder. Compact, fact-dense, structured for an LLM (Gabriel) to consume via `docs_list` / `docs_read`. Primary source.

**MaybeCompactAsync** — pre-iteration check in `AgentService` that may run `GenerateSummaryAsync` and fold the result into `Conversation.Summary`. Only runs between turns.

**MessageRole** — enum: `System` / `User` / `Assistant` / `Tool`.

**Mood** — enum on `ConversationState`: `Neutral | Playful | Venting | Serious | Curious | LowEnergy`. Drives system-prompt guidance and the Live State palette window.

**Native CoT** — provider-side chain-of-thought channel (`reasoning_content` in Grok 4, `thinking_blocks` in Anthropic, etc.). Streamed as `ReasoningDeltaEvent`, persisted on `Message.ReasoningContent`, NOT re-fed to subsequent iterations.

**Persona** — the static block of the system prompt: name, behavioral rules, hard prohibitions, few-shot examples. Assembled by `GabrielSystemPromptBuilder`.

**Personality stack** — three-stage pipeline: `IConversationStateUpdater` → `ISystemPromptBuilder` → `IResponsePostProcessor`. See `personality.md`.

**Project** (capital P) — Phase 8 aggregate; per-project ownership of conversations, files, persona overrides. The `IToolExecutionContext` carries the active project id per request.

**ProjectFile** — file uploaded to a `Project`'s storage. Accessed via `list_project_files` / `read_project_file` tools and `IProjectFileService`.

**Regenerate** — request a new alternative reply for an existing assistant turn. Old turn deactivates; new turn inherits the same `VariantGroupId`. See `variants.md`.

**Self-docs** — Gabriel's own documentation as accessed by the `docs_*` tools. Primary store: `docs/gabriel-self-docs/`. Fallback: `docs/Gabriel.Engine/` on GitHub.

**SSE** — Server-Sent Events. Wire format for streaming `AgentEvent`s: `data: {json}\n\n` frames.

**SSRF defense** — pre-fetch validation in `web_fetch` rejecting URLs that resolve to private / loopback / link-local addresses.

**System prompt** — built fresh per iteration by `ISystemPromptBuilder.Build(state)` and prepended to history. Static persona + dynamic block + few-shot.

**Task mode** — state where `UserAskedForDetail == true`. System prompt swaps to "deliver the full artifact" rules; length-bucket guidance is short-circuited.

**Token estimator** — `NaiveTokenEstimator`: `⌈chars / 4⌉ + 8` per message. Behind `ITokenEstimator` for future BPE swap-in.

**Tool descriptor** — `(Name, Description, ParametersJsonSchema)` projected from `ITool` by `IToolRegistry.AsDescriptors()`. The wire shape the model sees.

**Tool execution context** — `IToolExecutionContext`. Scoped per request, populated by `AgentService` at turn start. Project-scoped tools read from it (e.g., to know which project the call is for).

**Truncate-from-here** — delete operation that anchors on the variant group's earliest sibling, then removes everything from that anchor's `CreatedAt` onward.

**Variant group** — set of `Message` rows sharing a `VariantGroupId`. Singleton by default; populated by regenerate. Exactly one set of siblings is active at any time.

**VariantGroupId** — `Guid` on `Message`. Defaults to the message's own `Id` at creation. Regen inherits the original assistant's `VariantGroupId`.

## SEE ALSO

- `architecture.md` — where the types named here live.
- `agent-loop.md`, `tools.md`, `personality.md`, `sequence.md`, `variants.md` — context for each term.
