Returns the prefix of the input string s consisting of at most n UTF-16 code units. If s.Length is less than or equal to n, the method returns s unchanged. This private helper centralizes truncation logic so callers can safely obtain a substring of a maximum length without duplicating the boundary check at every call site.

## Remarks

This method consolidates the truncation behavior within the class to ensure consistent results wherever a short preview or label is needed. Truncation operates on UTF-16 code units; as such, cutting in the middle of a surrogate pair or a combining sequence may yield an invalid or visually odd string. For user-facing truncation that preserves full characters or grapheme clusters, consider a text-element-aware approach (e.g., System.Globalization.StringInfo).

Because the method is private, it is an internal implementation detail rather than part of the public API.

## Notes

- If n < 0, the slicing s[..n] would throw; ensure n is non-negative before invoking (the method itself does not guard against negative values).
- If s is null, accessing s.Length will throw a NullReferenceException.