namespace Gabriel.Engine.Sequence;

// The catalog of pattern + palette identifiers a client can pin on a Project
// or standalone Conversation. Identifiers are case-insensitive strings (not
// enum-typed on the wire) so the catalog can grow without database migrations.
// Unknown identifiers are silently ignored at generation time — the generator
// falls back to the seed-derived pick.
public static class SequenceCatalog
{
    // Names line up with PatternKind values; lowercase canonical form.
    public static readonly IReadOnlyList<string> Patterns = new[]
    {
        "plasma", "waves", "spiral", "pulse", "shimmer",
    };

    // Names line up with PaletteTemplates.All. Kept here (not generated from
    // the array) so they can be sorted / curated independently of the
    // template definition order.
    public static readonly IReadOnlyList<string> Palettes = new[]
    {
        "heat", "ice", "plasma", "matrix", "sunset", "ocean", "aurora", "rose",
        "cyber", "amber", "lime", "sakura", "mono", "void", "forge", "grok",
    };

    public static bool IsKnownPattern(string? name)
        => !string.IsNullOrWhiteSpace(name)
            && Patterns.Contains(name.Trim().ToLowerInvariant());

    public static bool IsKnownPalette(string? name)
        => !string.IsNullOrWhiteSpace(name)
            && Palettes.Contains(name.Trim().ToLowerInvariant());

    // Returns the canonical (lowercased + trimmed) form if known, else null.
    // Callers should persist what this returns rather than the raw input so
    // database rows always carry the canonical identifier.
    public static string? NormalizePattern(string? name)
        => IsKnownPattern(name) ? name!.Trim().ToLowerInvariant() : null;

    public static string? NormalizePalette(string? name)
        => IsKnownPalette(name) ? name!.Trim().ToLowerInvariant() : null;

    // Parse pattern override into the enum if possible, else null (seed-derived).
    public static PatternKind? TryParsePattern(string? name)
        => NormalizePattern(name) switch
        {
            "plasma" => PatternKind.Plasma,
            "waves" => PatternKind.Waves,
            "spiral" => PatternKind.Spiral,
            "pulse" => PatternKind.Pulse,
            "shimmer" => PatternKind.Shimmer,
            _ => null,
        };
}
