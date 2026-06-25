Precompiled, private Regex that matches simple HTML-like tags (<[^>]+>), used to strip markup from text in a consistent, low-overhead way. The field is static and readonly, so a single compiled instance is reused across the class, avoiding repeated allocations and regex creation during tag stripping.

## Remarks
Centralizes tag-stripping logic to ensure uniform, predictable results wherever HTML markup must be removed. The static readonly, compiled-Regex approach favors performance in hot paths such as sanitizing user input or processing web results, at the cost of relying on a pragmatic tag pattern rather than a full HTML parser. Note that this pattern trims tags but does not interpret or sanitize embedded JavaScript or CSS; use a full HTML sanitizer if those concerns apply.

## Notes
- It only strips simple tags; it won't handle nested or malformed HTML with the depth and nuance of a real HTML parser.
- It may over-strip text containing angle brackets used as literals, since <...> is treated as a tag.