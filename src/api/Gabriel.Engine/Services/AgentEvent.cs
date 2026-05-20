using System.Text.Json.Serialization;

namespace Gabriel.Engine.Services;

// Events yielded by AgentService.RunAsync - also the SSE wire format. Polymorphic
// JSON discriminator is "type", so clients switch on that string.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AgentUserMessagePersisted), "userMessagePersisted")]
[JsonDerivedType(typeof(AgentTextDelta),        "textDelta")]
[JsonDerivedType(typeof(AgentReasoningDelta),   "reasoningDelta")]
[JsonDerivedType(typeof(AgentToolCall),         "toolCall")]
[JsonDerivedType(typeof(AgentToolResult),       "toolResult")]
[JsonDerivedType(typeof(AgentAssistantMessage), "assistantMessage")]
[JsonDerivedType(typeof(AgentCompactStart),     "compactStart")]
[JsonDerivedType(typeof(AgentCompactDone),      "compactDone")]
[JsonDerivedType(typeof(AgentError),            "error")]
[JsonDerivedType(typeof(AgentDone),             "done")]
public abstract record AgentEvent;

// First event of every turn that originated from a user message (RunAsync,
// not RegenerateAsync) - tells the client the real DB id of the user message
// the server just persisted. Lets the client swap its temporary tmp-xxxxx id
// in the user entry for the real id in place, without a follow-up
// "GET conversation" round-trip after the stream ends.
public sealed record AgentUserMessagePersisted(Guid MessageId) : AgentEvent;

// Incremental assistant-text token. Clients concatenate to build the current message.
public sealed record AgentTextDelta(string Delta) : AgentEvent;

// Incremental "thinking" token - the model's chain-of-thought stream. Surfaced by
// reasoning-capable providers (Grok 4 reasoning_content, DeepSeek-R1, etc.). The UI
// can render this in a separate panel; the final assistant message carries the
// accumulated reasoning alongside its content for persistence.
public sealed record AgentReasoningDelta(string Delta) : AgentEvent;

// Assistant requested a tool. The persisted assistant Message is referenced by MessageId.
public sealed record AgentToolCall(Guid MessageId, string ToolCallId, string Name, string ArgumentsJson) : AgentEvent;

// Tool finished; observation is persisted as a separate Message with MessageId.
public sealed record AgentToolResult(Guid MessageId, string ToolCallId, string Content) : AgentEvent;

// Final assistant text message is persisted. Carries the accumulated text so
// clients can reconcile their delta-built view against the canonical content.
// ReasoningContent is the accumulated chain-of-thought for the same turn,
// null when the provider didn't emit a reasoning channel.
public sealed record AgentAssistantMessage(Guid MessageId, string Content, string? ReasoningContent = null) : AgentEvent;

// Compaction is about to start: a rolling-summary LLM call will fold the first
// `MessageCount` messages into a single summary. Emitted before the summary
// provider call so the UI can show a "compacting…" overlay while the user
// waits for the real turn to start. `CurrentTokens` is the pre-compact total;
// `ThresholdTokens` is the trigger line we just crossed.
public sealed record AgentCompactStart(int MessageCount, int CurrentTokens, int ThresholdTokens) : AgentEvent;

// Compaction finished. `SummaryTokens` is the size of the new rolling summary;
// `MessageCount` mirrors AgentCompactStart so the UI can render a "summarized
// N messages into M tokens" line. Always paired with a preceding
// AgentCompactStart - skipped entirely when the summary call fails (the UI
// then sees a long thinking phase but no compact pair, which is fine).
public sealed record AgentCompactDone(int MessageCount, int SummaryTokens) : AgentEvent;

// In-stream error (lookup failures throw before streaming starts and surface as HTTP 4xx/5xx).
public sealed record AgentError(string Message) : AgentEvent;

// Terminal event - the loop has finished and the SSE stream will close.
public sealed record AgentDone() : AgentEvent;
