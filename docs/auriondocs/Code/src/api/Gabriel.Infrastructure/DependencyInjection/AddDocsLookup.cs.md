Configures the dependency injection container to compose the model-facing docs lookup from two sources: LocalDocsLookup (primary, reading from disk) and GitHubDocsLookup (fallback, reading from GitHub). This private static AddDocsLookup(IServiceCollection, IConfiguration) is invoked during startup to register the sources, their options, and a CompositeDocsLookup that presents a single IDocsLookup with local-first priority for both ListAsync and ReadAsync.

## Remarks

By design, the composite pattern isolates the two sources while presenting a single API to consumers. Local docs are treated as the canonical source; if they are unavailable, the GitHub-backed docs are used as a fallback. The two HttpClients configured here (named ApiHttpClientName and RawHttpClientName) keep concerns separated and make network access configurable. The resulting CompositeDocsLookup enforces the priority order (local first, then remote) and ensures a failing source does not poison the others.

## Notes

- The priority order affects both ListAsync and ReadAsync behavior; changing the order changes which source supplies data first and how overlaps are resolved.
- GitHub token is optional; when provided, Authorization header is included; otherwise only public endpoints are used.
- If both sources fail to provide a requested doc, there is no data to return; callers should handle missing content gracefully.