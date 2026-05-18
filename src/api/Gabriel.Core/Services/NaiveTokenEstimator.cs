using Gabriel.Core.Entities;

namespace Gabriel.Core.Services;

// Coarse char/4 approximation — good enough for context-window budgeting in a
// dev/prototype setting. Real BPE tokenization is 30-50% more accurate but
// requires shipping a tokenizer/vocab. Swap in later if accuracy matters.
public class NaiveTokenEstimator : ITokenEstimator
{
    // Per-message overhead (role marker, separators) before counting content.
    private const int MessageOverhead = 8;

    public int EstimateText(string? text)
        => string.IsNullOrEmpty(text) ? 0 : (text.Length + 3) / 4;

    public int EstimateMessages(IEnumerable<Message> messages)
    {
        var total = 0;
        foreach (var m in messages)
        {
            total += MessageOverhead;
            total += EstimateText(m.Content);
            total += EstimateText(m.ToolCallsJson);
            total += EstimateText(m.ToolCallId);
        }
        return total;
    }
}
