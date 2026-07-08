# GabrielToolBridge.cs

> **Source:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`

## Contents

- [GabrielToolBridge](#gabrieltoolbridge)
- [GabrielToolBridge (constructor)](#gabrieltoolbridge-constructor)
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


Bridges a tool-incapable chat provider (for example, a local Ollama model or a commercial model without function-calling) to the IChatProvider shape expected by the rest of the agent stack. Use this when you need the agent and UI to receive the same event stream and tool-call events from a provider that cannot natively emit tool-call frames — the bridge rewrites history, injects tool documentation, and converts the provider's text output into ToolCall events.

## Remarks
GabrielToolBridge adapts a non-tooling wire protocol so the agent loop (RunStreamAsync, AgentContext, UI) can operate unchanged. It does this by: translating prior structured tool interactions into inline markers in the assistant text and labeled user messages; appending a system message that describes available tools and the expected inline wire format; streaming the inner provider's deltas until a <tool_call> marker appears and buffering the remainder; and finally parsing buffered output into ToolCallReady events and ToolCalls. The bridge does not execute tools itself — tool execution remains the responsibility of AgentService/ITool — and the inner provider is invoked with an empty tools list because the tool descriptors are embedded in the system prompt. The implementation also performs a small retry loop (MaxParseRetries = 2) to attempt to repair parse failures, with a design tradeoff that only the first attempt is live-streamed.

## Notes
- Live (typewriter) streaming only occurs on the first parse attempt; retries suppress re-streaming of corrected text and emit only the corrected tool calls.
- Tool descriptors must be consumable from the injected system message because the inner provider receives an empty tools list.
- If the inner provider unexpectedly emits native tool-call events, the bridge will pass them through rather than blocking them.

---

## GabrielToolBridge (constructor)
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


GabrielToolBridge’s constructor wires the bridge to a chat provider and a logger. By accepting an IChatProvider and an `ILogger<GabrielToolBridge>` and storing them internally, it prepares the bridge to forward chat events and log its activity as part of its tool-bridging responsibilities. This pattern supports dependency injection and makes the bridge easier to test by supplying test doubles for its collaborators.

## Remarks
GabrielToolBridge acts as a thin adapter around a chat-providing component and a logging channel. The constructor’s two dependencies establish the bridge’s collaboration with the chat layer and the logging infrastructure, allowing the rest of the bridge to focus on bridging tool interactions rather than sourcing messages or emitting logs. This design favors testability and configurability via DI, since the behavior can be varied by swapping the IChatProvider and logger.

## Notes
- The constructor does not perform null-check validation; ensure non-null values are passed to avoid later NullReferenceException when the bridge uses its collaborators.
- When used in a dependency injection setup, configure the container to supply concrete implementations for IChatProvider and `ILogger<GabrielToolBridge>` to avoid runtime surprises.

---

## Models
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public IReadOnlyList<LLMModel> Models => _inner.Models
```


Exposes a read-only collection of LLMModel instances that are available through the underlying bridge. This property simply forwards to the inner provider’s Models collection, offering a stable surface for enumerating model metadata (such as name, activity, context window, pricing, and tool mode) without allowing callers to mutate the collection.

## Remarks
This property acts as a thin abstraction over the inner provider, decoupling consumers from the concrete implementation of the model source. By returning `IReadOnlyList<LLMModel>`, it supports discovery and inspection of available models while preserving encapsulation—mutations must occur through the inner provider itself or its configuration surface.

## Notes
- The returned collection represents a live view of the inner provider; updates to _inner.Models will be visible on subsequent reads.
- The collection is read-only; you cannot add or remove models via this property. To change the set of models, modify the inner provider and re-query.
- The LLMModel items may themselves be mutable. If you need a stable snapshot, copy the list (and/or its items) to your own structure before performing long-running operations or cross-thread work.

---

## Name
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

```csharp
public string Name => _inner.Name
```


The Name property simply returns the wrapped provider’s Name, acting as a transparent passthrough. By delegating to the inner provider, the decorator preserves the underlying provider’s identity, ensuring the agent registry continues to resolve by the base provider’s name and models remain owned by the wrapped provider.

## Remarks
This decorator pattern is identity-preserving: it allows augmentation of behavior in front of a provider without altering how the provider is identified. Callers can rely on the underlying Name for routing, logging, and diagnostics, while the wrapper itself can add cross-cutting concerns elsewhere. The separation keeps decoration concerns orthogonal to identity resolution.

