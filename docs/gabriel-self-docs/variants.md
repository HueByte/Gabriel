# Variants and history

## PURPOSE
How regenerate, delete, and the variant picker work at the data, agent-loop, and API levels.

## USE WHEN
- User asks how regenerate works.
- User asks how to switch between alternative replies (variants).
- User asks why a deleted message took other messages with it.
- User asks why the model doesn't "see" an older reply that's still in the chat history.
- You need to reason about `VariantGroupId` / `IsActiveVariant`.

## QUICK REFERENCE

| Concept | Definition |
| --- | --- |
| `VariantGroupId` | `Guid` shared by every message in a regen sibling set. Defaults to the message's own `Id` (singleton group). |
| `IsActiveVariant` | `bool`, default `true`. Exactly one set of siblings within a group is active at any time. |
| Regen action | Old turn flips to `IsActiveVariant = false`; new turn carries the same `VariantGroupId`, `IsActiveVariant = true`. |
| Delete action | `TruncateFrom(messageId)` — anchors on the variant group's earliest sibling, then removes everything from that anchor's `CreatedAt` onward. |
| History filter | `ToProviderHistory` skips inactive non-tool messages AND tool messages whose `ToolCallId` isn't referenced by an active assistant. |

## DETAILS

### Variant group model

```csharp
// Message
public Guid VariantGroupId { get; private set; }   // defaults to Id at creation
public bool IsActiveVariant { get; private set; } = true;
```

- Singleton message → `VariantGroupId == Id`, `IsActiveVariant == true`.
- Regenerated turn → all new messages inherit the **original assistant's** `VariantGroupId`; old turn deactivates.

A "turn" inside a variant group is usually multi-message (assistant + tool_calls + tool results + final assistant text). They all share the group id and they all flip active/inactive together.

### Regenerate flow

`IAgentService.RegenerateAsync(convId, assistantMessageId)`:

1. Load conversation user-scoped.
2. Locate target. Validate: assistant role AND `IsActiveVariant`.
3. `conversation.DeactivateVariantGroup(target.VariantGroupId)` → flips every member.
4. Save.
5. `MaybeCompactAsync` (standard between-turn step).
6. Run regular ReAct loop with `variantGroupIdOverride = target.VariantGroupId`. Every `AppendAssistantText` / `AppendAssistantToolCalls` / `AppendToolResult` passes the override into `Message.Create`.

When the stream finishes, the group has ≥ 2 active+inactive turn-sequences sharing one id.

`RegenerateAsync` does NOT call `IConversationStateUpdater` — the state already reflects the user message being re-answered.

### Truncate-from-here (delete)

`IChatService.DeleteMessageAsync(convId, messageId)`:

1. Load conversation user-scoped.
2. Validate target exists.
3. `conversation.TruncateFrom(messageId)` returns removed messages.
4. `_conversations.RemoveMessages(removed)` (explicit EF removal; orphan-removal alone is fragile across EF Core versions).
5. Save.

`TruncateFrom` does NOT cut from the target onward — it first anchors on the **variant group's earliest sibling** by `CreatedAt`, then deletes from that anchor onward.

```csharp
public IReadOnlyList<Message> TruncateFrom(Guid messageId)
{
    var target = _messages.First(m => m.Id == messageId);
    var anchor = _messages
        .Where(m => m.VariantGroupId == target.VariantGroupId)
        .OrderBy(m => m.CreatedAt)
        .First();
    var toRemove = _messages.Where(m => m.CreatedAt >= anchor.CreatedAt).ToList();
    foreach (var m in toRemove) _messages.Remove(m);
    return toRemove;
}
```

**Why anchor on earliest sibling**: if the user deletes a regenerated variant that's currently active, naive "delete from this message onward" would leave inactive siblings orphaned with no active member — the conversation would show a blank turn at that position. Anchoring removes the whole turn cleanly.

For non-assistant messages (user, system, tool), `VariantGroupId == Id` so anchor = self. Truncation is a straight chronological cut.

### History filtering (`AgentService.ToProviderHistory`)

