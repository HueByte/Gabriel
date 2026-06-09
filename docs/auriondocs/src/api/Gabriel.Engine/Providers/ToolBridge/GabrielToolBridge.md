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

Bridges a tool-incapable chat provider to the IChatProvider tool-calling surface by rewriting message history, injecting tool documentation into the system prompt, streaming the inner provider's text while detecting inline <tool_call> markers, and synthesising tool-call events the agent loop expects. Use this when you need an existing local or limited model (one that cannot perform function/tool calls natively) to participate in an agent-driven tool-calling workflow without changing the agent loop or UI.

## Remarks
This decorator emulates a native tool-calling provider so higher-level agent logic (RunStreamAsync, AgentContext, UI) can remain unchanged. It performs three conceptual steps: rewrite prior structured tool calls into inline markers and convert tool-role results into user messages; append a system message that describes available tools and the wire format; and stream the inner provider's output through a splitter that yields live text until it encounters a <tool_call> marker, buffering the remainder to be parsed after the stream completes. On successful parse the bridge emits ToolCallReadyEvents and then ToolCalls; on parse failure it will append a corrective user message and retry the inner provider up to MaxParseRetries (2) times. The bridge does not execute tools itself — actual tool execution remains the responsibility of AgentService.ExecuteToolSafelyAsync/ITool.ExecuteAsync — it only converts the wire protocol into the event shape the agent expects. The decorator preserves the inner provider's identity (Name and Models) and intentionally calls the inner provider with an empty tools list because tool descriptors are delivered via the injected system message.

## Notes
- Live streaming is performed only on the first attempt. If parsing fails and the bridge retries, the retry output is not streamed (to avoid "unsending" already-yielded text); the retry will only emit corrected tool calls.
- The inner provider is invoked with an empty tools list; tool behaviour must therefore be described in the injected system message or the provider will not receive descriptors via the sibling tools field.
- If the wrapped provider unexpectedly exposes native tool calls, the bridge will forward them unchanged — this can happen if the underlying provider actually supports tools and the decorator is redundant; forwarding is safer than dropping them.
- The class intentionally does not run tools — it only synthesises the same event shape used by native tool-calling providers so server-side code can perform execution as usual.

---

## GabrielToolBridge

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** constructor

Creates a new GabrielToolBridge that wraps an IChatProvider implementation and an ILogger for GabrielToolBridge. Reach for this constructor when you need an adapter/bridge object that delegates chat-provider work to an inner IChatProvider while emitting logs using the provided ILogger.

## Remarks
This constructor simply captures the two dependencies — the inner IChatProvider and the `ILogger<GabrielToolBridge>` — into private fields for use by the bridge's instance methods. It is intended for use in dependency-injection or manual composition scenarios where the bridge composes an existing chat provider and a logger instead of creating those dependencies itself.

## Example
```csharp
// Manual instantiation
IChatProvider innerProvider = new SomeChatProvider(...);
ILogger<GabrielToolBridge> logger = loggerFactory.CreateLogger<GabrielToolBridge>();
var bridge = new GabrielToolBridge(innerProvider, logger);

// Typical DI registration (ASP.NET Core)
services.AddTransient<GabrielToolBridge>(sp =>
    new GabrielToolBridge(
        sp.GetRequiredService<IChatProvider>(),
        sp.GetRequiredService<ILogger<GabrielToolBridge>>()));
```

## Notes
- The constructor does not perform null checks; callers (or the DI container) should ensure non-null arguments to avoid NullReferenceException when the bridge uses these fields.

---

## Models

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

Returns the collection of LLM models exposed by the underlying bridge implementation. Use this property to enumerate which models are available through this GabrielToolBridge instance.

## Remarks
This property is a thin pass-through to the inner provider's Models collection (_inner.Models). It surfaces the same read-only view that the inner implementation exposes so callers can discover supported LLMModel instances without reaching into the inner object directly.

## Example
```csharp
// Iterate available models and print a simple representation
foreach (var model in bridge.Models)
{
    Console.WriteLine(model);
}
```

## Notes
- IReadOnlyList&lt;LLMModel&gt; does not guarantee the underlying collection is immutable; the underlying provider may modify the list, so the contents can change over time.
- Enumeration may observe concurrent changes if the inner collection is mutated by another thread; callers should synchronize if a stable snapshot is required.

