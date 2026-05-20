# Conversations API

The HTTP surface the webapp uses to read, mutate, and stream chat conversations. Everything here lives under [src/api/Gabriel.API/Controllers/ConversationsController.cs](../src/api/Gabriel.API/Controllers/ConversationsController.cs), with one detour to [MemoriesController.cs](../src/api/Gabriel.API/Controllers/MemoriesController.cs) for the memory entries the agent reads each turn. All routes are mounted under a global `/api` prefix that's added by [GlobalRoutePrefixConvention](../src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs), so the controller-declared `Route("conversations")` becomes `/api/conversations` on the wire.

## Authentication

Every endpoint on `ConversationsController` carries `[Authorize]`. The webapp authenticates by POSTing to `/api/auth/login`, which sets two `HttpOnly` cookies (access + refresh). A custom `JwtBearer.OnMessageReceived` handler reads the access cookie back on every request, so the browser never has to attach an `Authorization` header. Calls from outside the browser can still send `Authorization: Bearer <jwt>` directly — the same handler accepts either source.

A request from an unauthenticated principal returns `401`; a request from a principal that doesn't own the target conversation returns `404` (we never reveal the existence of another user's row). Conversation lookups raising a `NotFoundException` happen **before** the SSE stream opens, so the stream endpoints surface those as plain HTTP `404`s, not as in-stream `error` frames.

JSON in and out uses the System.Text.Json `JsonSerializerDefaults.Web` profile, so wire fields are camelCased even though the C# DTOs are PascalCase.

## Listing and fetching conversations

`GET /api/conversations?projectId={guid}` returns the user's conversations as an array of `ConversationResponse` rows. The `projectId` query parameter is optional: omit it for the "all my chats" sidebar view, pass a project Id to scope the list to that project. List rows have `messages: null` (the sidebar doesn't render them) and skip the parent-project lookup to avoid an N+1 over a long list — `projectIsDefault` and `effectiveAvatarSeed` are therefore `null` on list rows too.

`GET /api/conversations/{id}` returns a single conversation with the full ordered `messages` array populated, plus the project context (`projectIsDefault`, `effectiveAvatarSeed`) needed to render the avatar correctly.

The response shape is the same record either way:

```jsonc
{
  "id": "guid",
  "projectId": "guid | null",      // null for legacy pre-Phase-8 rows; otherwise the parent project
  "title": "string",
  "avatarSeed": 0,                  // long; the conversation's own seed
  "createdAt": "ISO-8601 datetime-offset",
  "updatedAt": "ISO-8601 datetime-offset",
  "messages": [/* MessageResponse */] | null, // populated only by GET /{id}
  "projectIsDefault": true | false | null,    // null = legacy row
  "effectiveAvatarSeed": 0 | null,  // project's seed in real projects; conversation seed in Default; null legacy
  "patternOverride": "string | null",  // pinned skin (Default-project chats only)
  "paletteOverride": "string | null",
  "mode": "chatty | elaborative | concise | tutor | critic | null"
}
```

`effectiveAvatarSeed` is what the frontend should feed the Gabriel-sequence renderer. Non-default projects override the per-conversation seed so every chat in a project shares one avatar; standalone chats (the auto-created Default project) fall back to their own `avatarSeed`.

## Creating, renaming, deleting

`POST /api/conversations` creates a new conversation. The body is `CreateConversationRequest`:

```jsonc
{ "title": "string | null", "projectId": "guid | null" }
```

If `projectId` is omitted, the conversation lands in the user's auto-created Default project (the "standalone bucket"). The response is `ConversationResponse` with no messages, returned as `201 Created`.

`PATCH /api/conversations/{id}` renames a conversation. Body is `UpdateConversationRequest` (`{ "title": "string" }`) and the response echoes the full `ConversationResponse` so the sidebar can update in place. Empty titles are rejected by the service layer.

`DELETE /api/conversations/{id}` permanently removes the conversation and all its messages. Returns `204 No Content`. There is no soft-delete / undo.

## Avatar, skin, mode

A conversation carries three knobs that affect how its avatar and assistant behave.

`POST /api/conversations/{id}/avatar/reroll` regenerates a fresh random `avatarSeed` for the conversation and returns the updated `ConversationResponse`. For chats inside a non-default project this is a no-op visually — the project's seed still wins via `effectiveAvatarSeed` — but the conversation seed is preserved so a future "promote to project" flow can carry it forward.