```csharp
// Collect tool_call.ids referenced by active assistant messages.
var activeToolCallIds = new HashSet<string>(StringComparer.Ordinal);
foreach (var m in messages)
{
    if (m.Role != MessageRole.Assistant || !m.IsActiveVariant) continue;
    if (string.IsNullOrEmpty(m.ToolCallsJson)) continue;
    foreach (var el in JsonDocument.Parse(m.ToolCallsJson).RootElement.EnumerateArray())
        activeToolCallIds.Add(el.GetProperty("id").GetString()!);
}

for (var i = startIdx; i < messages.Count; i++)
{
    var m = messages[i];
    if (m.Role == MessageRole.Tool)
    {
        if (m.ToolCallId is null || !activeToolCallIds.Contains(m.ToolCallId)) continue;
    }
    else if (!m.IsActiveVariant) continue;
    // ... include in provider history
}
```

Two filters, both necessary:
- `IsActiveVariant` excludes deactivated assistant/user/system messages directly.
- Tool-call-id linkage excludes tool messages whose parent assistant is gone (deactivated by regen or pre-variant legacy data). Without this, the model would see a tool result with no preceding `tool_calls` declaration and the provider wire-format invariant breaks.

The `Conversation` keeps ALL messages — nothing is destroyed unless explicitly deleted. The model just never sees inactive ones.

### Variant metadata in API responses

`GET /api/conversations/{id}` returns `MessageResponse` with:

```ts
{
  variantGroupId: string;       // shared across regen siblings
  variantIndex: number;         // 0-based position in group (by CreatedAt)
  variantCount: number;         // total siblings in this group
  variantSiblingIds: string[];  // ALL sibling ids in group, CreatedAt-sorted, includes self
}
```

Singletons (the common case): `variantCount == 1`, `variantIndex == 0`, `variantSiblingIds == [self.id]`. UI hides the picker.

Regenerated turns: `variantCount >= 2`. UI shows `< {variantIndex+1}/{variantCount} >` chrome; switching navigates by `variantSiblingIds` via `PATCH /active`.

The response **only includes active variants** and tool aftermath of active assistants — inactive siblings are NOT in the messages array. The client reaches them via the sibling ids list.

`ContractMappings.ToResponse` builds variant metadata across all messages, then per-message looks up its group siblings.

### API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `DELETE` | `/api/conversations/{convId}/messages/{messageId}` | Truncate-from-here. Returns conversation without `messages`. |
| `PATCH` | `/api/conversations/{convId}/messages/{messageId}/active` | Switch active variant within the group. Empty body. Returns conversation with new active `messages`. |
| `POST` | `/api/conversations/{convId}/messages/{messageId}/regenerate` | SSE; same wire format as `/messages/stream`. New reply inherits original's `VariantGroupId`. |

### Switch active variant

```csharp
public void SetActiveVariant(Guid messageId)
{
    var target = _messages.First(m => m.Id == messageId);
    foreach (var m in _messages.Where(m => m.VariantGroupId == target.VariantGroupId))
    {
        if (m.Id == target.Id) m.MarkActiveVariant();
        else m.MarkInactiveVariant();
    }
}
```

Within the group, flip target on, everyone else off. For multi-message variants, calling `SetActiveVariant` with ANY one message of the turn activates the entire turn (they share `VariantGroupId`).

### Compact interaction

The rolling compact cuts on User-role boundaries. Regen variant groups are always assistant + tool messages (never user), so the cut always lands BEFORE a variant group. A variant group never gets half-summarized.

When a variant group falls entirely pre-cut, the summary captures the active variant's content. Inactive siblings of summarized turns remain in the DB but become invisible.

## INVARIANTS

- Exactly one set of siblings within a `VariantGroupId` is active at any time.
- A new (non-regenerated) message has `VariantGroupId == Id`, `IsActiveVariant == true`.
- Regenerate inherits the **original assistant's** `VariantGroupId`, never the user's.
- `TruncateFrom` always anchors on the group's earliest sibling.
- Tool messages in history must reference a tool_call id of an active assistant.
- The API response array hides inactive variants; only the sibling ids list exposes them.

## PITFALLS

- "Why did deleting one assistant reply remove the others?" — they shared a `VariantGroupId` and the truncate anchored on the earliest sibling.
- "Why doesn't the model see the variant I just switched away from?" — `IsActiveVariant = false` filters it out of `ToProviderHistory`; the message still exists in the DB.
- Don't regenerate a user message — only assistant messages are regenerable. Current UX for "edit user message" is delete + retype.
- `PATCH /active` returns the **full** conversation; for very long conversations this could become large.

## SEE ALSO

- `agent-loop.md` — where `variantGroupIdOverride` flows through `RunStreamAsync`.
- Human-prose companion: `Gabriel.Engine/variants-and-history.md`.