## Notes
- Null-safety: _inner must be non-null; accessing Name will throw if the inner provider is not initialized.
- Immutability expectation: after construction, the inner provider should not be swapped if stability of identity is required; changing it could change the reported Name unexpectedly.

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


AppendFixupMessage creates a retriable conversation state after a parse failure by appending the model’s last output and a fix-up prompt that instructs re-issuing from scratch with a valid tool_call block.

## Remarks
AppendFixupMessage acts as a focused retry mechanism within the ToolBridge’s fix-up flow. It preserves the original conversation context by keeping the existing history, then appends the model’s previous assistant message followed by a user-facing instruction that requests a clean retry with a correctly formatted <tool_call> block and a JSON-encoded arguments object. By explicitly forbidding code fences and requiring a JSON object for arguments, it enforces the contract expected by the downstream parsing/dispatch logic and reduces the chance of repeated parse errors.

## Notes
- The method returns a new IReadOnlyList containing the original history, the assistant’s latest output, and the fix-up user message; it does not mutate the input history.
- It relies on ex.OffendingBlock and ex.Message to surface concrete diagnostics to guide the retry; if the offending block is absent, the fix-up content may be less informative.
- The behavior hinges on the ChatProviderMessage and MessageRole constructs to classify and carry the multi-turn dialogue through the retry step.


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


BuildToolDocs synthesizes a Markdown guide from a collection of ToolDescriptor objects, turning tool metadata into a consistent, user-facing reference for interactive tool usage. It outputs a block that begins with a [Tool calling] section, explains how to emit inline tool invocation blocks using the exact <tool_call>{"name":"<tool_name>","arguments":{...}}</tool_call> syntax, and enumerates each available tool by name, description, and a JSON schema of its parameters. This function is intended for scenarios where a user or agent needs a precise, copy-pasteable template to discover and invoke tools at runtime, rather than performing tool calls directly in code.

## Remarks
By centralizing tool metadata formatting in BuildToolDocs, the system ensures consistent guidance across different UI surfaces. It decouples the presentation from the tool definitions, so adding or updating tools only requires updating ToolDescriptor instances. The function also formalizes the interaction contract for tool invocation, reducing ambiguity for end users and downstream tooling.

## Notes
- The input must be non-null; passing null will throw a NullReferenceException.
- If ToolDescriptor.Name, Description, or ParametersJsonSchema are null or empty, the generated output may contain blanks; ensure these fields are populated.

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


Escapes backslashes and double quotes in a string so it can be embedded safely inside a JSON string literal. Use this when manually constructing JSON fragments to ensure literal backslashes and quotation marks do not terminate the string or corrupt syntax.

## Remarks
This method centralizes the escaping logic, so callers don't repeat Replace calls in multiple places. It assumes the input is raw text to be placed inside a JSON string; if you pass already escaped content, you may end up double-escaping. It does not perform a full JSON escaping pass; for complete safety, prefer a proper JSON serializer.

## Example
```csharp
string raw = "A \"quote\" and a \\ backslash";
string escaped = EscapeJsonString(raw);
// escaped == "A \\\"quote\\\" and a \\\\ backslash"
```

## Notes
- Applying EscapeJsonString more than once can double-escape quotes/backslashes; ensure you escape only raw input once or rely on a structured JSON serializer.

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


InjectToolDocs augments a chat history with a system-level description of the tools available to the model by inserting a dedicated system message. When tools are provided, it builds the documentation payload with BuildToolDocs and inserts it immediately after the initial block of system messages; if no tools are provided, the history is returned unchanged.

## Remarks
By placing the tool-descriptions under the System role, this helper centralizes how tool capabilities are conveyed (via BuildToolDocs) and keeps the normal user/assistant history intact. It computes a stable insertion point based on the first non-system message, ensuring tool docs appear early without reordering existing messages more than necessary. The approach minimizes coupling between history construction and the tool-descriptor formatting.

## Notes
- If the history contains no non-system messages, the docs message is appended after all system messages.
- The method returns a new IReadOnlyList and does not mutate the input history.

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


SerializeCallInline is a private helper that builds a JSON payload representing a single tool invocation for the bridge. It accepts a tool name and a JSON string of arguments, validates the arguments as well-formed JSON (falling back to {} if invalid or empty), and returns a string of the form {"name":"<escaped-name>","arguments":<arguments>} suitable for transmission.