`PUT /api/conversations/{id}/skin` pins the avatar's pattern and palette for **standalone** (Default-project) chats. Body is `SetSkinRequest`:

```jsonc
{ "pattern": "string | null", "palette": "string | null" }
```

Both fields are validated against the engine's `SequenceCatalog` — unknown names return `400`. Passing `null` clears that override and falls back to seed-driven selection. Real-project chats ignore these fields when rendering (the project's own skin wins), but the values are still echoed back so the UI can show the pinned choice.

`PUT /api/conversations/{id}/mode` sets a per-conversation behaviour bias. Body is `SetConversationModeRequest`:

```jsonc
{ "mode": "chatty | elaborative | concise | tutor | critic | null" }
```

`null` clears the bias and the agent falls back to its default (`chatty`). The value is sent and stored as the lowercased enum name; anything else returns `400`.

## Avatar visualisation and context window

`GET /api/conversations/{id}/sequence` returns the 64-frame, 16×16 pixel-art sequence that drives the on-screen Gabriel avatar. It's computed from `effectiveAvatarSeed` plus the conversation's current `ConversationState` (mood, length EMA, topic flags) — see [Gabriel.Engine/gabriel-sequence.md](Gabriel.Engine/gabriel-sequence.md) for the math. Response shape:

```jsonc
{
  "version": 1,
  "palette": [[r, g, b], ...],   // RGB tuples
  "frames":  [[ix, ix, ...], ...], // 64 arrays of 256 palette-index bytes each
  "metadata": {
    "seed": 0,
    "generatedAt": "ISO-8601",
    "stateSummary": "string | null"   // human-readable mood/length flags
  }
}
```

`GET /api/conversations/{id}/metrics` returns the token-usage breakdown the context-window strip and the auto-compact predictor rely on. The response is `ContextMetricsResponse`:

```jsonc
{
  "currentTokens": 0,             // total prompt tokens the agent would send right now
  "contextWindowTokens": 0,       // provider window for the active model
  "compactThresholdTokens": 0,    // currentTokens crossing this triggers a rolling summary
  "compactThresholdRatio": 0.0,   // 0..1, the configured fraction of the window
  "messagesAfterCut": 0,          // how many messages would survive a compact right now
  "isSummarized": false,          // true if a rolling-summary system message is already in scope
  "systemPromptTokens": 0,
  "projectPromptTokens": 0,
  "memoryTokens": 0,
  "summaryTokens": 0,
  "toolsTokens": 0,
  "conversationTokens": 0
}
```

The six per-category fields sum (within rounding) to `currentTokens`, and that's what the breakdown grid in the UI renders cell-by-cell. See [Gabriel.Engine/agent-loop.md](Gabriel.Engine/agent-loop.md) for the compact triggering rules.

## Sending a message (SSE)

`POST /api/conversations/{id}/messages/stream` is the hot path. The body is `SendMessageRequest`:

```jsonc
{ "content": "string" }
```

The response is `text/event-stream`. The controller sets `Cache-Control: no-cache` and `X-Accel-Buffering: no` so reverse proxies don't hold deltas. The lookup happens before the stream opens — a missing conversation is a clean `404`, not an in-stream error.

Once the stream is open, the controller forwards events from `IAgentService.RunAsync` as `data: <json>\n\n` SSE frames. Each JSON object carries a `type` discriminator (see [AgentEvent.cs](../src/api/Gabriel.Engine/Services/AgentEvent.cs)), and the controller applies a human-typing pacing simulation on top: a randomised "thinking" delay before the first `textDelta`, then a per-character throttle so deltas reach the client at roughly chars-per-second instead of as-fast-as-the-model-emits. Non-text events (tool calls, results, the final `assistantMessage`, errors, done) bypass the throttle.

The event union, in the order a typical turn emits them:

