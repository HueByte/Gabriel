using Gabriel.Core.Providers;

namespace Gabriel.Core.Tools;

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _byName;

    public IReadOnlyList<ITool> All { get; }

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        All = tools.ToList();
        _byName = All.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
    }

    public ITool? Find(string name) => _byName.GetValueOrDefault(name);

    public IReadOnlyList<ToolDescriptor> AsDescriptors()
        => All.Select(t => new ToolDescriptor(t.Name, t.Description, t.ParametersJsonSchema)).ToList();
}
