# GabrielToolBridge.cs

> **Source:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`

## Contents

- [GabrielToolBridge_overview](#gabrieltoolbridge_overview)
- [GabrielToolBridge](#gabrieltoolbridge)
- [Models](#models)
- [Name](#name)
- [AppendFixupMessage](#appendfixupmessage)
- [BuildToolDocs](#buildtooldocs)
- [EscapeJsonString](#escapejsonstring)
- [InjectToolDocs](#injecttooldocs)
- [SerializeCallInline](#serializecallinline)
- [StreamAsync](#streamasync)
- [TranslateHistory](#translatehistory)
- [MaxParseRetries](#maxparseretries)

---

## GabrielToolBridge_overview

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** class

```csharp
// "Bridges" a tool-incapable provider (a local Ollama model, a small
// commercial model without function calling) to the same IChatProvider
// surface as a native tool-calling provider. Strategy:
//
//   1. Pre-call: rewrite the message history so prior structured tool calls
//      become inline <tool_call> markers in the assistant's text, and prior
//      Tool-role results become labelled User messages (since non-native
//      protocols don't have a Tool role).
//   2. Pre-call: inject a system message at the end of the system block
//      describing the tools and the wire format.
//   3. Stream the inner provider's text deltas through ToolCallStreamSplitter:
//      live-emit deltas until "<tool_call>" is seen, then buffer the tail.
//   4. After stream completes, run ToolCallBlockParser over the buffered
//      tail. On success, synthesise ToolCallReadyEvents and finish with
//      ToolCalls. On parse failure, append a fix-up user message and retry
//      up to MaxParseRetries times.
//
// The agent loop sees the same event shape it gets from native providers,
// so RunStreamAsync / AgentContext / the UI stay unchanged. The bridge does
// NOT execute tools - that still happens server-side in
// AgentService.ExecuteToolSafelyAsync via ITool.ExecuteAsync. This class
// only bridges the wire protocol.
//
// One UX tradeoff worth flagging: live streaming happens on attempt 1 only.
// If the parse fails and we retry, the retry's text is NOT streamed (we
// can't unsend what we already yielded) and we only emit its tool calls. In
// practice the typewriter effect is preserved for the common case (single
// attempt, success) and gracefully degrades on the rare retry path.
public sealed class GabrielToolBridge : IChatProvider
{
    // Three attempts total = original + 2 retries. Matches the
    // EmptyStopMaxRetries pattern already used elsewhere in the agent loop.
    private const int MaxParseRetries = 2;

    private readonly IChatProvider _inner;
    private readonly ILogger<GabrielToolBridge> _logger;

    public GabrielToolBridge(IChatProvider inner, ILogger<GabrielToolBridge> logger)
    {
    }

    // Identity passes through - the agent registry still resolves by the
    // base provider's name. Same for Models: the decorator is wrapping a
    // specific provider that owns those entries.
    public string Name => _inner.Name;
    public IReadOnlyList<LLMModel> Models => _inner.Models;

    public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        string modelName,
        [EnumeratorCancellation] CancellationToken ct = default)
```


Bridges a non-tool-calling (or tool-incapable) chat provider so it appears to the rest of the agent stack as a native tool-calling IChatProvider. Use this when you must run a local/small/third-party model that cannot perform structured tool calls but you want the agent loop, UI and tooling behavior to remain unchanged.

## Remarks
This decorator rewrites message history and injects a tool description wire-format into the system prompt so the inner model can emit tool-call markers inline in plain text. It streams the inner provider's deltas while watching for <tool_call> markers, buffers the tail after a marker, parses tool-call blocks after the stream completes, and synthesizes the same event shapes an actual tool-capable provider would produce. It never executes tools itself — actual tool invocation is still performed server-side (e.g. AgentService.ExecuteToolSafelyAsync). The class preserves the wrapped provider's identity (Name, Models) so registry resolution and model metadata remain correct.

## Example
```csharp
// Wrap an existing chat provider so it can be used by an agent that expects
// tool-calling behavior. The bridge will perform message rewriting and
// parsing; tool execution remains the agent/service's responsibility.
IChatProvider inner = new OllamaChatProvider(...);
ILogger<GabrielToolBridge> logger = loggerFactory.CreateLogger<GabrielToolBridge>();
var bridge = new GabrielToolBridge(inner, logger);