| `type` | Fields | When |
| --- | --- | --- |
| `userMessagePersisted` | `messageId` | Always first. Lets the client swap its optimistic `tmp-xxx` user-message id for the real DB id without a follow-up round-trip. |
| `compactStart` | `messageCount`, `currentTokens`, `thresholdTokens` | Only when the user's turn would cross the compact threshold — the UI shows a "compacting…" overlay while the rolling summary is generated. |
| `compactDone` | `messageCount`, `summaryTokens` | Pairs with `compactStart`. Skipped if the summary call fails (the agent falls back to truncation, which the UI sees as a long thinking phase with no compact pair). |
| `reasoningDelta` | `delta` | Provider chain-of-thought tokens (Grok 4 `reasoning_content`, DeepSeek-R1, OpenAI o-series). Accumulated client-side into the reasoning panel. Missing entirely for providers that don't expose a reasoning channel. |
| `textDelta` | `delta` | Assistant text tokens. Concatenate to render the streaming bubble. Subject to the typewriter throttle. |
| `toolCall` | `messageId`, `toolCallId`, `name`, `argumentsJson` | The model requested a tool. The assistant message with `messageId` already exists in the DB; `toolCallId` ties the future `toolResult` back to this call. `argumentsJson` is a raw JSON string (not a parsed object) — the schema varies per tool. |
| `toolResult` | `messageId`, `toolCallId`, `content` | Tool finished; its observation is its own tool-role message at `messageId`. |
| `assistantMessage` | `messageId`, `content`, `reasoningContent` | Final reconciled assistant turn. `content` is the post-processed text (AI-ism strip + length cap applied); use it as the canonical message body and discard the delta-built draft. `reasoningContent` carries the accumulated reasoning for the same turn, or `null` if no reasoning channel was active. |
| `error` | `message` | Mid-stream failure. Headers are already sent at this point, so a `5xx` isn't possible — the agent emits this frame instead and closes the stream. |
| `done` | _(none)_ | Terminal event. The stream closes immediately after. Clients should not assume any further frames will arrive. |

ReAct iterations interleave these: a single turn can emit one or more `textDelta` runs, then `toolCall` + `toolResult` pairs, then another `textDelta` run before the final `assistantMessage`. The agent loop bounds iterations at `Agent.MaxIterations` (default 8) — beyond that, the run terminates with whatever text it has, then `done`.

## Regenerating, deleting, and navigating message variants

Conversations support "regenerate this assistant turn", which produces a sibling message that shares a `variantGroupId` with the original. The UI exposes a `< 2/3 >` picker on any message that has siblings.

`POST /api/conversations/{id}/messages/{messageId}/regenerate` re-runs the agent on the user turn that preceded `messageId` and streams the new variant back via the same SSE event union as `/messages/stream` — minus the `userMessagePersisted` event (no new user message is being persisted; the prior user turn is reused). The new assistant message inherits `variantGroupId` from the original, and the original is marked inactive so the freshly-streamed variant becomes the active sibling.

`PATCH /api/conversations/{id}/messages/{messageId}/active` switches which sibling in a variant group is the active one. The response is the full `ConversationResponse` *with* the `messages` array, so the client can re-render the chat with the chosen variant (and the new history-filter view that goes with it). The agent only ever sees the active sibling — inactive variants stay in the DB but are filtered out of the prompt.

`DELETE /api/conversations/{id}/messages/{messageId}` deletes a message and **all subsequent messages** in the conversation. The service layer anchors on the earliest sibling in the variant group, so deleting one regenerated reply also cleans up the rest of the regenerated turn and everything that came after. Returns `ConversationResponse` without the messages array (the client re-renders against its remaining local state, or refetches). See [Gabriel.Engine/variants-and-history.md](Gabriel.Engine/variants-and-history.md) for the data-model rules these endpoints enforce.

### Message shape

The `messages` array on a single-conversation fetch contains `MessageResponse` rows:

```jsonc
{
  "id": "guid",
  "role": "user | assistant | system | tool",
  "content": "string | null",        // null is valid for assistant turns that only requested tools
  "createdAt": "ISO-8601",
  "variantGroupId": "guid",          // shared across regen siblings; equals id for non-regenerated messages
  "variantIndex": 0,                 // 0-based position among siblings, ordered by createdAt
  "variantCount": 1,                 // total siblings; 1 means "no other variants"
  "variantSiblingIds": ["guid", ...], // every sibling id, including this one, in variantIndex order
  "toolCallId": "string | null",     // set on tool-role messages; ties back to the assistant's tool_call
  "toolCalls": [                     // set on assistant messages that requested tools
    { "id": "string", "name": "string", "argumentsJson": "raw JSON string" }
  ] | null,
  "reasoningContent": "string | null" // provider chain-of-thought for the turn; null if the channel was inactive
}
```

