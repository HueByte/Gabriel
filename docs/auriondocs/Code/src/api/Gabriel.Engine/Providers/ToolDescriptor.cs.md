# ToolDescriptor

> **File:** `src/api/Gabriel.Engine/Providers/ToolDescriptor.cs`  
> **Kind:** record

```csharp
public record ToolDescriptor(string Name, string Description, string ParametersJsonSchema)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Name`](ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `Description` | `string` | — |
| `ParametersJsonSchema` | `string` | — |


ToolDescriptor is a provider-facing, serializable descriptor for a tool exposed by the engine. It combines a human-friendly Name, a Description of the tool, and a ParametersJsonSchema that defines the tool's argument shape as a JSON Schema. Because ToolDescriptor is a C# record, it is immutable and participates in value-based equality, making it ideal for use as metadata across discovery, UI rendering, and request construction without coupling to the tool's implementation.

## Remarks
ToolDescriptor serves as the contract between tool implementations and the surfaces that present or invoke them. It decouples metadata from behavior: consumers can enumerate tools, show labels, and validate input purely from this descriptor, while the actual invocation logic remains in separate components. The ParametersJsonSchema acts as a lightweight, language-agnostic description of accepted parameters, enabling dynamic forms and input validation by models without inspecting tool internals. The immutability of ToolDescriptor ensures safe sharing across threads and boundaries.

## Notes
- The ParametersJsonSchema must be a valid JSON Schema text; invalid content is a runtime error for tooling components.
- This descriptor is purely metadata; do not embed executable code or environment-sensitive data in Name/Description; keep sensitive details out.
- As a record, equality is structural; to update metadata, create a new instance rather than mutating.