Return the first n characters of the given string s, or the entire string when its length is less than or equal to n. Use this helper to safely truncate a string to a maximum length without throwing when the input is shorter than the requested length.

## Remarks
A tiny utility that centralizes the common "take first N characters" pattern so callers don't need to write conditional substring logic. It uses C# range syntax for brevity and returns the original string unchanged when no truncation is needed.

## Example
```csharp
// returns "Hel"
var small = FirstChars("Hello", 3);

// returns "Hi" because the input is shorter than 5
var unchanged = FirstChars("Hi", 5);
```

## Notes
- The input string s must not be null; calling this with null will result in a NullReferenceException.
- n must be non-negative; a negative n will throw an exception when evaluating the range.
- When truncation occurs a new string is allocated; when s.Length <= n the original string is returned as-is.