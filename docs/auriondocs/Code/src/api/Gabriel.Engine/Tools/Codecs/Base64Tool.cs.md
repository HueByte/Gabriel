# Base64Tool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/Base64Tool.cs`  
> **Kind:** class

```csharp
public sealed class Base64Tool : ITool
```


Base64Tool is a pure function that encodes text to Base64 or decodes a Base64 string back to text based solely on its input arguments. It treats text as UTF-8 and enforces a maximum input length of 100000 characters. Use op encode to convert text into Base64, or op decode to turn a Base64 string back into readable text. When encoding for URLs or filenames, you can opt-in url_safe to emit the URL-safe alphabet with no padding; decoding accepts both alphabets transparently. It is not intended for hashing or other non-reversible encodings.

## Remarks
Base64Tool implements the ITool interface to provide a deterministic, side-effect-free transformation that is easy to compose in tests and scripts. Validation and error behavior are centralized in Base64Exception, keeping the normal path simple: a result string prefixed with Encoded: or Decoded:, or an error string prefixed with Error:. The url_safe option makes it practical to embed encoded data in URLs or filenames, while decoding remains flexible.

## Notes
- text must be non-empty and not exceed 100000 characters; otherwise a Base64Exception is thrown.
- op must be encode or decode; any other value raises a Base64Exception.
- encoding with url_safe uses a URL-safe alphabet and omits padding; decoding accepts both alphabets automatically.