await foreach (var evt in bridge.StreamAsync(history, tools, modelName, cancellationToken))
{
    // evt is the same event shape the agent expects from a native tool-capable provider
    HandleAgentEvent(evt);
}
```

## Notes
- Live streaming of textual deltas happens only on the first attempt; if the bridge must retry parsing the buffered tail it will not re-stream the retry's text (only the corrected tool-call events are emitted). 
- The bridge calls the inner provider with an empty tools list and instead injects tool descriptors into the system prompt; ensure the model and the injected wire-format match the bridge's parser expectations. 
- If the inner provider unexpectedly emits native tool calls, those are forwarded as-is (the decorator defers to the safest behavior rather than blocking them).

---

## GabrielToolBridge

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** constructor

Constructs a GabrielToolBridge by capturing the given IChatProvider and an ILogger specialized for GabrielToolBridge. Reach for this constructor when wiring the bridge into your composition root or creating it manually so the bridge can delegate chat operations to the provided provider and emit structured logs via the supplied logger.

## Remarks
This constructor performs only assignment of the dependencies to the instance's private fields—there is no additional initialization logic. The GabrielToolBridge acts as a thin adapter that forwards calls to the underlying IChatProvider while using the provided `ILogger<GabrielToolBridge>` for diagnostics; keeping the constructor cheap and side-effect free makes the type suitable for use with dependency injection containers and for simple manual composition.

## Example
```csharp
// Manual composition
IChatProvider provider = new SomeChatProvider(...);
ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { /* config */ });
ILogger<GabrielToolBridge> logger = loggerFactory.CreateLogger<GabrielToolBridge>();
var bridge = new GabrielToolBridge(provider, logger);

// In an ASP.NET Core DI registration (factory style)
services.AddSingleton<GabrielToolBridge>(sp =>
    new GabrielToolBridge(
        sp.GetRequiredService<IChatProvider>(),
        sp.GetRequiredService<ILogger<GabrielToolBridge>>()));
```

## Notes
- The constructor does not validate arguments; passing null for either parameter will likely cause a NullReferenceException later. Prefer using GetRequiredService in DI or validate inputs before calling.
- Construction is intentionally lightweight and has no side effects; heavy initialization should be deferred to methods called after construction.

---

## Models

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

Exposes the list of LLM models supported by this bridge by delegating directly to the underlying provider's Models collection. Use this property to enumerate or inspect the available LLMModel entries without attempting to modify the collection through the bridge.

## Remarks
This property is a thin forwarding accessor to _inner.Models — it does not copy or transform the collection. GabrielToolBridge provides this passthrough so callers can query supported models through the bridge API while keeping the authoritative model list maintained by the inner provider.

## Example
```csharp
// Enumerate available models and print their IDs/names
foreach (var model in gabrielToolBridge.Models)
{
    Console.WriteLine($"Model: {model.Id} - {model.Name}");
}
```

## Notes
- The property returns the same IReadOnlyList instance from the inner provider; mutations to the underlying collection (if the provider alters it) will be observable here.
- IReadOnlyList prevents callers from modifying the collection via this property, but it does not guarantee the underlying collection is immutable.
- If you need a stable snapshot of the models for long-running operations, copy the list (e.g., .ToList()) to avoid observing subsequent changes.

---

## Name

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

Returns the name of the wrapped provider. This bridge property simply exposes the underlying provider's Name so the bridge does not alter the provider's identity — use it whenever code needs the canonical provider name (for registry lookups, logging or diagnostics).

## Remarks
This property intentionally delegates to the inner provider (via _inner.Name) so the GabrielToolBridge remains an identity-preserving wrapper. The agent registry and any model ownership/lookup logic rely on the base provider's name rather than the bridge, so the bridge must forward the Name value instead of introducing a separate identifier.

## Notes
- The value is a live forward: reading this property returns the current value of _inner.Name, so changes to the wrapped provider's name are reflected here.
- Accessing the property will throw a NullReferenceException if _inner is null; callers should ensure the bridge has been properly initialized before reading Name.

---

## AppendFixupMessage

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Creates an augmented conversation history to use when retrying after a tool-call parse failure. It returns a new message list that comprises the original history plus two appended messages: first an assistant message containing the model's last (invalid) output, and then a user message that quotes the offending block and asks the model to re-issue a valid <tool_call> JSON block. Call this when handling a ToolCallParseException to prepare the conversation for a retry streaming invocation.

## Remarks
This helper centralizes the retry fix-up behavior so the retry attempt sees both the model's broken output (so it can inspect and correct it) and a clear, user-formatted instruction describing the parse error and the exact corrective action required. By returning a fresh IReadOnlyList it preserves the original history collection and provides a deterministic augmented history to pass to the re-streaming call.

## Example
```csharp
// Given an existing history, the raw model output, and a parse exception:
IReadOnlyList<ChatProviderMessage> history = ...;
string fullModelOutput = "...model emitted text including an invalid tool_call block...";
ToolCallParseException ex = ...; // exception containing OffendingBlock and Message

