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
public sealed class GabrielToolBridge : IChatProvider
```


Bridges a tool-incapable chat provider so it presents the same IChatProvider surface as a native tool-calling provider. Use this when you need the agent loop, UI and downstream logic to receive the usual tool-call events from a model that cannot execute or natively signal tool calls (for example, a local Ollama model or a small commercial model without function-calling). The bridge translates between the model's plain-text output and the agent's ToolCall events without changing where or how tools are executed.

## Remarks
This decorator rewrites the outgoing conversation and interprets incoming text so the agent stack can remain unchanged. Before calling the inner provider it serialises prior tool calls into inline markers and appends a system message that documents the available tools and the expected wire format. While streaming, it splits live text deltas and buffers the tail when it detects a tool-call marker; after the stream completes it parses the buffered tail to synthesise ToolCall events which the agent receives just like native tool calls. The bridge does not execute tools itself — actual tool execution remains the responsibility of the server-side AgentService/ITool pipeline.

## Example
```csharp
// Wrap an existing tool-incapable provider so the agent loop can handle tool events
IChatProvider innerProvider = new OllamaChatProvider(...);
var bridge = new GabrielToolBridge(innerProvider, logger);

await foreach (var ev in bridge.StreamAsync(history, tools, modelName))
{
    // ev can be standard text deltas, reasoning events, or synthesized ToolCall events
    HandleEvent(ev);
}
```

## Notes
- Live typewriter-style streaming is provided only on the first parse attempt; if the bridge must retry parsing the tool-call block (MaxParseRetries = 2), subsequent attempts do not stream their corrected text — only their final tool calls are emitted.
- The bridge calls the inner provider with an empty tools list; tool descriptors are injected into the system prompt instead. Do not assume the inner provider will perform tool execution.
- If the underlying model nevertheless emits native tool-call events, the bridge will pass them through unchanged — this means the decorator is safe but possibly redundant in that case.
- Parsing can fail; the bridge will append a corrective user message and retry a bounded number of times before falling back to emitting the raw text.

---

## GabrielToolBridge
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** constructor

```csharp
public GabrielToolBridge(IChatProvider inner, ILogger<GabrielToolBridge> logger)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `inner` | [`IChatProvider`](../IChatProvider.cs.md) | — |
| `logger` | `ILogger<GabrielToolBridge>` | — |

**Returns:** `public`


GabrielToolBridge's constructor wires two essential dependencies into the bridge: an IChatProvider for chat I/O and an `ILogger<GabrielToolBridge>` for logging and diagnostics. Use this constructor when configuring the bridge in a DI container or in tests, providing concrete implementations (or mocks) of IChatProvider and `ILogger<GabrielToolBridge>` so the bridge can delegate messaging and logging.

## Remarks
GabrielToolBridge serves as a bridge between the Gabriel engine and a chat provider, centralizing chat I/O behind a stable abstraction and enabling observable logging through the bridge’s logger. By accepting dependencies rather than instantiating them, it remains easily testable and swappable in different environments. Note that the constructor shown does not perform argument validation, so callers should guarantee non-null values to avoid a NullReferenceException later in the bridge’s operation.

## Example
```csharp
// Example: simple instantiation
IChatProvider chat = new ConsoleChatProvider();
ILogger<GabrielToolBridge> logger = new LoggerFactory().CreateLogger<GabrielToolBridge>();
var bridge = new GabrielToolBridge(chat, logger);
```

## Notes
- The constructor does not enforce null-checks; passing null for either dependency may lead to NullReferenceException during bridge operation.
- When wiring via a DI container, ensure that IChatProvider and `ILogger<GabrielToolBridge>` are resolved with appropriate lifetimes to match the bridge’s usage.

---

