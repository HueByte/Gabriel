Returns the first n characters of the provided string. If the input string's length is less than or equal to n, the original string is returned unchanged. Use this helper when you need a safe prefix/truncation without manually checking the string length.

## Remarks
This small utility centralizes the length check and slicing logic to avoid repeated Substring/range checks elsewhere in the codebase. It is intentionally minimal — it performs no null or bounds normalization and expects callers to provide sensible arguments.

## Example
```csharp
var first = FirstChars("Hello, world", 5); // "Hello"
var whole = FirstChars("Hi", 5);          // "Hi"
```

## Notes
- Passing a null string will throw a NullReferenceException (the method reads s.Length). Ensure callers validate or never pass null.
- A negative n will cause an ArgumentOutOfRangeException from the range operator; n must be >= 0.
- If n is greater than or equal to s.Length, the original string is returned (no allocation for truncation is necessary beyond the normal string handling).