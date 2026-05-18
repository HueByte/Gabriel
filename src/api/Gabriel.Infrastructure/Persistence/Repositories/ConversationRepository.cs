using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _ctx;

    public ConversationRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<Conversation?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _ctx.Conversations.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _ctx.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<Conversation>> ListAsync(Guid userId, CancellationToken ct = default)
        => await _ctx.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
        => await _ctx.Conversations.AddAsync(conversation, ct);

    public void AddMessage(Message message)
        => _ctx.Set<Message>().Add(message);

    public void Update(Conversation conversation)
        => _ctx.Conversations.Update(conversation);

    public void Remove(Conversation conversation)
        => _ctx.Conversations.Remove(conversation);
}
