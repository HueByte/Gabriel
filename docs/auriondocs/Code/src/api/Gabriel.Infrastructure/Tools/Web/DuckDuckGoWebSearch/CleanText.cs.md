Decodes HTML entities in the input, strips HTML tags, and trims surrounding whitespace to produce clean, plain-text output. Use this when you need to render or store text extracted from HTML content in a readable, markup-free form.

## Remarks
This method centralizes HTML normalization, ensuring consistent results for strings sourced from HTML content that must be displayed or processed as plain text. By applying HtmlDecode first and then removing markup, it preserves legibility while discarding tags; its private scope indicates it is an internal utility used by higher-level web-search related logic.

## Notes
- Null input will typically throw; ensure a non-null string is provided before calling.
- Relying on TagStripRegex means very malformed or complex HTML may not be perfectly sanitized.
- This is a private helper; changes to its behavior or signature can occur without affecting the public API.