## Remarks
It centralizes the encoding and validation of tool-call data inside Gabriel.ToolBridge. The method escapes the tool name to prevent JSON-escaping issues and uses JsonDocument.Parse to ensure the provided arguments are well-formed JSON; if not, it safely substitutes {} rather than failing. Being private, this is an internal implementation detail rather than part of the public API.

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


StreamAsync wraps an inner chat-provider to deliver a stream of ChatProviderEvent events while transparently handling tool invocations and tool-descriptor context.

It accepts a history of chat messages, a set of tool descriptors, and a model identifier, then yields events such as textual deltas, reasoning deltas, tool-call readiness, and finish signals. The wrapper injects tool documentation into the system prompt, then streams from the inner provider in two phases: a live, first pass and a fallback retry pass. In live mode, deltas are parsed by a ToolCallStreamSplitter and emitted as they arrive, exposing tool-call candidates only when complete markers are detected. If no marker is observed, the partial text is buffered and later emitted if a retry occurs. In retry mode, deltas are accumulated until a candidate tool-call marker is found; text is emitted in bulk when appropriate, ensuring tool calls are surfaced even when the underlying model doesn’t reliably expose them in real time.

The method forwards non-tool events (ReasoningDeltaEvent) unchanged and propagates any ToolCallReadyEvent produced by the inner provider. It also preserves the inner finish reason for the turn, and, if a native tool-call appears despite an empty tool list, forwards it to the caller for safety. The overall effect is a robust, tool-aware streaming surface that coordinates between the inner model, tool documentation, and tool-execution flow.

## Remarks
StreamAsync acts as a decorator around the raw inner provider, decoupling the model’s streaming quirks from the caller’s expectations about tool invocation. By injecting tool-descriptor context and by implementing a live-vs-retry strategy, it guarantees that tool usage is discoverable and correctly surfaced, even when the underlying model emits text without explicit tool-call markers. The splitting logic (ToolCallStreamSplitter) and the handling of various ChatProviderEvent subtypes work together to present a coherent, tool-aware stream to UI or orchestrator code.

## Example
```csharp
// Example usage: consume a tool-aware stream of events
await foreach (var evt in gabrielBridge.StreamAsync(history, tools, modelName, ct))
{
    switch (evt)
    {
        case TextDeltaEvent td:
            // render or accumulate textual content
            break;
        case ReasoningDeltaEvent rd:
            // display model reasoning if desired
            break;
        case ToolCallReadyEvent tcre:
            // a tool invocation has been detected and is ready to execute
            break;
        case FinishEvent fe:
            // end of this turn/stream segment
            break;
    }
}
```

## Notes
- The stream hinges on a concrete marker (<tool_call>) to identify tool-candidate text; changes to this format would require corresponding adjustments. 
- If the inner provider unexpectedly emits native tool calls while the tools list is empty, those calls are forwarded verbatim (the implementation errs on safety by honoring such calls). 
- The parameter MaxParseRetries controls how many retry attempts are attempted; higher values increase resilience to parsing quirks but add latency.

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


Translates a chat history into a wire-format representation that a non-tool-capable model can understand by embedding tool invocations inline and labeling tool results for human-readable consumption. It first builds a map from tool call IDs to tool names by scanning assistant messages that include tool calls, then constructs a translated history. For assistant messages containing tool calls, it appends inline <tool_call> blocks (serialized via SerializeCallInline) after the original content and returns a single transformed assistant message. For tool messages, it emits a user-facing line in the form "[Tool result: <name>] <content>" using the resolved tool name or "unknown" if the mapping isn’t available. All other messages are passed through unchanged. The result is an `IReadOnlyList<ChatProviderMessage>` that represents the history in a format suitable for non-tool-capable models.

---

## MaxParseRetries
> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** field

```csharp
private const int MaxParseRetries = 2
```


MaxParseRetries fixes the maximum number of parsing attempts to three total (the initial parse plus two retries). It follows the EmptyStopMaxRetries pattern used elsewhere in the agent loop, ensuring consistent retry semantics for parsing operations within GabrielToolBridge.

## Remarks
By encapsulating the limit in a private constant, this value expresses a fixed retry policy that cannot be changed at runtime. It avoids sprinkling literal numbers across parsing code and makes the relationship to other retry patterns explicit. This centralization supports predictable behavior and easier auditing of the agent's parsing flow.

## Notes
- This is a compile-time constant; it cannot be changed at runtime. If you need runtime configurability, consider making it configurable or injectable and wired through to the parsing logic.

---