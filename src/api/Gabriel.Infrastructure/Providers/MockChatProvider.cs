using System.Runtime.CompilerServices;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;

namespace Gabriel.Infrastructure.Providers;

// Stand-in for a real LLM. Streams a canned text reply word-by-word so the
// streaming flow is exercisable without xAI credentials, and on the first
// user turn (when tools are available) calls the first registered tool to
// demonstrate the ReAct loop.
//
// It also plays along with autonomous tasks so the whole task loop runs
// credit-free: task steps end with a task_checkpoint call (continue on step 1,
// done from step 2), and the contract-draft / step-review side calls - which
// ask for JSON - get canned JSON back.
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

        var system = history.FirstOrDefault(m => m.Role == MessageRole.System)?.Content ?? "";
        if (tools.Count == 0 && system.Contains("Reply with JSON only", StringComparison.Ordinal))
        {
            await Task.Delay(100, ct);
            yield return new TextDeltaEvent(MockJudgement(system, history));
            yield return new FinishEvent(FinishReason.Stop);
            yield break;
        }

        if (TaskCheckpointCall(history, tools) is { } checkpoint)
        {
            await Task.Delay(Random.Shared.Next(100, 300), ct);
            yield return checkpoint;
            yield return new FinishEvent(FinishReason.ToolCalls);
            yield break;
        }

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

    // A task step that hasn't reported yet: report it. "step N of" comes from
    // AgentTaskRunner's step message.
    private static ToolCallReadyEvent? TaskCheckpointCall(
        IReadOnlyList<ChatProviderMessage> history, IReadOnlyList<ToolDescriptor> tools)
    {
        if (!tools.Any(t => t.Name == "task_checkpoint")) return null;

        var lastUser = -1;
        for (var i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].Role == MessageRole.User) { lastUser = i; break; }
        }
        if (lastUser < 0) return null;

        var content = history[lastUser].Content ?? "";
        if (!content.StartsWith("[Autonomous task", StringComparison.Ordinal)) return null;
        if (history.Skip(lastUser + 1).Any(m => m.Role == MessageRole.Tool)) return null;

        var step = 1;
        var marker = content.IndexOf("step ", StringComparison.Ordinal);
        if (marker >= 0)
        {
            var digits = new string(content[(marker + 5)..].TakeWhile(char.IsDigit).ToArray());
            _ = int.TryParse(digits, out step);
        }
        var args = step >= 2
            ? """{"status":"done","progress":"[mock] finished the work","summary":"[mock] Task complete - nothing real was produced."}"""
            : $$"""{"status":"continue","progress":"[mock] made progress on step {{step}}"}""";
        return new ToolCallReadyEvent($"call_mock_{Guid.NewGuid():N}"[..24], "task_checkpoint", args);
    }

    // Canned answers for the engine's JSON side calls, told apart by the field
    // names their system prompts ask for.
    private static string MockJudgement(string system, IReadOnlyList<ChatProviderMessage> history)
    {
        if (system.Contains("doneCriteria", StringComparison.Ordinal))
        {
            var goal = history.LastOrDefault(m => m.Role == MessageRole.User)?.Content ?? "the goal";
            var objective = System.Text.Json.JsonSerializer.Serialize("[mock] " + goal);
            return $$"""{"objective": {{objective}}, "doneCriteria": ["[mock] the agent reports the work done"], "constraints": []}""";
        }
        return """{"progress": true, "done": true, "feedback": "[mock] looks complete"}""";
    }
}