var retryHistory = AppendFixupMessage(history, fullModelOutput, ex);
// Pass retryHistory to the routine that re-streams the model response.
```

## Notes
- The returned list is a new List instance (original history is not mutated).
- fullModelOutput is inserted as an Assistant message verbatim — ensure including that text is acceptable (it may contain sensitive or large content).
- The user-facing instruction explicitly requests a plain <tool_call>{"name":"...","arguments":{...}}</tool_call> block (no code fences) and reminds the model that "arguments" must be a JSON object.

---

## BuildToolDocs

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Builds a plain-text instruction block that describes the agent's tool-calling protocol and enumerates the provided tools. Use this when composing a prompt or system message for an LLM so the model understands the exact <tool_call> JSON syntax and which tools (and parameter schemas) are available.

## Remarks
This method centralizes the textual prompt fragment that teaches the model how to invoke external tools: it emits the rules for embedding a <tool_call> JSON block and then lists each ToolDescriptor with its name, description, and ParametersJsonSchema. The output is plain text (not fenced) and intended to be embedded directly into a system or user prompt produced by the tool bridge. The returned string is trimmed of trailing whitespace.

## Example
```csharp
// Given an IReadOnlyList<ToolDescriptor> tools already populated:
var docs = BuildToolDocs(tools);
Console.WriteLine(docs);

// Sample output begins:
// [Tool calling]
// You have access to the following tools. To use one, emit a block in this exact format, inline anywhere in your response:
//
// <tool_call>{"name":"<tool_name>","arguments":{...}}</tool_call>
//
// Rules:
// - The block must contain valid JSON. The "arguments" field must be a JSON object - use {} if the tool takes no arguments.
//
// Available tools:
//
// - Search: Searches the web.
//   Parameters: {"type":"object","properties":{"query":{"type":"string"}}}
//
```

## Notes
- The method does not validate its input: passing a null tools reference will throw. Ensure callers provide a non-null `IReadOnlyList<ToolDescriptor>`.
- ParametersJsonSchema is written verbatim; if it contains newlines or complex formatting those characters will appear in the output.
- The returned string is TrimEnd()'d to remove any trailing newline/whitespace.

---

## EscapeJsonString

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Returns a copy of the input string with backslashes and double quotes escaped so it can be placed inside a JSON string literal. Use this small helper when you need a minimal, manual escape of a value embedded into JSON text (e.g. building a tiny JSON fragment by string concatenation) instead of using a full JSON serializer.

## Remarks
This helper only performs the two most common escapes required for JSON string delimiters: it turns each backslash into \\\\ and each double quote into \". It exists to keep manual JSON string construction simple and fast; it is not intended to replace a proper JSON serializer when you are producing structured JSON or handling arbitrary input.

## Example
```csharp
var raw = "C:\\Temp\\file.txt";
var escaped = EscapeJsonString(raw);
// escaped == "C:\\\\Temp\\\\file.txt"

var name = "She said \"Hi\"";
var json = "{\"name\":\"" + EscapeJsonString(name) + "\"}";
// json == {"name":"She said \"Hi\""}

