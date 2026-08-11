# HashTool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/HashTool.cs`  
> **Kind:** class

```csharp
public sealed class HashTool : ITool
```


HashTool computes a cryptographic hash digest of the provided text and returns it as lowercase hexadecimal, prefixed with the algorithm name. It is a pure function with no I/O or external dependencies; you pass UTF-8 text and an optional algorithm to obtain a stable fingerprint of the input, useful for integrity checks or deduplication. The default algorithm is sha256, and you may choose md5, sha1, sha256, or sha512 (md5/sha1 are legacy checksums). The text length is limited to 1,000,000 characters, and the empty string is allowed. The return format is a single string like "<algo>: <hex-digest>" (lowercase hex). If arguments are invalid, the method returns an error string rather than throwing an exception.

## Remarks
HashTool serves as a focused, testable abstraction for producing deterministic digests within the agent workflow. It centralizes UTF-8 input handling and hex-encoded output formatting, enacting a hard input limit to guard against pathological inputs. While it supports md5 and sha1 for legacy interoperability, those algorithms are explicitly treated as non-security checksums. Validation errors surface as a plain error string rather than as exceptions, and a private HashException distinguishes argument validation from genuine runtime bugs.

## Example
```csharp
using System.Threading;

var tool = new HashTool();
var json = "{\"text\":\"abc\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
// result == "sha256: ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
```

## Notes
- The returned string always starts with the chosen algorithm followed by a colon and a space, e.g., "sha256: ...". To extract the digest, split on the first ": ".
- If the input JSON is malformed, or if the required "text" field is missing or not a string, the tool returns an error string (e.g., "Error: ...").
- The private HashException class is used to distinguish validation errors from real runtime failures; callers should rely on the normal string return value for all outcomes.
