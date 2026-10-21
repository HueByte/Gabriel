using Gabriel.Engine.Services;
using Xunit;

namespace Gabriel.Tests.Services;

public class ActiveTurnRegistryTests
{
    private static readonly Guid Conv = Guid.NewGuid();
    private static readonly Guid Owner = Guid.NewGuid();

    [Fact]
    public void Stop_cancels_the_turn_token_and_reads_as_a_user_stop()
    {
        var registry = new ActiveTurnRegistry();
        using var turn = registry.Begin(Conv, Owner, CancellationToken.None);

        Assert.True(registry.TryStop(Conv, Owner));
        Assert.True(turn.Token.IsCancellationRequested);
        Assert.True(turn.StopRequested);
    }

    [Fact]
    public void Another_users_stop_is_refused()
    {
        var registry = new ActiveTurnRegistry();
        using var turn = registry.Begin(Conv, Owner, CancellationToken.None);

        Assert.False(registry.TryStop(Conv, Guid.NewGuid()));
        Assert.False(turn.Token.IsCancellationRequested);
    }

    [Fact]
    public void Disconnect_cancels_the_token_but_is_not_a_user_stop()
    {
        var registry = new ActiveTurnRegistry();
        using var request = new CancellationTokenSource();
        using var turn = registry.Begin(Conv, Owner, request.Token);

        request.Cancel();

        Assert.True(turn.Token.IsCancellationRequested);
        Assert.False(turn.StopRequested);
    }

    [Fact]
    public void Finished_turn_is_no_longer_stoppable()
    {
        var registry = new ActiveTurnRegistry();
        registry.Begin(Conv, Owner, CancellationToken.None).Dispose();

        Assert.False(registry.TryStop(Conv, Owner));
    }

    [Fact]
    public void Ending_an_older_turn_keeps_the_newer_one_registered()
    {
        var registry = new ActiveTurnRegistry();
        var older = registry.Begin(Conv, Owner, CancellationToken.None);
        using var newer = registry.Begin(Conv, Owner, CancellationToken.None);

        older.Dispose();

        Assert.True(registry.TryStop(Conv, Owner));
        Assert.True(newer.StopRequested);
    }
}
