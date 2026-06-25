Sets up the docs lookup pipeline by registering a composite that prioritizes a local, LLM-native docs source and falls back to GitHub-backed docs. Use this during application startup to enable runtime doc lookups via IDocsLookup with local-first behavior and safe fallback.

## Remarks
Encapsulates the wiring in one DI-scoped place so the rest of the app can depend on IDocsLookup without caring about sources. The CompositeDocsLookup enforces priority (local first, then GitHub) and remains resilient if a source is unavailable; a failure in one source does not poison the others. LocalDocsLookup handles disk-stored, developer-authored docs while GitHubDocsLookup provides external, human-prose references, and the two are composed with an ILogger to aid diagnostics.

## Notes
- Local-first primacy; if you need to bypass, adjust the DI wiring to reorder sources.
- GitHub API usage relies on two named HttpClients (List API and raw content); the token is optional and only sent when provided, enabling access to both public and private docs depending on configuration.
