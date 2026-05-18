using Gabriel.Core.Providers;

namespace Gabriel.Core.Tools;

public interface IToolRegistry
{
    IReadOnlyList<ITool> All { get; }
    ITool? Find(string name);

    // Projects the registry into the provider-facing descriptor list. The
    // result is what gets sent to the LLM as the `tools` parameter.
    IReadOnlyList<ToolDescriptor> AsDescriptors();
}
