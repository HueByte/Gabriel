TextStatsTool is a pure, dependency-free utility that, given a block of text, returns a compact, human-friendly summary of its size and structure: character, word, line, sentence, and paragraph counts, plus an estimated reading time and a rough token count. It counts Unicode code points (so emoji and CJK are counted as users expect) and uses heuristic counts for sentences and paragraphs; use it when you need to answer 'how long is this?' rather than guess.

## Remarks
TextStatsTool centralizes the text-measurement logic behind a single, stateless interface, ensuring consistent metrics across the system whenever a user-visible measure or a cost estimate is needed. It exposes a deterministic, pure function: given the text, it returns a readable summary string without performing I/O, making it easy to test and reuse in dashboards, UI labels, or validation flows. It relies on Unicode code-point counting and simple heuristics for sentences and paragraphs to provide fast, approximate results suitable for UX display rather than exact linguistic analysis.

## Notes
- The token estimate is a rough heuristic (roughly one token per four characters) and should not be used for precise NLP tokenization.
- Reading time and sentence/paragraph counts are heuristic and language-sensitive; results can vary with punctuation and language.
- Input length is capped at 1,000,000 characters; inputs longer will trigger a validation error.