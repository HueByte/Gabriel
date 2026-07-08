# Adding a new agent tool

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new agent tool

When you need to add a new capability the engine can invoke (an agent tool), follow the existing file-tool examples so the new tool is discoverable and registered alongside other tools. A new tool is typically a small, self-contained type that implements the same interface as the existing tools; model its surface and placement on an existing implementation and then add it to the composition/registration site used by the engine.

## Reference implementation

Model a new file-related tool on the [FindTool](../../Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md) implementation. The FindTool source in src/api/Gabriel.Engine/Tools/Files/FindTool.cs is the concrete example this repository uses for a file-oriented agent tool; inspect that file to mirror structure, inputs, outputs, and how it implements the tool contract.

## Where it lives

Place the new type in the src/api/Gabriel.Engine/Tools/Files folder. The existing examples in that folder are named with the Tool suffix (for example FileInfoTool, FindTool, GrepTool, ListDirTool) and are public sealed classes that implement ITool; use those files as naming and placement examples when creating your new tool type.

## Wiring

The detected registration/composition site for tools is src/api/Gabriel.Engine/DependencyInjection.cs. Add your new tool to the same registration area used by the existing file tools in that file so the engine composes the new instance at startup; inspect src/api/Gabriel.Engine/DependencyInjection.cs to see how the repository wires the current tools and follow that pattern when registering your type.

## Existing examples

- [`FileInfoTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md)
- [`FindTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md)
- [`GrepTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md)
- [`ListDirTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md)

---
*Synthesised by Aurion on 2026-07-08 05:48:02 UTC*
