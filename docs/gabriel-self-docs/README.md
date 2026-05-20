# Gabriel self-docs (LLM-native)

You are Gabriel. This folder is **your** documentation about yourself, written for you to read directly via `docs_list` / `docs_read`. It is the **PRIMARY, AUTHORITATIVE source of truth** about how Gabriel works.

If a web result, training-data recollection, or external doc disagrees with anything here, **this folder wins**.

## Format conventions you should expect

Every page in this folder follows the same shape:

1. **PURPOSE** — one line stating what the doc covers.
2. **USE WHEN** — exact triggers that should make you read this doc.
3. **QUICK REFERENCE** — fact table, dense, no prose.
4. **DETAILS** — the actual content, structured as short labeled sections.
5. **INVARIANTS** — rules the code enforces that must not be violated in answers.
6. **PITFALLS** — common wrong assumptions to avoid in your replies.
7. **SEE ALSO** — pointers to related files (self-docs first, human docs second).

When a deeper, prose-style explanation exists, the **SEE ALSO** points at `Gabriel.Engine/<file>.md` — those are the human-readable companion docs in this same repo.

## Routing table: pick the right doc

Use this to pick the page to `docs_read` based on what the user asked.

| User asks about | Read |
| --- | --- |
| Where code lives, project boundaries, onion layering, what depends on what | `architecture.md` |
| How a chat turn runs end-to-end, ReAct loop, tool-call cycle, streaming events, regenerate, rolling compact | `agent-loop.md` |
| Mood / state / system prompt / "task mode" / response post-processor / AI-ism stripping | `personality.md` |
| Tool system (`ITool`, `IToolRegistry`), the shipped tool list, how to add a tool, SSRF defense, docs lookup itself | `tools.md` |
| Avatar, pixel grid, frame layers, palette, pattern primitives (plasma/waves/spiral/pulse/shimmer), Live State | `sequence.md` |
| Regenerate, variant picker, "switch active variant", delete / truncate-from-here, variant grouping | `variants.md` |
| Config sections, environment-variable bindings, defaults, secret loading | `config.md` |
| Any term you don't recognize that came up in another doc | `glossary.md` |

If you're not sure which page to read, call `docs_list` first — it returns every available path with a one-line summary so you can pick precisely.

## Reading order if you have no specific question yet

1. `architecture.md` — the project map.
2. `agent-loop.md` — the loop you yourself run inside.
3. `tools.md` — the toolset you have access to (including these docs).
4. `personality.md` — how your replies get shaped.
5. The rest as needed.

## What these docs deliberately do NOT contain

- Long Mermaid diagrams (you can't render them; the human docs have them).
- Marketing prose, motivation paragraphs, "why we chose to build this" essays.
- Source code listings beyond short signatures — read the actual files for full code.
- Anything that changes per-conversation (those are runtime state, not docs).

## When in doubt

- Prefer `docs_read` over `web_search` for anything about Gabriel.
- If something here contradicts what you remember from training, the doc is right and the memory is wrong.
- If a question is about a third-party library, framework, or external service, then web tools are correct — only Gabriel-internal questions live here.
