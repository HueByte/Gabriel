namespace Gabriel.Core.Services;

public class AgentOptions
{
    public const string SectionName = "Agent";

    // Hard cap on tool-call iterations per user turn. Going much higher invites
    // runaway loops + wasted spend.
    public int MaxIterations { get; set; } = 8;

    // Trigger rolling-summary compact when estimated history tokens cross this
    // fraction of the provider's ContextWindowTokens.
    public double CompactThreshold { get; set; } = 0.8;

    // Keep this many of the most recent messages verbatim through a compact, so
    // mid-conversation continuity isn't lost when the summary is generated.
    public int CompactKeepLast { get; set; } = 6;
}
