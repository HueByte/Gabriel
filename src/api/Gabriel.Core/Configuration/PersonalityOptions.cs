namespace Gabriel.Core.Configuration;

public class PersonalityOptions : IConfigSection<PersonalityOptions>
{
    public static string SectionName => "Personality";

    // Persona display name - injected into the system prompt and the few-shot block.
    // Future Phase 8 (per-project personality) replaces this with per-project config.
    public string Name { get; set; } = "Gabriel";

    // --- Streaming-tempo simulation (controller) ---------------------------------

    // Initial pause before forwarding the first text delta - the "thinking" beat.
    // Picked uniformly per turn; small jitter is built in by the range itself.
    public int MinThinkingDelayMs { get; set; } = 400;
    public int MaxThinkingDelayMs { get; set; } = 1100;

    // Target throughput while forwarding deltas. We pick a random cps in this
    // range per turn and pace cumulative chars-sent against it. Real fast human
    // typing is ~60-80 cps; we sit slightly above so short replies don't crawl.
    public int MinCharsPerSecond { get; set; } = 55;
    public int MaxCharsPerSecond { get; set; } = 85;
}
