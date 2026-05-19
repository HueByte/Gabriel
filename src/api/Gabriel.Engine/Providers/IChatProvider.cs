using Gabriel.Core.Configuration;

namespace Gabriel.Engine.Providers;

// Streaming abstraction over LLM providers (mock, Grok/xAI, OpenAI, etc).
// Providers yield a sequence of events (text deltas, completed tool calls,
// finish) so the agent loop can react incrementally - important for ReAct
// flows where every tool call would otherwise block the UI for seconds.
//
// Model selection is per-call: the agent resolves the user's PreferredModel
// (or the config default) into a model name and passes it here. The provider
// is otherwise stateless wrt the choice.
public interface IChatProvider
{
    // Stable identifier used by IChatProviderRegistry.Resolve and stored on
    // ApplicationUser.PreferredProvider.
    string Name { get; }

    // Catalog of models this provider can serve. Returned to the UI by
    // IModelCatalog. Empty for providers that aren't meant to show up in
    // the picker.
    IReadOnlyList<LLMModel> Models { get; }

    IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        string modelName,
        CancellationToken ct = default);
}