// When embedding into an interpolated string:
var json2 = $"{{\"path\":\"{EscapeJsonString(raw)}\"}}";
```

## Notes
- The method does not accept null; passing null will produce a NullReferenceException. Ensure the input is non-null or guard before calling.
- It only escapes backslashes and double quotes. Control characters (newline, tab, carriage return) and other characters that JSON requires escaping (e.g. 0x00–0x1F) are not transformed. Use a JSON serializer (System.Text.Json, Newtonsoft.Json) for correct and complete escaping of arbitrary values.
- Order of replacements matters: backslashes are escaped first to avoid double-escaping of previously inserted backslashes for escaped quotes.

---

## InjectToolDocs

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Inserts a generated system message that documents available tools into an existing chat history. Use this when preparing the system prompt for a model so that tool descriptors (name, description, JSON schema) appear immediately after the block of system messages and before the first non-system conversational message.

## Remarks
This helper ensures tool documentation is placed at the boundary between system-level messages and the rest of the conversation. It leaves the original history objects intact and returns a new list with the docs message inserted: system messages up to the first non-system message are preserved, the tool docs message is added, then the remainder of the conversation follows. If the history contains only system messages the tool docs are appended after them; if there are no system messages the docs are inserted at the start.

## Example
```csharp
// Given a conversation history and a set of tool descriptors
var history = new List<ChatProviderMessage> {
    new ChatProviderMessage(Role: MessageRole.System, Content: "Persona and instructions"),
    new ChatProviderMessage(Role: MessageRole.User, Content: "Hello")
};

var tools = new List<ToolDescriptor> { /* tool descriptors */ };

var augmented = InjectToolDocs(history, tools);
// augmented now contains the system persona, then the generated tool docs system message,
// then the original user message and remaining conversation.
```

## Notes
- If tools.Count is 0 the original history is returned unchanged.
- The method creates a new List and copies references to the existing messages; it does not deep-copy ChatProviderMessage instances.
- Complexity is linear in the history length due to copying; this is intentional to avoid mutating the input collection.

---

## SerializeCallInline

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Constructs a compact JSON string representing an assistant-emitted tool call with a name and an arguments payload. This method is used when the system needs an inline representation of a tool invocation: it ensures the provided argumentsJson is valid JSON (falling back to an empty object) and wraps the tool name and arguments into a single JSON object string.

## Remarks
This implementation is defensive: if argumentsJson is null, empty, whitespace, or not valid JSON, the method substitutes an empty JSON object ({}). The tool name is passed through EscapeJsonString before being embedded to prevent breaking the JSON string value.

## Example
```csharp
// Valid JSON arguments
var s1 = SerializeCallInline("toolA", "{\"foo\":true}");
// s1 -> {"name":"toolA","arguments":{"foo":true}}

// Invalid JSON or empty arguments => arguments replaced with {}
var s2 = SerializeCallInline("toolB", "not-json");
// s2 -> {"name":"toolB","arguments":{}}

