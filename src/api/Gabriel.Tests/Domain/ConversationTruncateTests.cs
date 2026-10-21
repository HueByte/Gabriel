using Gabriel.Core.Entities;
using Xunit;

namespace Gabriel.Tests.Domain;

public class ConversationTruncateTests
{
    // TruncateFrom orders by CreatedAt (UtcNow); spacing appends keeps two
    // messages from sharing a timestamp and blurring the cut.
    private static Message Append(Func<Message> append)
    {
        Thread.Sleep(3);
        return append();
    }

    private static (Conversation Conv, Message User1, Message Reply1, Message User2) TwoTurns()
    {
        var conv = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());
        var user1 = Append(() => conv.AppendUserMessage("first question"));
        var reply1 = Append(() => conv.AppendAssistantText("first answer"));
        var user2 = Append(() => conv.AppendUserMessage("second question"));
        Append(() => conv.AppendAssistantText("second answer"));
        return (conv, user1, reply1, user2);
    }

    [Fact]
    public void Truncating_after_the_compaction_cut_keeps_the_summary()
    {
        var (conv, _, reply1, user2) = TwoTurns();
        conv.UpdateSummary("summary of turn one", reply1.Id);

        conv.TruncateFrom(user2.Id);

        Assert.Equal("summary of turn one", conv.Summary);
        Assert.Equal(reply1.Id, conv.SummarizedThroughMessageId);
        Assert.Equal(2, conv.Messages.Count);
    }

    [Fact]
    public void Truncating_past_the_compaction_cut_drops_the_stale_summary()
    {
        var (conv, user1, reply1, _) = TwoTurns();
        conv.UpdateSummary("summary of turn one", reply1.Id);

        conv.TruncateFrom(user1.Id);

        Assert.Null(conv.Summary);
        Assert.Null(conv.SummarizedThroughMessageId);
        Assert.Empty(conv.Messages);
    }
}