---

## Name

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** property

Returns the canonical provider name by delegating to the wrapped (inner) provider. Use this when you need the provider's identity for things like registry lookup — the bridge does not introduce a new name, it passes the inner provider's name through.

## Remarks
This property exists because GabrielToolBridge is a decorator around a concrete provider: identity and model entries remain owned by the underlying provider. Exposing the inner provider's Name ensures the agent registry and other resolution logic continue to operate against the base provider's identity rather than the wrapper's.

## Example
```csharp
// Accessing the provider name through the bridge for registry resolution
string providerName = bridge.Name;
var resolved = registry.Resolve(providerName);
```

## Notes
- The property is read-only and simply returns _inner.Name; any change to the inner provider's Name (if mutable) will be reflected here at access time.
- Do not rely on the bridge having a distinct identity from the wrapped provider; registry, model ownership, and lookups use the inner provider's name.
- This delegation means renaming the bridge object itself has no effect on registry resolution — only the inner provider's Name matters.

---

## AppendFixupMessage

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Appends two messages to an existing chat history after a tool-call parse failure so the model can be retried with clearer context. The returned history contains (1) an assistant message with the raw model output that failed to parse, and (2) a user message quoting the offending block, the parse error, and explicit instructions to re-emit a valid <tool_call>{"name":"...","arguments":{...}}</tool_call> block (the "arguments" must be a JSON object and the block must not be wrapped in code fences).

## Remarks
This helper centralizes the retry fix-up logic so callers can re-stream the model with an augmented conversation that exposes the model to its own broken output and a precise corrective prompt. By appending both an assistant message (so the model sees what it previously produced) and a user message (which requests a corrected response), the retry has a higher chance of producing a well-formed tool call without changing earlier history entries.

## Notes
- The original history is copied into a new list; the method does not mutate the provided IReadOnlyList.
- Repeated retries will grow the returned history; callers should consider bounding retries or trimming history to avoid unbounded growth.
- The method embeds ex.OffendingBlock and ex.Message verbatim into the user prompt — be cautious if those values may contain sensitive data.
- The instructions require the tool call's "arguments" to be a JSON object and explicitly disallow code fences; nothing in this method enforces those constraints beyond the textual instruction.

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
| `tools` | ``IReadOnlyList<ToolDescriptor>`` | — |

**Returns:** `string`


Builds a plain-text instructions block that describes the tool-calling protocol and enumerates the provided tools. Use this when composing a prompt or instructions for an agent so it knows how to invoke tools: the method returns the exact text that should be inserted into the prompt to explain the required <tool_call> block format and to list each tool's name, description and parameter schema.

## Remarks
This centralizes the agent-facing documentation for tool invocation so all prompts use a single, consistent format. It emits a short set of rules (including the exact <tool_call> JSON markup the agent must use), guidance about how tool results will be returned, and then iterates the provided ToolDescriptor list to print each tool's Name, Description and ParametersJsonSchema. The resulting string is suitable for inclusion directly in a prompt template.

## Example
```csharp
// Example fragment produced by BuildToolDocs
[Tool calling]
You have access to the following tools. To use one, emit a block in this exact format, inline anywhere in your response:

<tool_call>{"name":"<tool_name>","arguments":{...}}</tool_call>

Rules:
- The block must contain valid JSON. The "arguments" field must be a JSON object - use {} if the tool takes no arguments.
...

Available tools:

- lookup: Look up a symbol by name and return its metadata.
  Parameters: { "name": "string" }

- runJob: Execute a background job.
  Parameters: { "jobId": "string", "payload": { ... } }
```

## Notes
- The method does not guard against a null tools argument; passing null will throw a NullReferenceException.
- ToolDescriptor fields (Name, Description, ParametersJsonSchema) are written verbatim; multi-line schema values will be inserted as-is and can affect formatting.
- The returned string is TrimEnd()'d so no trailing newline is present.

---

## EscapeJsonString

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Escapes backslashes and double quotes so a C# string can be embedded into a JSON string literal. Reach for this small helper when constructing JSON text by hand and you only need to ensure backslashes and double quotes are escaped (instead of using a full JSON serializer).

