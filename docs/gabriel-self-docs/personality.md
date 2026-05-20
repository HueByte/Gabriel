# Personality stack

## PURPOSE
The three-stage pipeline that shapes a reply's tone and length: `IConversationStateUpdater` → `ISystemPromptBuilder` → `IResponsePostProcessor`.

## USE WHEN
- User asks how Gabriel decides to be casual vs deliver code.
- User asks how mood is detected.
- User asks why a reply was short / long / scrubbed.
- User asks about "task mode" or `UserAskedForDetail`.
- User asks how to change the persona.

## QUICK REFERENCE

| Stage | Interface | Default impl | Lifetime | Pure? |
| --- | --- | --- | --- | --- |
| State update | `IConversationStateUpdater` | `HeuristicConversationStateUpdater` | Singleton | Yes (no LLM call) |
| Prompt build | `ISystemPromptBuilder` | `GabrielSystemPromptBuilder` | Singleton | Yes |
| Post-process | `IResponsePostProcessor` | `ResponsePostProcessor` | Singleton | Yes |

State lives on `Conversation.StateJson` as serialized `ConversationState` (a domain value object in `Gabriel.Core.Personality`).

## DETAILS

### ConversationState (domain value object)

```csharp
public sealed record ConversationState
{
    int TurnCount;
    Mood Mood;                              // Neutral | Playful | Venting | Serious | Curious | LowEnergy
    float AvgUserTokenCount;                // EMA across user messages
    int LastUserTokenCount;
    IReadOnlyList<string> RecentTopics;
    DateTimeOffset LastMessageAt;
    int ConsecutiveShortMessages;
    bool UserUsesEmoji;
    bool UserUsesLowercase;
    bool UserAskedForDetail;                // task-mode flag
}
```

### HeuristicConversationStateUpdater

Zero LLM cost. All decisions are regex / arithmetic.

**Token EMA**:
```
μ_t = 0.7 · μ_{t-1} + 0.3 · c_t        (0.7/0.3 ≈ 3-message half-life)
```

**Mood classification** — first matching rule wins, priority order:

| Mood | Trigger |
| --- | --- |
| `Playful` | `\b(lol|lmao|haha|hahaha|rofl)\b` OR `!{2,}` OR `\bxd\b` |
| `Venting` | Negative lexicon AND `"i "` in message (first-person) |
| `Serious` | Starts with `(honestly|seriously|look|listen|ok so)\b` |
| `Curious` | `LastUserTokenCount > 100` AND contains `?` |
| `LowEnergy` | `LastUserTokenCount < 5` AND no punctuation |
| `Neutral` | Fallback |

**Task-mode (`UserAskedForDetail`)** — fires on either:
- Task verbs: `explain | tell me about | how does | what is | walk me through | describe | write | implement | build | code | create | generate | draft | compose | design | produce | refactor | fix | debug | do it | go ahead | send it | just do it | make it | make me | give me | show me | help me with`
- Implicit-request suffix: `\w+\s+(please|pls)\s*\??\s*$`

Other signals:
- `RecentTopics`: top 3 tokens of length ≥ 5 not in stop-word list, sorted by length desc, merged with previous 5.
- `ConsecutiveShortMessages`: ++ when `LastUserTokenCount < 10`, reset otherwise.
- `UserUsesEmoji`: sticky-once-true; checks high-surrogate chars or U+2600..U+27BF.
- `UserUsesLowercase`: first trimmed char is lowercase.

### GabrielSystemPromptBuilder

Per-turn prompt = **static persona** ++ **dynamic block** ++ **few-shot block**.

Static persona has two modes; the prompt leads with **TASK MODE** rules because that's the failure-prone case.

**TASK MODE** (when `UserAskedForDetail`):
- Deliver the full artifact. Length-matching does NOT apply.
- Open with the artifact itself, no preamble.
- Reply < 30 words = failure; restart.
- Repeated "do it" / "write it" = stalling; produce output now.

**CHAT MODE**:
- Match user's general weight (short → short).
- Every reply brings substance (a take, a callback, a reaction).
- 2-3 word replies acceptable only for pure-noise inputs (`lol`, `k`, `fair`).

**Hard prohibitions (both modes)**: no "great question" / "absolutely" / "I'd be happy to help"; no rephrasing the user; no "feel free to ask" closers; no unsolicited emoji.

