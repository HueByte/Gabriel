namespace Gabriel.Engine.Sequence;

// Fixed narrow color set ("skin tone") for a Gabriel Sequence. Per the spec,
// palettes are typically 8-32 entries - narrow enough that the sequence reads
// as a coherent visual identity, wide enough to express layer-level differences.
//
// Palette[0] is the canonical "base" / quiescent color. Higher indices are
// brighter or further-saturated variations. Layers shift toward different
// regions of the palette to give DNA / Live State a visual signature.
public sealed record Palette(IReadOnlyList<RgbColor> Colors)
{
    public int Count => Colors.Count;
    public RgbColor this[int index] => Colors[index];
}
