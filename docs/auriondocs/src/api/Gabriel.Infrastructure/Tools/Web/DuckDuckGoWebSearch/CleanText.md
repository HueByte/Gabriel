Decodes HTML entities, removes HTML tags using TagStripRegex, and trims surrounding whitespace from the given string. Use this helper when normalizing text extracted from web content (for example, search result snippets) so consumers get plain, trimmed text instead of HTML-encoded or tagged fragments.

## Remarks
This centralizes the common sequence of cleaning steps applied to HTML-derived text: first HtmlDecode converts entities (e.g. &amp; -> &), then TagStripRegex removes markup, and finally Trim removes leading/trailing whitespace. Keeping these steps together ensures consistent, predictable output across the code that processes web-derived strings; the exact tag-removal behavior depends on the TagStripRegex definition and can be adjusted there if different stripping rules are required.

## Notes
- The input must not be null: callers should ensure a non-null string is passed (Regex.Replace will throw if given a null input).
- All HTML tags matched by TagStripRegex are removed; any inline formatting or tag-delimited content will be lost, not escaped. If preserving some markup or converting tags to plaintext equivalents is required, modify the pipeline accordingly.