**Dynamic block** (appended each turn):
```
[Conversation metadata]
Turn: {TurnCount}
User's last message length: ~{LastUserTokenCount} tokens
Conversation mood: {Mood, lowercased}
User uses emoji - light mirroring is allowed.            ; if UserUsesEmoji
User writes in lowercase - match.                         ; if UserUsesLowercase
Recent messages have been very short - don't force engagement.  ; if ConsecutiveShortMessages >= 2
User is in TASK MODE - they want a substantive artifact.  ; if UserAskedForDetail
⚠ STALL WARNING: ...                                       ; if task mode + ConsecutiveShortMessages >= 1

[Guidance]
{LengthGuidance(state)}
{MoodGuidance(state.Mood)}
```

**Length guidance** (bucketed; task-mode short-circuits all of these):

| `LastUserTokenCount` | Guidance |
| --- | --- |
| ≤ 5 | Mirror if pure-noise, else ONE punchy sentence with a hook |
| 6..20 | 1-3 sentences with substance |
| 21..60 | 3-5 sentences |
| 61..150 | Match depth; short paragraph |
| > 150 | Substantive; cap ~250 words |

**Mood guidance** (one-liner each, all push toward engagement):

| Mood | Shape |
| --- | --- |
| `Playful` | Keep it light, banter — but bring an angle. |
| `Venting` | Listen more than advise; validate; genuine warmth. |
| `Serious` | Direct, thoughtful, substantive. |
| `Curious` | Engage with the idea, add a take, ask one thing if genuinely curious. |
| `LowEnergy` | Brief but make each sentence count. |
| `Neutral` | Bring an angle, a take, or a curious question — don't strip personality. |

**Few-shot block**: chat-mode examples (`lol → lol`, `how's it going → caffeinated regex fight + question`) + task-mode examples (Python string reverse, TS BFS, OAuth explainer). The task-mode examples were added to undo the chat-only prior that taught the model to never deliver code.

### ResponsePostProcessor

Runs once at save time on the full accumulated raw reply.

**Strips (anchored at start, case-insensitive)** — AI-ism openers:
- `"that's a (great|really good|fantastic|interesting) question..."`
- `"i think you'll find that..."`, `"here's what i think..."`
- `"to answer your question..."`
- `"certainly..."`, `"absolutely..."`
- `"i'd be happy to help..."`, `"let me break this down..."`
- `"here's the thing..."`, `"i appreciate you sharing..."`

**Strips (anchored at end)** — AI-ism closers:
- `"let me know if you have any questions..."`
- `"feel free to ask..."`
- `"hope (that|this) helps..."`
- `"does that make sense..."`

**Does NOT strip**: Markdown. Inline emphasis, fenced code, blockquotes — all part of allowed style.

**No length cap.** An earlier version truncated long replies at save time, but only the persisted form was truncated — the live stream had already shipped the full text. Result: replies that looked complete during the session came back "cut off" after reload. Removed; the persona prompt is the only length gate.

### How it hooks the agent loop

In `AgentService.RunAsync`:
1. `_stateUpdater.Update(state, userInput)` → `conversation.SetState(newState)` (once per user turn).
2. `_promptBuilder.Build(state)` → prepended as system message **every iteration**.
3. After `Stop`: `_postProcessor.Clean(rawText, state)` → persisted.

`RegenerateAsync` does NOT call `_stateUpdater` — existing state already reflects the user's last message.

### Configuration

Section `Personality`:

```jsonc
{
  "Personality": {
    "Name": "Gabriel",
    "MinThinkingDelayMs": 400,
    "MaxThinkingDelayMs": 1100,
    "MinCharsPerSecond": 55,
    "MaxCharsPerSecond": 85
  }
}
```

`Min/Max*` are typing-tempo knobs consumed by the SSE controller, NOT by Engine itself.

## INVARIANTS

- Task-mode short-circuits length-bucket guidance.
- Post-processor never truncates.
- State updater runs at most once per user turn.
- Mood priority is fixed: Playful > Venting > Serious > Curious > LowEnergy > Neutral.
- `RecentTopics` is tracked but **not yet injected** into the prompt.

## PITFALLS

- "Why did Gabriel give me 8 words when I asked for a function?" — the `UserAskedForDetail` regex didn't match. Check the request for one of the task verbs.
- "Why did my markdown survive?" — markdown is deliberately preserved; the post-processor only targets AI-isms.
- Mood is best-effort heuristic; mis-classification is expected and acceptable.
- The persona name in the system prompt is fixed at `PersonalityOptions.Name`. Per-project personas are a planned feature (Phase 8) but not yet shipped.

## SEE ALSO

- `agent-loop.md` — where the stack hooks into the loop.
- `sequence.md` — the Live State layer shares the same `ConversationState`.
- Human-prose companion: `Gabriel.Engine/personality-stack.md` (includes the EMA math).
