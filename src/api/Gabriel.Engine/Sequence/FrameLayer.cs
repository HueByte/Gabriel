namespace Gabriel.Engine.Sequence;

// Logical layering of the 64 frames. Indices are inclusive ranges:
//
//   DnaCore       frames  0..15   immutable identity, never changes once generated
//   StableTraits  frames 16..31   long-term preferences, very slow drift (weeks)
//   Context       frames 32..47   medium-term accumulated reactions (hours / days)
//   LiveState     frames 48..63   current emotional state, recomputed per turn
//
// The layer of a given frame index is `(index / 16)` cast to this enum.
public enum FrameLayer
{
    DnaCore       = 0,
    StableTraits  = 1,
    Context       = 2,
    LiveState     = 3,
}

public static class FrameLayers
{
    public const int FramesPerLayer = 16;
    public const int LayerCount = 4;

    public static FrameLayer Of(int frameIndex) => (FrameLayer)(frameIndex / FramesPerLayer);

    public static (int Start, int End) Range(FrameLayer layer)
    {
        var start = (int)layer * FramesPerLayer;
        return (start, start + FramesPerLayer - 1);
    }
}
