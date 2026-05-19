# Gabriel — documentation

Architectural and behavioral documentation for the Gabriel codebase. The roadmap and per-feature design notes live under [`.dev/`](../.dev/) (gitignored); this folder is the durable, committed reference.

## Layout

```text
docs/
└── Gabriel.Engine/
    ├── README.md                 — overview + folder map + key concepts
    ├── architecture.md           — onion layering + project dependency graph
    ├── agent-loop.md             — ReAct loop, streaming events, rolling compact
    ├── personality-stack.md      — state tracker, system prompt, post-processor
    └── variants-and-history.md   — variant grouping, regeneration, truncation
```

## Where to start

- **New to the codebase:** read [Gabriel.Engine/README.md](Gabriel.Engine/README.md) end-to-end.
- **Debugging a chat turn:** [Gabriel.Engine/agent-loop.md](Gabriel.Engine/agent-loop.md).
- **Tuning the persona:** [Gabriel.Engine/personality-stack.md](Gabriel.Engine/personality-stack.md).
- **Working on message delete / regenerate:** [Gabriel.Engine/variants-and-history.md](Gabriel.Engine/variants-and-history.md).
- **Understanding why a class lives where it does:** [Gabriel.Engine/architecture.md](Gabriel.Engine/architecture.md).

## Conventions

- **Diagrams** use [Mermaid](https://mermaid.js.org/). GitHub / VS Code render these natively in markdown preview.
- **Math** uses LaTeX inside `$...$` / `$$...$$` blocks. GitHub renders these via MathJax; VS Code needs the *Markdown+Math* extension.
- **File links** are relative paths to source so they stay clickable in any editor.
