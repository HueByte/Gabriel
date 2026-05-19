namespace Gabriel.Engine.Sequence;

// The five pattern primitives the generator can render. Each has its own
// animation grammar (see Patterns.cs). The seed picks one when no override
// is set on the owning Project / Conversation; explicit overrides let users
// pin a specific look without rerolling for it.
public enum PatternKind
{
    Plasma,
    Waves,
    Spiral,
    Pulse,
    Shimmer,
}
