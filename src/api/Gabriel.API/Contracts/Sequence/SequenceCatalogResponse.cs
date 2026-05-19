namespace Gabriel.API.Contracts.Sequence;

// Catalog of selectable pattern + palette identifiers for the avatar skin
// picker. Clients send the chosen identifier back via the project / conversation
// PATCH endpoints; the server validates against this list and stores it on the
// entity. Sent as separate string arrays (rather than enum types) so the
// catalog can grow without forcing client regeneration.
public record SequenceCatalogResponse(
    IReadOnlyList<string> Patterns,
    IReadOnlyList<string> Palettes);