## Models
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public IReadOnlyList<LLMModel> Models => _inner.Models
```


This property exposes the read-only list of LLMModel instances sourced from the underlying inner provider. It delegates to _inner.Models, giving callers a non-mutating view of the models currently available through the Gabriel Tool Bridge.

## Remarks
By delegating to the inner provider, this symbol decouples consumers from the concrete implementation and provides a consistent surface for discovering available models at the bridge boundary. It simplifies testing and composition by letting the inner provider vary without requiring callers to change their access pattern. It also serves as a stable access point for UI bindings or logging that need to enumerate the models.

## Example
```csharp
// Most common usage: list available models from the bridge
foreach (var model in bridge.Models)
{
    Console.WriteLine($"{model.Name} ({model.Id})");
}
```

## Notes
- This is a live view into the inner provider's Models; there is no snapshot. Changes to the underlying collection are reflected here as they occur.
- You cannot mutate the list via this property; to change which models exist, modify the inner provider.

---

## Name
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public string Name => _inner.Name
```


This read-only property forwards to the inner provider's Name, preserving the underlying provider identity even when the provider is wrapped by a decorator. It enables callers to rely on the base provider’s canonical name for registry resolution and model wiring, without needing to unwrap the decorator.

## Remarks
This member is a transparent piece of the decorator pattern: identity remains stable across wrapping boundaries, so consumers can treat decorated and non-decorated providers uniformly when querying by Name. It keeps the naming contract intact for the agent registry and any components that log or route based on a provider’s Name.

## Notes
- If the decorator is not initialized with a non-null inner provider, accessing Name will throw a NullReferenceException.
- Name is immutable from the decorator’s perspective; it simply exposes the inner provider's Name. To change identity, alter the inner provider itself.

---

## AppendFixupMessage
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static IReadOnlyList<ChatProviderMessage> AppendFixupMessage(
        IReadOnlyList<ChatProviderMessage> history,
        string fullModelOutput,
        ToolCallParseException ex)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `history` | `IReadOnlyList<ChatProviderMessage>` | — |
| `fullModelOutput` | `string` | — |
| `ex` | [`ToolCallParseException`](ToolCallBlockParser.cs.md) | — |

**Returns:** `IReadOnlyList<ChatProviderMessage>`


After a parse failure caused by an invalid tool_call block in the chat history, this helper constructs a new, augmented history that includes the model’s previous output and a corrective prompt. It appends two messages: first, the Assistant’s last model output to preserve context, and second, a User-facing fixup message that exposes the offending block and the parse error, then instructs the model to re-issue a corrected response using a valid <tool_call>{"name":"...","arguments":{...}}</tool_call> structure. The method returns a new `IReadOnlyList<ChatProviderMessage>` that preserves the original history while supplying the necessary guidance for retry. This keeps retry logic encapsulated in one place and ensures consistent, self-describing prompts during error recovery.

---

## BuildToolDocs
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static string BuildToolDocs(IReadOnlyList<ToolDescriptor> tools)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `tools` | `IReadOnlyList<ToolDescriptor>` | — |

**Returns:** `string`


BuildToolDocs generates a narrative description of how to invoke runtime tools from within a response. It accepts a list of ToolDescriptor objects and renders a self-contained guide that explains the exact inline tool_call blocks to emit, the required JSON structure for arguments, and the rule that tool results arrive in subsequent messages prefixed with [Tool result: ToolName]. Use this when you need to expose up-to-date, machine-generated tool invocation guidance to users or client code, instead of hand-writing the instructions.

## Remarks
By centralizing tool metadata into ToolDescriptor, this helper ensures the documentation stays in sync with the actual tool surface. It decouples the presentation from the tool definitions and provides a consistent protocol that downstream clients can rely on when integrating tool calls into their workflows.

## Example
```csharp
// Most common usage: emit a tool call block inline
<tool_call>{"name":"SpellCheck","arguments":{"text":"Ths sntnc has erors."}}</tool_call>
```

## Notes
- The "arguments" field must be a JSON object; use {} if there are no arguments.
- Do not wrap the <tool_call> block in Markdown code fences; emit inline in your response.
- After a tool runs, results will appear as messages prefixed with [Tool result: ToolName]; use those results to drive further actions.

---

## EscapeJsonString
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static string EscapeJsonString(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"")
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `s` | `string` | — |

**Returns:** `string`


Escapes a string so it can be safely embedded in a JSON string literal. It first escapes backslashes by doubling them, then escapes double quotes, ensuring the value remains syntactically valid when inserted into manually constructed JSON.

