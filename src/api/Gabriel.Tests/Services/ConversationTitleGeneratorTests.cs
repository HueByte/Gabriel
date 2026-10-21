using System.Runtime.CompilerServices;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Gabriel.Tests.Services;

public class ConversationTitleGeneratorTests
{
    private static readonly ConversationTitleGenerator Generator = new(NullLogger<ConversationTitleGenerator>.Instance);

    [Theory]
    [InlineData("Planning a Trip to Kyoto", "Planning a Trip to Kyoto")]
    [InlineData("\"Planning a Trip to Kyoto.\"", "Planning a Trip to Kyoto")]
    [InlineData("Title: Rust Lifetimes Explained", "Rust Lifetimes Explained")]
    [InlineData("**Sourdough   Starter  Tips**", "Sourdough Starter Tips")]
    [InlineData("\n\nBudget Spreadsheet Help\nHere is why I chose it", "Budget Spreadsheet Help")]
    [InlineData("“Przepis na pierogi”", "Przepis na pierogi")]
    public void Sanitize_cleans_common_model_output(string raw, string expected)
        => Assert.Equal(expected, ConversationTitleGenerator.Sanitize(raw));

    [Theory]
    [InlineData("")]
    [InlineData("   \n  ")]
    [InlineData("\"...\"")]
    public void Sanitize_returns_null_when_nothing_usable(string raw)
        => Assert.Null(ConversationTitleGenerator.Sanitize(raw));

    [Fact]
    public void Sanitize_cuts_long_titles_on_a_word_boundary()
    {
        var title = ConversationTitleGenerator.Sanitize(string.Join(' ', Enumerable.Repeat("verbose", 20)))!;

        Assert.True(title.Length <= ConversationTitleGenerator.MaxTitleLength);
        Assert.EndsWith("verbose…", title);
    }

    [Fact]
    public async Task Generate_uses_text_channel_only()
    {
        var provider = new ScriptedProvider(
            new ReasoningDeltaEvent("The user wants a name for a cat, so…"),
            new TextDeltaEvent("Naming a "),
            new TextDeltaEvent("New Kitten"),
            new FinishEvent(FinishReason.Stop));

        var title = await Generator.GenerateAsync(provider, "m", "help me name my kitten", CancellationToken.None);

        Assert.Equal("Naming a New Kitten", title);
        Assert.Empty(provider.LastTools!);
    }

    [Fact]
    public async Task Generate_returns_null_instead_of_throwing_on_provider_failure()
    {
        var provider = new ScriptedProvider(throwWith: new HttpRequestException("503"));

        Assert.Null(await Generator.GenerateAsync(provider, "m", "hello", CancellationToken.None));
    }

    private sealed class ScriptedProvider(params ChatProviderEvent[] events) : IChatProvider
    {
        private readonly Exception? _throwWith;

        public ScriptedProvider(Exception throwWith) : this() => _throwWith = throwWith;

        public string Name => "scripted";
        public IReadOnlyList<LLMModel> Models => [];
        public IReadOnlyList<ToolDescriptor>? LastTools { get; private set; }

        public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
            IReadOnlyList<ChatProviderMessage> history,
            IReadOnlyList<ToolDescriptor> tools,
            string modelName,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            LastTools = tools;
            await Task.Yield();
            if (_throwWith is not null) throw _throwWith;
            foreach (var e in events) yield return e;
        }
    }
}
