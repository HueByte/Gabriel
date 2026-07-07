# HashTool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/HashTool.cs`  
> **Kind:** class

```csharp
public sealed class HashTool : ITool
```


HashTool computes a cryptographic hash digest of the given text and returns it as lowercase hexadecimal. It is a pure function with no I/O or external dependencies, encoding text as UTF-8 and returning a string in the form "<algo>: <hex>"; the default is sha256 and the supported algorithms are md5, sha1, sha256, and sha512 (md5/sha1 are legacy checksums only).

## Remarks
HashTool provides a stable, verifiable fingerprint of a string without touching outside resources, which makes it ideal for content-addressable storage, cache keys, integrity checks, or any scenario where a deterministic digest is required. It’s designed as a small, testable unit: given the same text and algorithm, it always yields the same lowercase hex digest, enabling straightforward comparisons in higher-level logic.

## Notes
- The tool expects a JSON payload with a required "text" field and an optional "algo" field; invalid JSON or a missing/incorrect "text" yields a HashException with a descriptive message.
- The input text is capped at 1,000,000 characters to avoid excessive memory usage.
- The "algo" must be one of: md5, sha1, sha256, or sha512; md5/sha1 are provided for legacy checksums only.