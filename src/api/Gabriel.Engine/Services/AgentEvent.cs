using System.Text.Json.Serialization;

namespace Gabriel.Engine.Services;

// Events yielded by AgentService.RunAsync — also the SSE wire format. Polymorphic
// JSON discriminator is "type", so clients switch on that string.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AgentTextDelta),        "textDelta")]
[JsonDerivedType(typeof(AgentToolCall),         "toolCall")]
[JsonDerivedType(typeof(AgentToolResult),       "toolResult")]
[JsonDerivedType(typeof(AgentAssistantMessage), "assistantMessage")]
[JsonDerivedType(typeof(AgentError),            "error")]
[JsonDerivedType(typeof(AgentDone),             "done")]
public abstract record AgentEvent;

// Incremental assistant-text token. Clients concatenate to build the current message.
public sealed record AgentTextDelta(string Delta) : AgentEvent;

// Assistant requested a tool. The persisted assistant Message is referenced by MessageId.
public sealed record AgentToolCall(Guid MessageId, string ToolCallId, string Name, string ArgumentsJson) : AgentEvent;

// Tool finished; observation is persisted as a separate Message with MessageId.
public sealed record AgentToolResult(Guid MessageId, string ToolCallId, string Content) : AgentEvent;

// Final assistant text message is persisted. Carries the accumulated text so
// clients can reconcile their delta-built view against the canonical content.
public sealed record AgentAssistantMessage(Guid MessageId, string Content) : AgentEvent;

// In-stream error (lookup failures throw before streaming starts and surface as HTTP 4xx/5xx).
public sealed record AgentError(string Message) : AgentEvent;

// Terminal event — the loop has finished and the SSE stream will close.
public sealed record AgentDone() : AgentEvent;
