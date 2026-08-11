Represents a precompiled regular expression used by the Lite endpoint parsing to extract result links from the simplified Lite HTML. The pattern matches anchor elements that have an href attribute and the class attribute set to result-link, capturing the URL as href and the displayed text as text. This regex assumes the Lite endpoint's flatter structure where a link is followed (one row down) by a snippet cell; even though such links are not wrapped in /l/?uddg=…, they are still passed through UnwrapRedirect for safety — the helper is a no-op when the marker is absent.

## Remarks
The Regex is stored as a private static readonly field and is compiled to ensure fast matching in hot paths, avoiding repeated allocations during parsing of multiple Lite results. It focuses narrowly on anchors with class='result-link' so the rest of the HTML is ignored, and it exposes two named capture groups (href and text) that downstream code uses to obtain the destination URL and the link title.

## Notes
- The named capture groups href and text are relied upon by the surrounding parsing logic; renaming them would break the extraction.
- Compiled Regex trades a small startup cost for faster repeated matches in tight loops.
- The Lite HTML structure is assumed to remain stable; changes to the anchor shape or class name would require updating the pattern.