## Remarks
This tiny helper centralizes a focused escaping behavior, reducing duplication across call sites and guarding against basic JSON syntax errors in hot paths where a full serializer would be overkill. It is not a full JSON encoder; for complete JSON serialization, rely on a dedicated library.

## Example
```csharp
// Common case: escape a value before embedding in JSON
string input = "A \"quote\" with \\ backslash";
string escaped = EscapeJsonString(input);
```

## Notes
- Escapes only backslashes and double quotes; other JSON control characters are not handled here.
- Not a substitute for a proper JSON serializer.
- Private method; intended for internal usage within the hosting class.

---

## InjectToolDocs
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static IReadOnlyList<ChatProviderMessage> InjectToolDocs(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `history` | `IReadOnlyList<ChatProviderMessage>` | — |
| `tools` | `IReadOnlyList<ToolDescriptor>` | — |

**Returns:** `IReadOnlyList<ChatProviderMessage>`


InjectToolDocs constructs a single system message containing tool documentation and inserts it into the chat history so that the model is aware of available tools before responding. It relies on BuildToolDocs(tools) to render the descriptors and places the resulting system message right after existing system messages but before the first user/assistant turn; if there are no non-system messages yet, the docs are appended at the end. The method returns a new history list, leaving the original history untouched.

## Remarks
This symbol exists to decouple tool metadata generation from prompt assembly. By centralizing the tool documentation in a system message, the model consistently sees the same tool surface across turns and callers can enable or disable tooling without changing prompt logic. The insertion logic preserves the surrounding system messages so system-level context is kept at the top.

## Notes
- Repeated calls may duplicate the docs; call only once at the start of a conversation or guard against existing docs.
- The function scans history linearly to find the insertion point; for very long histories this is O(n).

---

## SerializeCallInline
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static string SerializeCallInline(string name, string argumentsJson)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `name` | `string` | — |
| `argumentsJson` | `string` | — |

**Returns:** `string`


Serializes a tool invocation into a compact JSON payload by pairing a tool name with its arguments. The tool name is escaped for safe JSON embedding, and the provided arguments string is validated as JSON; if it is blank or invalid, an empty object is substituted to guarantee a valid payload.

## Remarks
Centralizes the formatting of inline tool calls produced by the assistant. It decouples name escaping and argument validation from callers, ensuring consistency and reducing the risk of malformed tool invocations being rendered downstream. The implementation uses a lightweight JSON validation step (JsonDocument.Parse) to avoid altering the original arguments when they are already valid JSON.

## Example
```csharp
// Example
var payload = SerializeCallInline("ComputeSum", "{\"a\":1,\"b\":2}");
// payload == "{\"name\":\"ComputeSum\",\"arguments\":{\"a\":1,\"b\":2}}"
```

## Notes
- If argumentsJson is blank or invalid JSON, the resulting payload uses an empty object for the arguments field, which may drop intended arguments.
- The name is escaped with EscapeJsonString to prevent breaking the JSON payload.
- The method is private and intended for internal use by the ToolBridge to standardize tool call serialization.

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
| `history` | `IReadOnlyList<ChatProviderMessage>` | — |
| `tools` | `IReadOnlyList<ToolDescriptor>` | — |
| `modelName` | `string` | — |
| `ct` | `CancellationToken` | `default` |

**Returns:** `IAsyncEnumerable<ChatProviderEvent>`


Streams an asynchronous sequence of chat provider events produced by a language model, wrapping an inner provider to pre-inject tool descriptors into the system prompt and post-process the output to reliably surface tool invocations. It supports a retry strategy: the first pass streams live; subsequent passes buffer text until a valid tool-call candidate is observed, ensuring tool calls are surfaced correctly even when the model delays or obscures them.

## Remarks
By isolating tool-call handling behind a splitter and a retry buffer, this symbol decouples tool invocation from plain text streaming. It guarantees a consistent surface for tool calls across models and timing variations, while keeping the rest of the streaming pipeline agnostic to the exact signaling of tool usage. It also embeds tool descriptors into the system prompt once, so the inner provider can reference them without re-supplying descriptors on every turn; if the inner provider emits unexpected tool-calls, they are still forwarded safely rather than being dropped.

## Example
```csharp
// Example: consuming the streamed events from the tool bridge
IAsyncEnumerable<ChatProviderEvent> stream = bridge.StreamAsync(history, tools, modelName, ct);
await foreach (var e in stream)
{
    switch (e)
    {
        case TextDeltaEvent td:
            Console.Write(td.Delta);
            break;
        case ReasoningDeltaEvent rd:
            // Optional: surface reasoning in a debugging or audit view
            break;
        case ToolCallReadyEvent tc:
            // Handle the surfaced tool call (invoke tooling as needed)
            break;
        case FinishEvent f:
            // End of the current turn
            break;
    }
}
```

## Notes
- The method relies on the inner provider emitting or allowing tool calls to be surfaced as markers; if the inner provider does not emit or signal tool calls with the expected marker, tool invocations may be delayed or suppressed accordingly.
- The retry mechanism trades latency for correctness: increasing MaxParseRetries can improve the chance a tool call is captured, at the cost of additional buffering.
- CancellationToken ct is respected to cancel streaming; long-running streams will terminate when ct is canceled.


---

## TranslateHistory
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

```csharp
private static IReadOnlyList<ChatProviderMessage> TranslateHistory(IReadOnlyList<ChatProviderMessage> history)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `history` | `IReadOnlyList<ChatProviderMessage>` | — |

**Returns:** `IReadOnlyList<ChatProviderMessage>`


Translates the assembled chat history into a wire-format understood by the inner model by inlining tool calls in assistant messages and tagging tool results as user messages. Use this when you need to present tool interactions to a non-tool-capable model, rather than exposing raw tool calls.

## Remarks
This method uses a two-pass transformation: first it builds a map from tool-call IDs to tool names by scanning assistant messages that contain ToolCalls; then it rewrites the history to inline each tool call in the assistant content and convert tool messages to user messages annotated with the tool name. The translation clears the ToolCalls on assistant messages so the inner model sees plain text. If a tool call reference cannot be resolved (for example the ToolCallId is missing or unknown), the name defaults to "unknown" in the resulting user-facing tag.

## Example
```csharp
// Example: translate a history with one tool invocation
var history = new List<ChatProviderMessage>
{
  new ChatProviderMessage(Role: MessageRole.Assistant, Content: "Running tool...", ToolCalls: new [] { new ToolCall { Id = "tool-1", Name = "Summarize", ArgumentsJson: "{\"text\":\"Hello world\"}" } }),
  new ChatProviderMessage(Role: MessageRole.Tool, Content: "Result text", ToolCallId: "tool-1")
};
var translated = TranslateHistory(history);
// translated[0].Content now contains an inline tool_call block for Summarize
// translated[1].Role == User and Content starts with "[Tool result: Summarize] ..."
```

## Notes
- The translation inlines tool calls into the assistant text and clears the ToolCalls field on that message.
- Tool messages are turned into user messages tagged with the original tool name when available; unknown mappings fall back to "unknown".
- The order of messages and the two-pass approach are crucial to correctly resolve ToolCallIds to names.

---

## MaxParseRetries
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** field

```csharp
private const int MaxParseRetries = 2
```


Defines the maximum number of parse attempts allowed in the parsing loop of GabrielToolBridge. With a value of 2, the total number of attempts is three (the initial parse plus two retries). This centralizes the retry policy and aligns with the EmptyStopMaxRetries pattern used elsewhere in the agent loop, so callers can rely on a consistent, bounded parsing effort rather than ad hoc retries.

## Remarks
MaxParseRetries encodes the bounded-retry policy for parsing within GabrielToolBridge. Centralizing the limit ensures consistent behavior with the system's other retry patterns, specifically the EmptyStopMaxRetries pattern used in the agent loop. It prevents endless parsing loops by constraining effort after a fixed number of retries, helping maintain responsiveness under persistent input issues.

## Notes
- Not configurable at runtime; changing requires editing the code and recompiling.
- Keep in sync with other retry-pattern constants to avoid inconsistent behavior across the parsing/agent loops.

---