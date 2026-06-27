# ToolDescriptor

> **File:** `src/api/Gabriel.Engine/Providers/ToolDescriptor.cs`  
> **Kind:** record

Represents the provider-facing metadata for a tool: an immutable, value-type record containing the tool's name, a short description, and the raw JSON Schema that describes the tool's parameters. Use this when publishing or transferring tool metadata to a model or provider component that expects a compact, serializable descriptor.

## Remarks
This record intentionally stores ParametersJsonSchema as an opaque string; parsing, validation, or interpretation of that JSON Schema is the responsibility of the consumer. Using a simple record keeps the shape lightweight and suitable for serialization, equality checks, and deconstruction in higher-level plumbing (for example, registries or provider APIs that enumerate available tools).

## Example
```csharp
// Create a descriptor for a simple 'translate' tool
var translateSchema = "{ \"type\": \"object\", \"properties\": { \"text\": { \"type\": \"string\" } }, \"required\": [\"text\"] }";
var descriptor = new ToolDescriptor(
    Name: "translate",
    Description: "Translates input text into a target language",
    ParametersJsonSchema: translateSchema
);

// Read values
Console.WriteLine(descriptor.Name); // "translate"

// Make an updated copy using record's with-expression
var updated = descriptor with { Description = "Translates text between languages" };
```

## Notes
- ParametersJsonSchema is stored as raw JSON text; this type does not validate or parse it — callers must ensure it is a valid JSON Schema if required.
- ToolDescriptor is a record (value semantics). Instances are immutable by default but can be cloned with the with-expression to produce modified copies.