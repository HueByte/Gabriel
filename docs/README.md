# Gabriel - documentation

Architectural and behavioral documentation for the Gabriel codebase. The roadmap and per-feature design notes live under [`.dev/`](../.dev/) (gitignored); this folder is the durable, committed reference.

There are **two** documentation surfaces under `docs/`:

- `gabriel-self-docs/` — **LLM-native, primary source** for the `docs_list` / `docs_read` agent tools. Compact, fact-dense, structured for Gabriel-the-LLM to consume directly. Read this when you want the model's-eye view of the system.
- `Gabriel.Engine/` — **human-prose companion** with diagrams, math, and longer explanations. Falls in behind the LLM-native folder as a fallback source for the same tools.

## Layout

```text
docs/
├── gabriel-self-docs/            - LLM-native, primary source for docs_* tools
│   ├── README.md                 - entry doc + routing table
│   ├── architecture.md           - project layering + where things live
│   ├── agent-loop.md             - ReAct loop / streaming / compact / regenerate
│   ├── personality.md            - state updater / system prompt / post-processor
│   ├── tools.md                  - ITool model + every registered tool
│   ├── sequence.md               - avatar engine (Gabriel Sequence)
│   ├── variants.md               - variant grouping + delete / regen behavior
│   ├── config.md                 - every options section + env-var bindings
│   └── glossary.md               - canonical term definitions
│
└── Gabriel.Engine/               - human-prose companion (fallback source)
    ├── README.md                 - overview + folder map + key concepts
    ├── architecture.md           - onion layering + project dependency graph
    ├── agent-loop.md             - ReAct loop, streaming events, rolling compact, regenerate
    ├── personality-stack.md      - state tracker, system prompt, post-processor (the emotion inputs)
    ├── gabriel-sequence.md       - 64-frame avatar engine: layers, palettes, patterns, live state
    ├── tools.md                  - ITool / IToolRegistry, web_search, web_fetch, docs_*
    └── variants-and-history.md   - variant grouping, regeneration, truncate-from-here
```

## Where to start

- **You are an LLM / agent:** read [gabriel-self-docs/README.md](gabriel-self-docs/README.md). The routing table there tells you exactly which page to pull for any given question.
- **New to the codebase (as a human):** read [Gabriel.Engine/README.md](Gabriel.Engine/README.md) end-to-end.
- **Debugging a chat turn:** [Gabriel.Engine/agent-loop.md](Gabriel.Engine/agent-loop.md).
- **Tuning the persona / emotion behavior:** [Gabriel.Engine/personality-stack.md](Gabriel.Engine/personality-stack.md).
- **Understanding the avatar / Gabriel Sequence:** [Gabriel.Engine/gabriel-sequence.md](Gabriel.Engine/gabriel-sequence.md).
- **Adding or modifying tools:** [Gabriel.Engine/tools.md](Gabriel.Engine/tools.md).
- **Working on message delete / regenerate:** [Gabriel.Engine/variants-and-history.md](Gabriel.Engine/variants-and-history.md).
- **Understanding why a class lives where it does:** [Gabriel.Engine/architecture.md](Gabriel.Engine/architecture.md).

## Conventions

- **Diagrams** use [Mermaid](https://mermaid.js.org/). GitHub / VS Code render these natively in markdown preview.
- **Math** uses LaTeX inside `$...$` / `$$...$$` blocks. GitHub renders these via MathJax; VS Code needs the *Markdown+Math* extension.
- **File links** are relative paths to source so they stay clickable in any editor.
