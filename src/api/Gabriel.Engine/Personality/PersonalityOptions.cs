namespace Gabriel.Engine.Personality;

public class PersonalityOptions
{
    public const string SectionName = "Personality";

    // Persona display name — injected into the system prompt and the few-shot block.
    // Future Phase 8 (per-project personality) replaces this with per-project config.
    public string Name { get; set; } = "Gabriel";

    // --- Response length caps (post-processor) -----------------------------------

    // Hard ceiling on response tokens relative to user's last message tokens.
    // Final cap = min(LastUserTokens * Multiplier, AbsoluteCap), raised to
    // DetailCap when ConversationState.UserAskedForDetail is true.
    //
    // DetailCap is generous because the user's "write me A* in C++" / "explain
    // OAuth" requests can run 1k-2k tokens easily, and truncating mid-code is
    // worse than emitting too much. The persona prompt is the primary length
    // gate; the cap is just a runaway-safety net.
    public double MaxResponseMultiplier { get; set; } = 2.5;
    public int MaxResponseTokenCap { get; set; } = 300;
    public int DetailResponseTokenCap { get; set; } = 2000;

    // --- Streaming-tempo simulation (controller) ---------------------------------

    // Initial pause before forwarding the first text delta — the "thinking" beat.
    // Picked uniformly per turn; small jitter is built in by the range itself.
    public int MinThinkingDelayMs { get; set; } = 400;
    public int MaxThinkingDelayMs { get; set; } = 1100;

    // Target throughput while forwarding deltas. We pick a random cps in this
    // range per turn and pace cumulative chars-sent against it. Real fast human
    // typing is ~60-80 cps; we sit slightly above so short replies don't crawl.
    public int MinCharsPerSecond { get; set; } = 55;
    public int MaxCharsPerSecond { get; set; } = 85;
}
