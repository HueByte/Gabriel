HashTool computes a cryptographic hash digest of the provided text and returns it as a lowercase hexadecimal string prefixed with the chosen algorithm name (for example: sha256: abcd...).
It is a pure function of its inputs: there are no I/O, no external dependencies, and no approvals required. Input text is encoded as UTF-8. The tool supports md5, sha1, sha256, and sha512, defaulting to sha256 when the algo is not specified. MD5 and SHA1 are kept for legacy checksum scenarios only and are not recommended for security-sensitive purposes. If you need reversible encoding, use base64 instead of hashing.

## Remarks
HashTool abstracts cryptographic digest computation behind a small, deterministic, JSON-based API, enabling callers to obtain a fingerprint for a string without implementing hashing themselves. It centralizes input validation (text length and allowed algorithms) and enforces a consistent output format, which simplifies reasoning about fingerprints across higher-level workflows. The contract is expressed through a JSON payload and a simple string result, making it easy to test and simulate in environments that may not expose cryptographic primitives directly.

## Example
```csharp
// Example usage
var tool = new HashTool();
var json = "{\"text\": \"example\", \"algo\": \"sha256\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
// result is a string like: "sha256: <hex-digest>"
```

## Notes
- On invalid arguments, ExecuteAsync returns an error string starting with "Error: ..." rather than throwing; callers should handle this as a normal result. The error messages originate from the internal HashException used during argument parsing and validation.
- The digest length and hex formatting depend on the chosen algorithm; the output always prefixes the digest with the algorithm name (e.g., sha256: ...). MD5 and SHA1 are legacy options and should not be used for security-critical purposes.
