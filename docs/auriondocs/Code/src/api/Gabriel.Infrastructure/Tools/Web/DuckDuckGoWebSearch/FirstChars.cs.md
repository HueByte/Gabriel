Returns the first up to n characters of the provided string. If s.Length <= n, it returns s unchanged; otherwise it returns a new string containing the first n characters. This private helper is useful for producing compact previews or prefixes of strings without forcing allocations when the input already fits within the limit.

## Remarks
Encapsulates the boundary logic for truncating a string to a fixed width, enabling consistent previews across call sites and reducing duplicated conditional slicing.

## Example
```csharp
var a = FirstChars("abcdef", 3); // "abc"
var b = FirstChars("hi", 5); // "hi"
```

## Notes
- Null s will throw a NullReferenceException when accessing s.Length.
- Negative n will throw an ArgumentOutOfRangeException due to the range operation.