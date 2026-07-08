# ToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/ToolRegistry.cs`  
> **Kind:** class

```csharp
public class ToolRegistry : IToolRegistry
```


ToolRegistry centralizes access to all ITool implementations and provides fast, case-insensitive lookup by tool name. Use it when you need to discover available tools or resolve a tool by name without depending on concrete collections. It also exposes a read-only snapshot of tools (All) and a metadata surface (ToolDescriptor) via AsDescriptors for tooling and UIs.

## Remarks
By encapsulating the lookup and descriptor logic, ToolRegistry decouples consumers from the details of how tools are stored or described. It provides a single, testable surface for discovery that can be swapped or extended without affecting call sites. It assumes unique, case-insensitive tool names; duplicates will fail at construction. Additionally, AsDescriptors iterates over All to produce ToolDescriptor records, ensuring a stable, serializable description surface for downstream consumers.

## Notes
- Duplicate tool names (case-insensitive) cause construction to fail. Ensure tool names are unique regardless of case.
- Find returns null when no tool matches the provided name.
- All is captured at construction time and exposed as a read-only list; it is not intended for runtime mutation.