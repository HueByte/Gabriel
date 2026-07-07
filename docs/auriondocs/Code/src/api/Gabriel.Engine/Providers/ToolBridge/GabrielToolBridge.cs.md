# GabrielToolBridge.cs

> **Source:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`

## Contents

- [GabrielToolBridge](#gabrieltoolbridge)
- [GabrielToolBridge](#gabrieltoolbridge-1)
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

## GabrielToolBridge
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** class

```csharp
public sealed class GabrielToolBridge : IChatProvider
```


Bridges a non-tool-calling ("tool-incapable") chat provider to the IChatProvider surface expected by the agent runtime. Use this when you need to treat a local or constrained LLM (one that cannot perform native function/tool calls) as if it produced the same event shape as a provider that supports tool calls so the agent loop and UI can remain unchanged.

## Remarks
This decorator adapts an inner IChatProvider by: translating prior structured tool interactions into inline markers, injecting a system message that documents the available tools and the wire format, streaming the inner provider's text while detecting embedded <tool_call> markers, and parsing buffered tail text into ToolCall events. It intentionally does not execute tools itself — tool execution remains the responsibility of the server-side AgentService/ITool pipeline. The class preserves provider identity and model metadata (Name and Models) so registry and selection logic continue to behave as if the original provider were used.

## Notes
- Live streaming of text is only provided for the first parse attempt. If the bridge must retry parsing the model output, the retry's textual output is not re-streamed (to avoid "unsending" already emitted text); only the corrected tool-call events from the retry are produced.
- The inner provider is called with an empty tools list: tool descriptors are embedded in the injected system prompt instead of being passed via the tools parameter.
- This class only bridges the wire protocol/format; it does not execute tools. Tool execution continues to occur via AgentService and ITool implementations.
- If the underlying provider unexpectedly emits native tool calls despite receiving an empty tools list, the bridge will pass them through (honouring the inner provider rather than hiding them).

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


Initializes a GabrielToolBridge with the provided IChatProvider and `ILogger<GabrielToolBridge>` instances, storing them for subsequent use by the bridge to perform chat operations and to emit diagnostic logs.

## Remarks
This constructor embodies dependency injection by taking its collaborators as parameters, enabling easy swapping of implementations and straightforward unit testing with mocks. It decouples GabrielToolBridge from concrete chat or logging implementations, and the `ILogger<GabrielToolBridge>` parameter provides type-scoped logging to aid diagnosis and tracing through the bridge's activity.

## Notes
- No argument validation is performed; ensure non-null values are supplied to avoid NullReferenceException during use.

---

## Models
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public IReadOnlyList<LLMModel> Models => _inner.Models
```


This property surfaces the current collection of LLMModel configurations available through the ToolBridge. It delegates to the underlying inner provider and exposes them as a read-only `IReadOnlyList<LLMModel>`, letting callers enumerate the models and inspect their metadata (e.g., Name, IsActive, ContextWindowTokens, and pricing or capability flags) without mutating the collection.

## Remarks
By acting as a pass-through, Models decouples consumer code from the inner implementation and provides a stable contract for discovering available models. It relies on the LLMModel metadata to inform orchestration decisions about activation, context, and pricing when selecting a model for a task.

## Notes
- Read-only contract ensures consumers cannot alter the collection; mutations (if any) would need to occur via the inner provider.
- If the inner model list updates over time, consumers may observe updated items on subsequent enumerations.

---