`variantSiblingIds` is what the `< N/M >` picker uses to drive `PATCH .../active` without re-fetching: the index N is `variantIndex + 1`, M is `variantCount`, and switching variants is a one-call PATCH plus a state swap.

## Memory entries (cross-cutting)

Memory is technically `/api/memories/*`, not `/api/conversations/*`, but it's a conversation-adjacent surface because the agent reads memories into the system prompt every turn and the chat UI exposes a "Remember this" action that writes here.

`GET /api/memories?scope=all&projectId={guid}` returns the memories the agent will see for a given context. Pass `scope=all` to get the merged user-scope + project-scope view (what the agent actually consumes); omit `scope` to get only entries that match the given `projectId` (or only user-scope entries when `projectId` is omitted).

`POST /api/memories` is an upsert keyed by `(userId, projectId, name)`. Body:

```jsonc
{
  "projectId": "guid | null",        // null = user-scope, visible in every conversation
  "type": "user | feedback | project | reference",
  "name": "kebab-case-string",       // unique per scope; collisions overwrite
  "description": "one-line summary",
  "body": "full memory body"
}
```

Returns the saved `MemoryDto`. The agent's own `memory_save` tool routes through this same path, so a memory captured via tool and one captured via the "Remember this" UI are indistinguishable in storage. `DELETE /api/memories/{id}` returns `204` and is a hard delete.

```jsonc
// MemoryDto (response shape)
{
  "id": "guid",
  "projectId": "guid | null",
  "type": "user | feedback | project | reference",
  "name": "string",
  "description": "string",
  "body": "string",
  "createdAt": "ISO-8601",
  "updatedAt": "ISO-8601"
}
```

## Error model

The controllers don't define their own problem-details middleware; failures bubble through the global exception filter that maps domain exceptions to status codes (`NotFoundException` → `404`, `ValidationException` → `400`, anything else → `500` with a sanitized `error` body). For SSE endpoints, the lookup that loads the conversation runs synchronously before the response stream opens, so 404s and authorization failures still surface as normal HTTP responses. Failures that happen mid-stream — provider errors, tool exceptions — surface as a single `error` SSE event followed by stream closure; the HTTP status stays `200` because headers were already flushed.

## Endpoint reference

| Method | Route | Body | Response | Streams |
| --- | --- | --- | --- | --- |
| `GET` | `/api/conversations?projectId=` | — | `ConversationResponse[]` (no messages) | no |
| `GET` | `/api/conversations/{id}` | — | `ConversationResponse` (with messages) | no |
| `POST` | `/api/conversations` | `CreateConversationRequest` | `ConversationResponse` (`201`) | no |
| `PATCH` | `/api/conversations/{id}` | `UpdateConversationRequest` | `ConversationResponse` | no |
| `DELETE` | `/api/conversations/{id}` | — | `204` | no |
| `POST` | `/api/conversations/{id}/avatar/reroll` | — | `ConversationResponse` | no |
| `PUT` | `/api/conversations/{id}/skin` | `SetSkinRequest` | `ConversationResponse` | no |
| `PUT` | `/api/conversations/{id}/mode` | `SetConversationModeRequest` | `ConversationResponse` | no |
| `GET` | `/api/conversations/{id}/sequence` | — | `GabrielSequenceResponse` | no |
| `GET` | `/api/conversations/{id}/metrics` | — | `ContextMetricsResponse` | no |
| `POST` | `/api/conversations/{id}/messages/stream` | `SendMessageRequest` | `text/event-stream` of `AgentEvent` | yes |
| `POST` | `/api/conversations/{id}/messages/{messageId}/regenerate` | — | `text/event-stream` of `AgentEvent` | yes |
| `PATCH` | `/api/conversations/{id}/messages/{messageId}/active` | — | `ConversationResponse` (with messages) | no |
| `DELETE` | `/api/conversations/{id}/messages/{messageId}` | — | `ConversationResponse` (no messages) | no |
| `GET` | `/api/memories?scope=&projectId=` | — | `MemoryDto[]` | no |
| `POST` | `/api/memories` | `SaveMemoryRequest` | `MemoryDto` | no |
| `DELETE` | `/api/memories/{id}` | — | `204` | no |
