using Gabriel.Core.Entities;

namespace Gabriel.Engine.Services;

// Approximate token count for context-window budgeting. Behind an interface so
// the naive char-based impl can be swapped for a real BPE tokenizer later
// without touching callers.
public interface ITokenEstimator
{
    int EstimateText(string? text);
    int EstimateMessages(IEnumerable<Message> messages);
}