## Remarks
This is an intentionally minimal helper: it replaces backslashes first and then double quotes to avoid double-escaping. It does not perform full JSON string encoding (for example, it doesn't escape control characters or emit \uXXXX escapes), so it's suitable for simple, controlled scenarios where those extra escapes are unnecessary.

## Example
```csharp
var raw = "C:\\temp\\file\"name\"";
var escaped = EscapeJsonString(raw);
var json = $"{\"path\": \"{escaped}\"}"; // -> {"path": "C:\\temp\\file\"name\""}
```

## Notes
- Passing null will cause a NullReferenceException because the method calls instance Replace on the input string.
- Does not escape control characters (\n, \r, \t, etc.) or produce unicode (\u) escapes; use System.Text.Json or Newtonsoft.Json for robust, standards-compliant JSON encoding.
- Designed as a small convenience for manual serialization; prefer a JSON library for user input or security-sensitive contexts.


---

## InjectToolDocs

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Inserts a system-role ChatProviderMessage containing rendered tool descriptors into an existing conversation history. Use this when you need the model to be aware of available tools (their names, descriptions and JSON schemas) while keeping the original conversation order and leading system messages intact.

## Remarks
This helper creates a single system message (via BuildToolDocs) that documents the provided tools and inserts it immediately after any leading system messages and before the first non-system message. The intent is to keep tool documentation part of the system-message block so it remains logically a part of the model's system prompt (and cacheable with other system content) rather than becoming an interleaved user/assistant message.

## Example
```csharp
// Given an existing conversation history and a list of tool descriptors:
IReadOnlyList<ChatProviderMessage> history = /* existing conversation */;
IReadOnlyList<ToolDescriptor> tools = /* tool descriptors to expose to the model */;

// Inject a system message describing the tools into the history:
var updatedHistory = InjectToolDocs(history, tools);

// Pass updatedHistory to the chat provider so the model sees the tool docs
```

## Notes
- If tools.Count is 0 the original history is returned unchanged.
- The insertion point is the first non-system message; if the history is all system messages the docs message is appended after them.
- The method returns a new list containing the existing messages plus the inserted docs message; it does not mutate the input list.
- Calling this repeatedly (without deduplication) will insert multiple tool-doc messages; callers should avoid duplicate insertions if that is undesirable.

---

## SerializeCallInline

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Creates a compact JSON representation of a tool call suitable for inlining in assistant output. Use this when you have a tool name and a JSON-formatted arguments string and need a safe, single-line object like { "name": "...", "arguments": {...} } — the method ensures the name is escaped for JSON and that arguments are valid JSON, falling back to an empty object if not.

## Remarks
This helper centralizes the small-but-important concerns of embedding a tool invocation into assistant-generated text: it escapes the tool name, validates that the arguments are valid JSON, and guarantees the resulting string is a well-formed JSON object. The method is defensive — empty or invalid argument strings are replaced with an empty JSON object ({}), avoiding malformed output that could break downstream consumers.

## Example
```csharp
// Typical valid usage
var json = SerializeCallInline("search", "{\"q\":\"latest news\",\"limit\":5}");
// json => {"name":"search","arguments":{"q":"latest news","limit":5}}

// If argumentsJson is empty or invalid it falls back to an empty object
var json2 = SerializeCallInline("notify", "not a json");
// json2 => {"name":"notify","arguments":{}}
```

## Notes
- argumentsJson is expected to be a JSON value (typically an object). If it's null, whitespace, or invalid JSON a fallback of {} is used.
- The tool name is escaped via EscapeJsonString to avoid producing invalid JSON or injection through the name.
- The method does not perform schema or content validation of the arguments; it only checks for syntactic JSON validity.
- The implementation is side-effect free and safe to call concurrently.

---

## StreamAsync

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Streams chat provider events while detecting and handling embedded tool-call markers (<tool_call>). Use this when you need a streaming chat response that may contain tool-invocation instructions: the method forwards incremental text and reasoning deltas from an inner provider, detects tool-call markers inserted into the text, and emits tool-call readiness events instead of raw marker text. It performs a live first attempt (streaming deltas as they arrive) and then up to MaxParseRetries additional attempts that buffer output and emit only corrected tool-call events so previously-streamed text is not retracted.

## Remarks
This method is a decorator around an inner chat provider that bridges free-form model output into the structured tool-invocation protocol. Tool descriptors are injected into the system prompt before calling the inner provider, so the inner provider is invoked with an empty tools list; the bridge uses a ToolCallStreamSplitter to detect <tool_call> markers in the text channel and to separate normal textual deltas from candidate tool-call payloads. The first attempt streams live so users see immediate partial responses; subsequent retry attempts buffer textual output and only surface corrected tool-call events, because already-streamed text cannot be retracted.

## Example
```csharp
await foreach (var evt in bridge.StreamAsync(history, tools, modelName, ct))
{
    switch (evt)
    {
        case TextDeltaEvent t: Console.Write(t.Delta); break;
        case ReasoningDeltaEvent r: /* display internal chain-of-thought */ break;
        case ToolCallReadyEvent tc: /* invoke tool described by tc.Descriptor with tc.Input */ break;
        case FinishEvent f: Console.WriteLine($"Finished: {f.Reason}"); break;
    }
}
```

## Notes
- The live first attempt cannot be retracted: any text emitted during attempt 0 is final and retries only emit corrected tool-call events (and buffered text as a single bulk delta when necessary).
- The inner provider is called with an empty tools list because tool descriptors are placed into the prompt by InjectToolDocs; if the inner provider nevertheless emits native ToolCallReadyEvent those are forwarded unchanged.
- The method forwards ReasoningDeltaEvent unchanged — some providers expose an internal reasoning channel that should be preserved.
- The method honors cancellation via the provided CancellationToken (EnumeratorCancellation) on the async enumerable.

---

## TranslateHistory

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** method

Rewrites a tool-aware chat history into a plain-text form that an inner (non-tool-capable) model can consume. Assistant messages that include structured ToolCalls are converted into regular assistant content with inline <tool_call>...</tool_call> blocks (produced by SerializeCallInline) and have their ToolCalls cleared; Tool messages are converted into user messages whose content is prefixed with "[Tool result: <name>] ..." where <name> is recovered by matching the ToolCallId to a previously seen assistant tool call. Messages that are neither assistant-with-tool-calls nor tool messages are passed through unchanged.

## Remarks
This method first does a forward scan of the history to build a mapping from tool-call IDs to tool names so that subsequent Tool messages can be labeled with the correct tool name. The conversion is intentionally lossy: structured ToolCalls and ToolCallId fields are removed so the resulting list looks like a normal text-only conversation that older or simpler providers expect. Use this when bridging between a tool-capable front-end and an inner provider that only understands plain assistant/user message content.

## Example
```csharp
// Input (conceptual): assistant issues a tool call, later the tool returns a message
var history = new List<ChatProviderMessage>
{
    new ChatProviderMessage(Role: MessageRole.Assistant, Content: "Looking up...", ToolCalls: new[] { new ToolCall(Id: "call-1", Name: "search", ArgumentsJson: "{\"q\":\"x\"}" ) }),
    new ChatProviderMessage(Role: MessageRole.Tool, Content: "Results found", ToolCallId: "call-1")
};

var translated = TranslateHistory(history);

// Result (conceptual):
// translated[0].Role == MessageRole.Assistant
// translated[0].Content == "Looking up...\n<tool_call>...serialized call...</tool_call>"
// translated[1].Role == MessageRole.User
// translated[1].Content == "[Tool result: search] Results found"
```

## Notes
- The translation is lossy: ToolCalls and ToolCallId metadata are cleared; if you need structured tool metadata later you must retain it before calling this method.
- If a Tool message references a ToolCallId that wasn't seen in an earlier assistant message, the tool name falls back to "unknown".
- Null or empty Content is handled safely; trailing newlines added during serialization are trimmed from assistant content.

---

## MaxParseRetries

> **File:** `src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs`  
> **Kind:** field

Controls how many times parsing is retried after the initial attempt. The constant value (2) represents the number of retries, which results in up to three parse attempts total (initial try + 2 retries). Use this to bound transient parse failures and avoid indefinite retry loops.

## Remarks
This constant mirrors the "EmptyStopMaxRetries" pattern used elsewhere in the agent loop to keep retry behavior consistent across components. It exists as a single source of truth for parse retry policy so related logic can rely on the same retry limits.

## Notes
- The value is the number of retries, not the total attempts; value 2 means three attempts in total (initial + 2 retries).
- It's a compile-time constant; changing it requires a rebuild and may need corresponding updates where other retry limits are defined to remain consistent.

---