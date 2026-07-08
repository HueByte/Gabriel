TagStripRegex is a precompiled regular expression field used to strip HTML-like tags from text within the DuckDuckGo web search component. The pattern <[^>]+> matches a '<' character, followed by one or more characters that are not '>', and a closing '>', enabling tag removal without incurring runtime regex compilation costs.

## Remarks
TagStripRegex is private, static, and readonly, so a single instance is shared across the class and cannot be reassigned after initialization. This design minimizes allocations and improves performance in hot paths that perform repeated tag stripping against search results. Note that the pattern is intentionally simple and not a full HTML parser; for complex HTML handling, prefer a dedicated parser.

## Notes
- The regex is simplistic and may not handle edge cases such as comments or CDATA sections, or angle brackets used in non-tag contexts.
- Because the field is compiled, changes require recompiling the assembly to take effect.