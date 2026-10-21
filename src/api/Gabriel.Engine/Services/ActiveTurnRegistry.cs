using System.Collections.Concurrent;

namespace Gabriel.Engine.Services;

// Tracks the turn currently streaming per conversation so a separate request
// (POST .../messages/stop) can stop it. A stop is distinct from the client
// disconnecting: the SSE connection stays open, the loop persists what was
// produced so far and ends with ordinary assistantMessage + done events, so
// the client gets real message ids instead of a dangling partial bubble.
//
// In-process only. Gabriel runs as a single API instance; a scaled-out
// deployment would need this keyed in a shared store.
public interface IActiveTurnRegistry
{
    // Registers a turn for the conversation. Dispose when the turn ends.
    ActiveTurn Begin(Guid conversationId, Guid userId, CancellationToken requestAborted);

    // Requests a stop of the conversation's running turn. False when nothing
    // is running for that conversation and user (finished, never started, or
    // another user's conversation).
    bool TryStop(Guid conversationId, Guid userId);
}

public sealed class ActiveTurnRegistry : IActiveTurnRegistry
{
    private readonly ConcurrentDictionary<Guid, ActiveTurn> _turns = new();

    public ActiveTurn Begin(Guid conversationId, Guid userId, CancellationToken requestAborted)
    {
        var turn = new ActiveTurn(conversationId, userId, requestAborted, this);
        // Last writer wins: a second tab starting a turn on the same
        // conversation becomes the stoppable one.
        _turns[conversationId] = turn;
        return turn;
    }

    public bool TryStop(Guid conversationId, Guid userId)
    {
        if (!_turns.TryGetValue(conversationId, out var turn) || turn.UserId != userId) return false;
        turn.RequestStop();
        return true;
    }

    // Only removes the entry if it's still this turn - a newer turn on the same
    // conversation may have replaced it.
    internal void End(ActiveTurn turn)
        => _turns.TryRemove(new KeyValuePair<Guid, ActiveTurn>(turn.ConversationId, turn));
}

public sealed class ActiveTurn : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly CancellationTokenSource _linked;
    private readonly CancellationToken _requestAborted;
    private readonly ActiveTurnRegistry _registry;

    internal ActiveTurn(Guid conversationId, Guid userId, CancellationToken requestAborted, ActiveTurnRegistry registry)
    {
        ConversationId = conversationId;
        UserId = userId;
        _requestAborted = requestAborted;
        _registry = registry;
        _linked = CancellationTokenSource.CreateLinkedTokenSource(requestAborted, _stop.Token);
    }

    public Guid ConversationId { get; }
    public Guid UserId { get; }

    // Cancelled by either a stop request or the client disconnecting. Hand this
    // to provider calls and tools; keep the request token for persistence.
    public CancellationToken Token => _linked.Token;

    // True only for an explicit stop while the client is still connected - the
    // case where the loop should wind down gracefully and keep emitting events.
    public bool StopRequested => _stop.IsCancellationRequested && !_requestAborted.IsCancellationRequested;

    internal void RequestStop()
    {
        try { _stop.Cancel(); }
        catch (ObjectDisposedException) { /* turn ended between lookup and stop */ }
    }

    public void Dispose()
    {
        _registry.End(this);
        _linked.Dispose();
        _stop.Dispose();
    }
}
