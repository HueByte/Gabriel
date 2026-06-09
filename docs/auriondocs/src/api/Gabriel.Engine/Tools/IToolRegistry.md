# IToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/IToolRegistry.cs`  
> **Kind:** interface

Represents a read-only registry of available tools: use it to enumerate all registered tools, look up a tool by name at runtime, or produce the provider-facing list of ToolDescriptor objects that should be sent to a language model as the `tools` parameter.

## Remarks
This interface centralizes tool discovery and the projection that is exposed to an LLM provider. Implementations act as the authoritative source of which tools exist and what metadata is safe to expose; AsDescriptors() returns a provider-facing view and may filter or redact internal-only information. Consumers should use All/Find for runtime resolution and AsDescriptors when constructing requests for the provider.

## Example
```csharp
// enumerate tools
IReadOnlyList<ITool> all = registry.All;

// lookup by name (may return null)
ITool? calc = registry.Find("calculator");
if (calc != null)
{
    // invoke or inspect the tool
}

// get the descriptors to send to the LLM/provider
IReadOnlyList<ToolDescriptor> descriptors = registry.AsDescriptors();
// pass `descriptors` to the provider client as the `tools` parameter
```

## Notes
- Find(string) returns null when no matching tool exists; callers must handle that case.
- The All property is IReadOnlyList — do not attempt to modify the returned collection.
- The name-matching semantics for Find (case-sensitivity, aliases, normalization) are not specified by this interface and depend on the implementation.