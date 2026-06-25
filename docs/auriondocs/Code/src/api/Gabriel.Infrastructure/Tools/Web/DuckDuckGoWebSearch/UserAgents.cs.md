UserAgents is a small, hard-coded pool of real-browser User-Agent strings used to initialize the session's user agent fingerprint. It is not rotated per-request; a single UA is selected during session warmup and kept for the duration of the session, with the pool designed to spread fingerprints across deployments and restarts.

## Remarks
Centralizes UA management to avoid per-request rotations that could trigger bot-detection while preserving fingerprint diversity across environments. By selecting a single UA at session warmup and keeping it for the session, it mirrors real browser behavior and reduces fingerprint noise within a session. The pool distributes fingerprints across deployments and restarts, preventing uniform UA usage across the system.

## Notes
- Hard-coded UA strings may become outdated; update periodically to reflect current browser versions.
- The field is private and static readonly; it cannot be modified at runtime. If you need dynamic UA rotation, introduce a separate mechanism.