namespace Gabriel.API.Contracts.Sequence;

// Wire-format Gabriel Sequence. The frame Pixels arrays are palette indices
// (each value in [0, palette.Length)) so the body stays compact — a 16×16
// palette-indexed frame is 256 bytes pre-serialization vs ~3 KB raw RGB.
//
// Palette colors are serialized as [r, g, b] tuples; clients reconstruct
// RgbColor by index lookup. Frames are sent in canonical 0..63 order; layers
// can be derived as `floor(frameIndex / 16)` (0=DNA, 1=Traits, 2=Context, 3=Live).
public record GabrielSequenceResponse(
    int Version,
    IReadOnlyList<int[]> Palette,
    IReadOnlyList<int[]> Frames,
    SequenceMetadataResponse Metadata);

public record SequenceMetadataResponse(
    long Seed,
    DateTimeOffset GeneratedAt,
    string? StateSummary);
