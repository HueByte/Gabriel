This private, precompiled Regex identifies the light-weight search result entries in the DuckDuckGo Lite HTML response. It matches an anchor tag with class='result-link' and captures the target URL from the href attribute and the displayed title from the anchor's inner text. The pattern is used during Lite endpoint parsing to extract the link and label for each result; named capture groups 'href' and 'text' are consumed downstream to build result items, and the link is passed through UnwrapRedirect when a redirect marker is present (the helper is a no-op otherwise). The Regex is compiled for performance (RegexOptions.Compiled) and uses Singleline to allow the inner text to span lines.

## Remarks
Lite endpoint parsing uses a flatter table structure and single-quoted class attributes; this Regex captures exactly that scenario and is intentionally narrow to avoid false positives. Keeping this logic in a single compiled pattern helps readability and performance; if the Lite HTML changes, this symbol will need updating to reflect new markup.

## Notes
- Fragility: relies on exact Lite HTML markup (href value in double quotes and class='result-link'); changes in the endpoint’s markup may cause the pattern to fail.
- Scope: specifically designed for the Lite results path; behavior may differ for other DuckDuckGo result formats.