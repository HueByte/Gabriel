Returns the first n characters of the provided string, or the original string when its length is less than or equal to n. Use this helper when you need a safe, concise truncation that avoids slicing when not necessary.

## Remarks
This small utility centralizes truncation logic so callers don't need to repeat the length check before slicing. It uses the C# range operator for concise substring extraction and returns the original string unchanged when no truncation is required.

## Example
```csharp
// "Hel"
var a = FirstChars("Hello", 3);
// "Hi" (input shorter than n)
var b = FirstChars("Hi", 5);
// returns the original string when length <= n (no truncation)
var s = "Example";
var c = FirstChars(s, s.Length);
```

## Notes
- If s is null this method will throw a NullReferenceException.
- A negative n will cause the range operation to throw (ArgumentOutOfRangeException); n should be >= 0.
- The method counts UTF-16 code units (char) — it can split surrogate pairs or combining sequences if used on grapheme clusters.