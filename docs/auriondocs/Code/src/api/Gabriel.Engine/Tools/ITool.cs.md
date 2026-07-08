# ITool

> **File:** `src/api/Gabriel.Engine/Tools/ITool.cs`  
> **Kind:** interface

```csharp
public interface ITool
```


An ITool is a pluggable unit of functionality that can be discovered and invoked by name with a JSON-encoded argument payload, returning a string result. Implementations expose a Name, a human-friendly Description, a JSON Schema via ParametersJsonSchema to validate inputs, and an asynchronous ExecuteAsync to perform the work.

## Remarks
ITool provides an abstraction that decouples tool definitions from their host, enabling runtime extension via DI and a registry. It enables the host to enumerate available tools and invoke them uniformly by name, using a JSON-encoded payload defined by ParametersJsonSchema. This uniform contract makes it easy to add or swap capabilities without recompiling the host engine.

## Notes
- Ensure unique Name values across all tools; Find(string) behavior may become ambiguous if duplicates exist.
- Validate inputs against ParametersJsonSchema and respect the CancellationToken in ExecuteAsync to avoid blocking or long-running operations.

## Dependencies
- IToolRegistry

## Dependency APIs
```csharp
public interface IToolRegistry
{
    IReadOnlyList<ITool> All { get; }
    ITool? Find(string name);
    IReadOnlyList<ToolDescriptor> AsDescriptors();
}
```

## Symbol To Document
- Name: ITool
- Kind: interface
- File: src/api/Gabriel.Engine/Tools/ITool.cs
- Language: csharp
- ID: dbbd4a88-4c71-47b1-918c-1c4a7a5cf83b