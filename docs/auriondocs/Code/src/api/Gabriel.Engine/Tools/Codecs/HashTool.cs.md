# HashTool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/HashTool.cs`  
> **Kind:** class

```csharp
public sealed class HashTool : ITool
```


HashTool computes a cryptographic hash of input text (UTF-8) and returns the digest as a lowercase hexadecimal string, prefixed by the algorithm used (for example, 'sha256: ...'). It is a pure function with no I/O or external side effects, intended for fingerprinting or checksumming strings to produce stable identifiers rather than guessing a digest manually.

## Remarks
HashTool provides a simple, deterministic hashing interface behind a single, well-defined entry point. It supports sha256 (default), sha512, sha1, and md5, while clearly signalling that md5/sha1 are legacy checksums and not suitable for security purposes. By handling UTF-8 encoding and the algorithm prefix in a single place, it prevents inconsistencies in digest computation across callers and promotes consistent, comparable fingerprints.

## Notes
- The input text is limited to 1,000,000 characters; inputs longer than this trigger a validation error.
- The 'algo' parameter must be one of md5, sha1, sha256, or sha512; md5/sha1 are legacy and should be avoided for security-related use cases.
- Output format is the chosen algorithm name followed by a colon and a lowercase hexadecimal digest (e.g., 'sha256: <digest>'). If the arguments are invalid, the function returns a string starting with 'Error: ...'.