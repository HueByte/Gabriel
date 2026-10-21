using System.Text;
using System.Text.RegularExpressions;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;
using Microsoft.Extensions.Logging;

namespace Gabriel.Engine.Services;

public interface IConversationTitleGenerator
{
    // Returns a short display title for a conversation opened with
    // `firstUserMessage`, or null when the model gave nothing usable. Never
    // throws: a missing title must not fail the turn it rides along with.
    Task<string?> GenerateAsync(
        IChatProvider provider,
        string modelName,
        string firstUserMessage,
        CancellationToken ct);
}

// One small side call to the turn's own provider/model. AgentService starts it
// in parallel with the main turn, so on any normal reply the title is ready
// before the answer finishes streaming and adds no latency.
public sealed partial class ConversationTitleGenerator : IConversationTitleGenerator
{
    public const int MaxTitleLength = 60;

    // Bounds how long a slow title call can hold back the turn's `done` event.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    // The opening message is all the model needs; capping it keeps a pasted
    // document from turning the title call into an expensive one.
    private const int MaxInputChars = 2_000;

    private const string SystemPrompt =
        "You write titles for chat conversations. Given the user's opening message, reply with a title " +
        "of 2 to 6 words that names the topic. Use the same language as the message. Reply with the " +
        "title only: no quotes, no trailing punctuation, no prefix like 'Title:'.";

    private readonly ILogger<ConversationTitleGenerator> _logger;

    public ConversationTitleGenerator(ILogger<ConversationTitleGenerator> logger) => _logger = logger;

    public async Task<string?> GenerateAsync(
        IChatProvider provider,
        string modelName,
        string firstUserMessage,
        CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(Timeout);

        var input = firstUserMessage.Length > MaxInputChars ? firstUserMessage[..MaxInputChars] : firstUserMessage;
        var history = new List<ChatProviderMessage>
        {
            new(MessageRole.System, SystemPrompt),
            new(MessageRole.User, input),
        };

        try
        {
            // Text deltas only - reasoning models stream their thinking on a
            // separate channel that must not leak into the title.
            var text = new StringBuilder();
            await foreach (var evt in provider.StreamAsync(history, [], modelName, timeout.Token))
            {
                if (evt is TextDeltaEvent td) text.Append(td.Delta);
                else if (evt is FinishEvent) break;
            }

            var title = Sanitize(text.ToString());
            if (title is null)
                _logger.LogWarning("Title generation returned nothing usable | provider={Provider} model={Model}", provider.Name, modelName);
            return title;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Title generation timed out after {Seconds}s | provider={Provider} model={Model}",
                Timeout.TotalSeconds, provider.Name, modelName);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Title generation failed | provider={Provider} model={Model}", provider.Name, modelName);
            return null;
        }
    }

    // Models ignore "title only" often enough that the output needs cleaning:
    // first non-empty line, no "Title:" label, no wrapping quotes/markdown, no
    // trailing punctuation, whitespace collapsed, cut on a word boundary.
    public static string? Sanitize(string raw)
    {
        var line = raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        if (line is null) return null;

        line = TitleLabel().Replace(line, "");
        line = line.Trim(' ', '"', '\'', '`', '*', '#', '_', '“', '”', '‘', '’', '«', '»');
        line = line.TrimEnd('.', '!', '?', ':', ';', ',', '。');
        line = Whitespace().Replace(line, " ").Trim();
        if (line.Length == 0) return null;

        if (line.Length > MaxTitleLength)
        {
            var cut = line.LastIndexOf(' ', MaxTitleLength - 1);
            line = (cut > MaxTitleLength / 2 ? line[..cut] : line[..(MaxTitleLength - 1)]).TrimEnd() + "…";
        }
        return line;
    }

    [GeneratedRegex(@"^\W*title\s*[:\-–—]\s*", RegexOptions.IgnoreCase)]
    private static partial Regex TitleLabel();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
