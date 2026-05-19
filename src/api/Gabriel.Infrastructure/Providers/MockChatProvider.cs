using System.Runtime.CompilerServices;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;

namespace Gabriel.Infrastructure.Providers;

// Stand-in for a real LLM. Streams a canned text reply word-by-word so the
// streaming flow is exercisable without xAI credentials, and on the first
// user turn (when tools are available) calls the first registered tool to
// demonstrate the ReAct loop.
public class MockChatProvider : IChatProvider
{
    public string Name => "mock";

    // Exposed to IModelCatalog so the UI picker shows a "mock" option in dev.
    // Small context window so compact behavior is exercisable in dev with a
    // few chatty turns. Not IsDefault — the real provider's IsActive entry
    // wins the bootstrap when one is configured.
    public IReadOnlyList<LLMModel> Models { get; } = new List<LLMModel>
    {
        new()
        {
            Name = "mock-default",
            IsActive = false,
            ContextWindowTokens = 8_000,
        },
    };

    private static readonly string[] Templates =
    [
        "[mock] Got it - you said: \"{0}\". Plugging in a real provider will replace this.",
        "[mock] Interesting. \"{0}\" - let me think about that. (This is a placeholder reply.)",
        "[mock] I hear you on \"{0}\". Once a real provider is wired up I'll have something smarter to say.",
        "[mock] \"{0}\" - noted. Mock provider here; swap me out via DI for the real one.",
    ];

    public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        string modelName,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Mock ignores the model name — every entry in Models routes to the
        // same canned-reply behavior. Param exists for interface compliance.
        _ = modelName;
        // If we've never run a tool in this conversation and at least one is registered,
        // fire it once to prove the agent loop. Subsequent turns answer with text.
        var hasToolResult = history.Any(m => m.Role == MessageRole.Tool);

        if (!hasToolResult && tools.Count > 0)
        {
            await Task.Delay(Random.Shared.Next(200, 500), ct);
            var first = tools[0];
            yield return new ToolCallReadyEvent(
                Id: $"call_mock_{Guid.NewGuid():N}"[..24],
                Name: first.Name,
                ArgumentsJson: "{}");
            yield return new FinishEvent(FinishReason.ToolCalls);
            yield break;
        }

        var lastUser = history.LastOrDefault(m => m.Role == MessageRole.User)?.Content ?? "(no prompt)";
        var snippet = lastUser.Length > 80 ? lastUser[..80] + "..." : lastUser;
        var template = Templates[Random.Shared.Next(Templates.Length)];
        var reply = string.Format(template, snippet);

        foreach (var word in reply.Split(' '))
        {
            await Task.Delay(40, ct);
            yield return new TextDeltaEvent(word + " ");
        }

        yield return new FinishEvent(FinishReason.Stop);
    }
}