// Null or whitespace arguments => {}
var s3 = SerializeCallInline("toolC", "   ");
// s3 -> {"name":"toolC","arguments":{}}
```

## Notes
- argumentsJson is validated by attempting to parse it with JsonDocument; any JsonException causes the method to use "{}" instead.
- The method does not enforce that the arguments JSON is an object; arrays or primitives are accepted and embedded as-is when they are valid JSON.
- The tool name is escaped via EscapeJsonString to avoid injecting invalid JSON; callers should not pre-escape the name.

---

## StreamAsync

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        string modelName,
        [EnumeratorCancellation] CancellationToken ct = default)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `history` | ``IReadOnlyList<ChatProviderMessage>`` | — |
| `tools` | ``IReadOnlyList<ToolDescriptor>`` | — |
| `modelName` | `string` | — |
| `ct` | `CancellationToken` | `default` |

**Returns:** ``IAsyncEnumerable<ChatProviderEvent>``


Streams events from the decorated inner chat provider while intercepting and translating any embedded tool-call markers into the provider's structured events. Use this when you want an LLM-backed chat provider to support tool calls expressed in the model's textual output: the bridge injects tool descriptors into the system prompt, forwards the inner provider's stream, and emits safe text deltas, reasoning deltas, tool-call readiness events and a finish event while applying retry logic to avoid retracting already-streamed text.

## Remarks
This method is a decorator around an inner Chat provider. It first injects tool documentation into the conversation (so the inner provider sees tool descriptions in the prompt) and then calls the inner provider with an empty tool list. The implementation attempts the parse/emit cycle multiple times: the first attempt streams text live (so users see text as it arrives), while subsequent attempts buffer the inner provider's textual output and only emit corrected tool-call events to avoid retracting previously streamed text. A ToolCallStreamSplitter is used to separate safe text deltas from candidate tool-call fragments; reasoning-channel events are forwarded unchanged and any ToolCallReadyEvent emitted by the inner provider is passed through.

## Example
```csharp
await foreach (var evt in bridge.StreamAsync(history, tools, modelName, cancellationToken))
{
    switch (evt)
    {
        case TextDeltaEvent td:
            Console.Write(td.Delta);
            break;
        case ReasoningDeltaEvent rd:
            // show internal reasoning channel
            break;
        case ToolCallReadyEvent tc:
            // inspect tc.ParsedCall / invoke tool
            break;
        case FinishEvent fe:
            Console.WriteLine("<finished: " + fe.Reason + ">");
            break;
    }
}
```

## Notes
- The first (attempt==0) run streams text incrementally; retries buffer textual output and only emit corrected tool-call events — consumers must handle both incremental deltas and bulk retry deltas.
- The bridge injects tool descriptors into the system prompt and calls the inner provider with an empty tools list; do not assume the inner provider receives the tool descriptor collection separately.
- ReasoningDeltaEvent values are forwarded unchanged; any native ToolCallReadyEvent produced by the inner provider is also passed through.


---

## TranslateHistory

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Rewrites a chat history so a downstream, non-tool-capable model sees previous tool interactions as plain text the model understands. Use this when you need to convert provider-level assistant/tool messages (which may include structured ToolCalls and Tool messages) into a sequence composed only of plain assistant/user messages with embedded <tool_call> blocks and tool-result annotations.

## Remarks
This function walks the input history twice: first to build a map from tool call IDs to their original tool names (so tool-result messages can be labeled), then to produce a translated list. Assistant messages that contained structured ToolCalls are converted into assistant-text messages whose Content includes inline <tool_call>...</tool_call> fragments (the original ToolCalls and ToolCalls fields are cleared). Tool messages are converted into user messages prefixed with "[Tool result: <name>]" where the name is recovered from the earlier mapping; if the name isn't known the result is labeled "unknown." All other messages are passed through unchanged. The result is a new list; the original history is not mutated.

## Example
```csharp
// Input (conceptual): an assistant message that invoked a tool, then a tool result message
var history = new List<ChatProviderMessage>
{
    // Assistant: had an internal ToolCalls list (not shown here in full)
    new ChatProviderMessage(Role: MessageRole.Assistant, Content: "Thinking...", ToolCalls: /* calls with Id/Name/ArgumentsJson */ null),

    // Tool: produced output linked to the assistant's tool call id
    new ChatProviderMessage(Role: MessageRole.Tool, Content: "42", ToolCallId: "call-1")
};

// After TranslateHistory(history):
// - The assistant message's Content will include an inline <tool_call>...</tool_call> block
//   representing the invoked tool and its arguments, and its ToolCalls will be cleared.
// - The tool message becomes a user message with content like:
//   "[Tool result: <tool-name>] 42"
var translated = TranslateHistory(history);
```

## Notes
- The mapping from ToolCallId → tool name is built by scanning the history in order; if a Tool message appears before the assistant message that declared the corresponding ToolCall the tool name will be reported as "unknown." 
- The function clears ToolCalls/ToolCallId in the produced messages — structured tool metadata is intentionally lost in favor of wire-format text the inner model expects.
- SerializeCallInline is used to render tool calls inline; the exact inline syntax depends on that implementation and is not interpreted here.

---

## MaxParseRetries

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** field

```csharp
// Three attempts total = original + 2 retries. Matches the
    // EmptyStopMaxRetries pattern already used elsewhere in the agent loop.
    private const int MaxParseRetries = 2
```


Number of additional parse retries the parser will attempt after the initial parse attempt. Set to 2, this yields three total parse attempts (initial attempt + 2 retries) and is used to limit repeated parse attempts in the tool-bridge parsing logic.

## Remarks
This constant enforces a small, bounded retry policy for parsing failures so the agent loop does not enter prolonged or infinite retry cycles. It intentionally matches the EmptyStopMaxRetries pattern used elsewhere in the agent loop to keep retry behavior consistent across related components. Being a private const keeps the behavior fixed and predictable within this implementation.

## Notes
- MaxParseRetries counts only the retries, not total attempts; total attempts = MaxParseRetries + 1.
- Because it is a compile-time constant and private, changing the value requires a code change and rebuild; tests or external callers cannot override it without refactoring.

---