namespace Gabriel.Engine.Sequence;

// The canonical Gabriel Sequence: 64 palette-indexed 16×16 frames + a shared
// palette + metadata. See [.dev/notes/emotion-engine.md](../../.dev/notes/emotion-engine.md)
// for the spec.
//
// Storage strategy: we DON'T persist the rendered bytes. Sequences are
// generated on-demand from (seed, ConversationState) by IGabrielSequenceGenerator
// — both inputs already live on the Conversation entity, so no extra rows.
public sealed record GabrielSequence(
    int Version,
    Palette Palette,
    IReadOnlyList<Frame> Frames,
    SequenceMetadata Metadata)
{
    public const int FrameCount = 64;
    public const int SchemaVersion = 1;

    public Frame this[int index] => Frames[index];
    public Frame this[FrameLayer layer, int frameInLayer]
        => Frames[(int)layer * FrameLayers.FramesPerLayer + frameInLayer];
}

public sealed record SequenceMetadata(
    long Seed,
    DateTimeOffset GeneratedAt,
    // Short human-readable summary of the Live State that drove the generation —
    // e.g. "playful, short-message rhythm, 4 turns in". Helps debugging + supports
    // the spec's "observation pass" (any compatible AI can read this).
    string? StateSummary);
