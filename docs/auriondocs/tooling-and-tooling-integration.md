# Tooling and Tooling Integration

> Tooling bridge between chat providers and the tool invocation surface, plus tooling discovery.

This guide explains the pieces that connect a chat provider that cannot natively call tools to the tool-invocation surface, and how tools are discovered and resolved. It focuses on the bridge that adapts providers, the parser that extracts tool-call fragments from streaming text, the registry that holds available tools, and the ITool contract that implementations must satisfy.

## GabrielToolBridge.cs
Bridges a tool-incapable chat provider to tool-calling surface.

GabrielToolBridge adapts an inner chat provider that cannot directly emit structured tool-invocation messages into the IChatProvider tool-calling surface by rewriting message history, injecting tool documentation into the system prompt, streaming the inner provider's text, and producing outputs that downstream logic can interpret as tool calls. In practice this component is the entry point when you need to present tools to a model that only produces text: it prepares the prompt (using tool metadata from the registry) and surfaces the provider's streamed textual output so other components can parse and act on embedded <tool_call> fragments. See [GabrielToolBridge.cs](Code/src/api/Gabriel.Engine/Providers/ToolBridge/GabrielToolBridge.cs.md) for details.

## ToolCallBlockParser.cs
Parses <tool_call> blocks from streamed text into parsed tool invocations.

ToolCallBlockParser is the focused utility that scans a buffered text tail and extracts <tool_call>...</tool_call> blocks, returning ParsedToolCall instances you can use to drive actual invocations. Use this when you receive a string produced by a streaming splitter or the bridge component and need a deterministic, recoverable representation of the tool invocation encoded in text. It sits downstream of the streaming output produced by the bridge and provides the structured pieces that the runtime will match against registered tools. See [ToolCallBlockParser.cs](Code/src/api/Gabriel.Engine/Providers/ToolBridge/ToolCallBlockParser.cs.md) for implementation notes.

## ToolRegistry.cs
Stores and provides fast lookup of registered ITool implementations.

ToolRegistry is the authoritative in-process catalog of available tools: it stores ITool implementations, provides fast case-insensitive lookup by name, and can project registry entries into lightweight ToolDescriptor objects for injection into prompts or metadata surfaces. The bridge consults the registry to assemble the tool documentation it injects into prompts, and the runtime consults it to resolve a ParsedToolCall into the actual ITool to invoke. See [ToolRegistry.cs](Code/src/api/Gabriel.Engine/Tools/ToolRegistry.cs.md) for the public API.

## ITool.cs
Represents an interface for tools available to agents, including schema.

ITool defines the contract each tool implementation must satisfy: it exposes a JSON Schema describing the tool's argument shape (ParametersJsonSchema) and a way to execute the tool and produce an observation string. ToolRegistry stores implementations of this interface and the bridge uses the schema/descriptor to surface usage information to models. When a ParsedToolCall is extracted from a stream, the runtime resolves the call to an ITool from the registry and validates/dispatches according to the declared schema. See [ITool.cs](Code/src/api/Gabriel.Engine/Tools/ITool.cs.md) for details on the interface.

These components form a simple request flow: the GabrielToolBridge adapts a text-only provider and injects tool descriptors drawn from ToolRegistry (which holds ITool implementations); the provider's streamed output can include <tool_call> fragments that ToolCallBlockParser extracts into ParsedToolCall objects; those parsed calls are resolved back to ITool implementations from the ToolRegistry for validation and invocation. Together they let a text-producing model participate in a tool-enabled runtime without the provider itself needing native tooling support.

---
*Synthesised by Aurion on 2026-06-09 03:23:35 UTC*