## Name
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public string Name => _inner.Name
```


Returns the underlying provider’s name by delegating to the wrapped (inner) provider, preserving the base provider’s identity even when wrapped by this decorator. This ensures that the agent registry resolves by the original provider’s name, and that models owned by the wrapped provider remain associated with that base name.

Use this property when you need the original provider name for registry lookups, logging, or configuration that relies on the base provider’s identity rather than the decorator’s wrapper.

## Remarks
This property is a straightforward pass-through of the inner provider’s Name. It lets the decorator participate in naming and discovery without altering identity, which helps maintain consistent behavior across wrapped providers and preserves the semantics described in the comments that identity passes through.

## Notes
- Assumes _inner is non-null; accessing Name will throw if the inner provider is null.
- The value is delegated live — changes to the inner provider’s Name reflect immediately through this property.

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


Appends a corrective messaging sequence to a chat history after a tool-call parse failure. The method accepts the current history, the full textual model output, and the parse exception, and returns a new read-only history that first preserves the previous history and the model's failed output, then appends a user-facing fix-up prompt. This prompt reports the offending block, states the parse error, and instructs the model to re-issue its response from scratch with valid <tool_call>{"name":"...","arguments":{...}}</tool_call> blocks, ensuring the "arguments" field is a JSON object and that the block is not wrapped in code fences.

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


BuildToolDocs constructs a compiled, human- and machine-readable guide describing how to invoke runtime tools. Given a collection of ToolDescriptor entries, it returns a single string that begins with a [Tool calling] header and enumerates the exact block format to emit when requesting a tool invocation. It also lists each available tool by name, its description, and a JSON schema for its parameters. This helper is used whenever you need to expose the current tool set to users or to other parts of the system in a stable, self-describing format.

## Remarks
BuildToolDocs centralizes the formatting of tool invocation guidance, ensuring consistent help text across tool sets and reducing duplication when tool availability changes. By deriving the documentation from ToolDescriptor, the output stays in sync with the actual surface exposed by the tool bridge. The produced string is intended for display or logging rather than direct execution.

## Notes
- The generated text begins with a literal [Tool calling] section and instructs users to emit a <tool_call> block in a specific JSON format. The exact block shape is not executed by this function; downstream components are responsible for handling tool invocations.
- If the tools collection is empty, the output still contains the header and a section listing "Available tools" but with no tool entries.

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


EscapeJsonString converts a string into a form safe for JSON string literals by escaping backslashes and double quotes. Use it when you need to embed arbitrary text into a JSON string literal without relying on a full serializer.

## Remarks
This is an internal helper that ensures consistent escaping across the ToolBridge code path. By performing replacements in a fixed order (backslashes first, then quotes), it avoids double-escaping issues and aligns with JSON string rules. Because it's private to the containing class, its usage is restricted to that scope, reducing the risk of inconsistent escaping elsewhere.

## Notes
- The method does not handle null input; passing null would throw a NullReferenceException.
- It escapes only backslashes and double quotes; other JSON-sensitive characters (e.g., newlines) are not escaped by this helper.
- Being private, it's intended as an internal utility. If you need to escape in external code, use a standard JSON serializer.

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


InjectToolDocs augments a chat history by injecting a generated tool-documentation payload as a system message, placing it immediately after any existing system messages. When a non-empty set of ToolDescriptor entries is provided, it creates a new ChatProviderMessage with Role set to System and Content produced by BuildToolDocs(tools). It then computes the insertion point at the boundary between system messages and the rest of the conversation and returns a new history sequence that includes the docs message at that point; if no tools are supplied, it returns the original history unmodified. This enables the runtime to expose available tools to the model without altering the original conversation flow.

## Remarks
InjectToolDocs centralizes the mechanism of advertising tool capabilities to the model. By deriving the docs content from BuildToolDocs and injecting it at the system boundary, it guarantees the model sees the tools before any user or assistant turns. The approach helps keep the tool-availability concerns separate from the rest of the chat history manipulation, making testing and reasoning easier. It also preserves the original ordering of messages, only adding a single docs message.

## Notes
- Repeated calls can insert multiple identical tool-doc messages if the history is augmented multiple times; consider guarding against re-injection in cases where history may already include tool docs.
- If there are no tools provided (tools.Count == 0), the function is a no-op and returns the input history unchanged.

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


Converts a tool invocation into a single JSON payload used by the inline renderer. It takes a tool name and a JSON-encoded arguments string, validates and normalizes the arguments, escapes the tool name, and returns a JSON string in the shape {"name":"<escaped>","arguments":<args>}.

## Remarks
This helper centralizes the creation of tool-call payloads and guards against malformed input. It uses JsonDocument.Parse to validate the provided arguments and falls back to {} when arguments are missing or invalid, ensuring downstream consumers never receive invalid JSON. The tool name is escaped with EscapeJsonString to prevent injection issues inside the payload.

## Notes
- If argumentsJson is valid JSON but not an object (e.g., an array or primitive), that value is embedded as the arguments field as-is.
- The validation step does not mutate the input beyond choosing a safe default when invalid; the original string is otherwise preserved inside the payload.

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


Streams chat provider events asynchronously by wrapping an inner provider, enriching the history with tool descriptors, and emitting a sequence of events that may include TextDeltaEvent, ReasoningDeltaEvent, ToolCallReadyEvent, or FinishEvent. It first translates the incoming history and injects tool documentation, then delegates to the inner provider and post-processes its stream to surface tool invocations in a way that supports both live streaming and retry scenarios.

## Remarks
This symbol acts as a façade around the inner chat provider to inject tool-related metadata into the prompt and to orchestrate a two-mode streaming strategy. In live mode, it yields sanitized text chunks as the model streams, preserving the surrounding narrative while suppressing or transforming tool-call cues as needed. In retry mode, it buffers output to allow corrections or re-folding of tool invocations, ensuring that the final presented stream reflects the intended tool usage even if the initial live stream includes imperfect tool markers. The ToolCallStreamSplitter is the abstraction that detects tool_call markers in the text and governs when to emit plain text versus tool-invocation signals. This design isolates the complexity of tool invocation handling from callers, providing a consistent streaming experience while guarding against unsafe retractions of already-emitted content.

## Notes
- The method operates in two phases: a live mode that streams text as produced and a retry mode that buffers text for a consolidated re-emission. You cannot retract text emitted in live mode; the retry path exists to mitigate scenarios where tool calls need to be reinterpreted.
- If no <tool_call> marker showed up, the implementation emits a FinishEvent with the inner finish reason, ensuring clean turn boundaries. In retry mode, if the buffered tail contains no tool_call markers, the buffered text is emitted as a single delta to preserve readability.
- When the inner provider emits native tool signals despite an empty tool-descriptor list, those signals are forwarded to callers for safety, though the outer wrapper typically drives the tooling via InjectToolDocs.

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


Translates a history of ChatProviderMessage entries into a wire-format compatible with an inner model that cannot perform tools directly. It does this by first building a map from tool-call IDs to tool names and then producing a translated history: assistant messages that invoked tools are expanded to include inline <tool_call> blocks for each call, and tool messages are converted into user messages labeled with the corresponding tool result. The function returns the transformed, read-only history while leaving the original history untouched.

## Remarks
This translation layer decouples the outer tool-boundary interactions from the inner model's expectations. By materializing tool calls as inline blocks, it preserves the semantic meaning of tool usage without requiring the inner model to understand ToolCall constructs. The forward scan ensures that every tool-output can be named consistently when a Tool message is encountered. If a tool call ID cannot be resolved, the tool result is labeled as unknown, which helps surface incomplete tool-traceability rather than failing silently.

## Notes
- If a Tool message references a ToolCallId that cannot be resolved, the code uses "unknown" as the tool name.
- The transformation never mutates the input history; it returns a new translated list.

---

## MaxParseRetries
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** field

```csharp
private const int MaxParseRetries = 2
```


MaxParseRetries is a private constant that defines how many additional parse attempts are allowed after the initial attempt. With a value of 2, this enforces a total of three parse attempts before giving up, mirroring the EmptyStopMaxRetries pattern used in the agent loop.

## Remarks
MaxParseRetries is private to the containing class and serves as an internal tuning knob for the parsing path. It establishes a deterministic retry ceiling that helps bound latency and resource usage within the GabrielToolBridge flow, while keeping external behavior untouched. Adjusting this value should be coordinated with other retry semantics in the agent loop to preserve consistent retry behavior across the system.

## Example
```csharp
// Example usage of the retry bound
for (int attempt = 0; attempt <= MaxParseRetries; attempt++)
{
    // attempt to parse; exit loop on success
}
```

## Notes
- The loop bound using <= MaxParseRetries yields MaxParseRetries + 1 total parse attempts (the initial attempt plus retries).

---