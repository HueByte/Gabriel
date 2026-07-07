# ToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/ToolRegistry.cs`  
> **Kind:** class

```csharp
public class ToolRegistry : IToolRegistry
```


ToolRegistry provides an in-memory catalog of ITool implementations and exposes a fast, case-insensitive lookup by name via Find, plus a convenient projection of tools into ToolDescriptor metadata via AsDescriptors.

## Remarks
It materializes All from the provided tools and builds a case-insensitive index by Name for quick lookup; the design intentionally keeps a snapshot that won't reflect subsequent changes to the input collection. AsDescriptors returns lightweight metadata by projecting the essential fields (Name, Description, ParametersJsonSchema), decoupling consumers from the concrete ITool implementations and enabling UI or tooling to list or describe available tools.
