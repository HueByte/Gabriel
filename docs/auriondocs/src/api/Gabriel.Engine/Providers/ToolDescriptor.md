# ToolDescriptor

> **File:** `src/api/Gabriel.Engine/Providers/ToolDescriptor.cs`  
> **Kind:** record

A lightweight, immutable representation of a provider-facing tool: its name, a short human-readable description, and a raw JSON Schema string that describes the tool's parameter shape. Reach for this record when exposing or serializing tool metadata to the engine or model (for example, when registering tools with a provider API or sending tool definitions to a model that expects a parameters schema).

## Remarks
This record is a simple DTO used to carry tool metadata between provider code and the engine/model layer. The ParametersJsonSchema property contains the JSON Schema text for the tool's arguments and is passed through to downstream components as-is; ToolDescriptor does not parse or validate that schema. Being a C# record, it provides value-based equality, concise construction/deconstruction, and is suitable for serialization.

## Example
```csharp
using System.Text.Json;

var schema = @"{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"}},\"required\":[\"query\"]}";
var tool = new ToolDescriptor(
    Name: "DocumentSearch",
    Description: "Searches indexed documents for a query string.",
    ParametersJsonSchema: schema
);

// Serialize to JSON for transmission to the model/provider
var json = JsonSerializer.Serialize(tool);
```

## Notes
- ParametersJsonSchema is a raw JSON Schema string and is not validated by this type — ensure it is valid JSON Schema before creating the record.  
- Because the schema is embedded as a string, take care with escaping when constructing literals or embedding schemas in source code.  
- Large or deeply nested schemas will increase message size when serializing; consider referencing or compressing schemas if size is a concern.