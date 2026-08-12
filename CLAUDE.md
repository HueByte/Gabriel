# CLAUDE.md — Gabriel agent collaboration brief

This file is the working brief for AI agents (and a solid orientation doc for humans) operating in this repository. It is committed because its conventions apply to anyone working in the tree. It is a living document: when reality changes, update it in the same session — stale claims here are bugs.

## Project Overview

**Gabriel** is an LLM chat app with a pixel-art "AI entity" avatar, built as a playground for chat-agent ideas. The mission (stated 2026-08-11): a proper **generalistic personal assistant — to talk, to do tasks, to learn and teach.** Feature priorities are judged against that mission.

Current phase: post-foundation. The agent loop (ReAct + streaming), sessions, auto-compact, identity/JWT, projects, SQLite memory, markdown chat, Docker, and a broad tool belt are shipped. 2026-08-12 (Claude Code-parity push): parallel tool-call execution (`ITool.IsParallelSafe` + `Agent:MaxParallelToolCalls`), per-conversation todo tools (`todo_write`/`todo_read`), a guard-railed `shell_execute` (off by default), semantic memory over Qdrant (`SemanticMemory:Enabled`, off by default; SQLite stays authoritative), and a real test project (`Gabriel.Tests`, xUnit). See `.dev/PLAN.md` (local-only) for the roadmap.

Naming: the repo folder is `PulsePixel` (historical); the product, solution, and all namespaces are **Gabriel**. `prototype/` holds the original Node-based pixel experiments — excluded from lint/CI gates, kept for the trail.

## Architecture

Onion-layered .NET 10 backend + Vite/React 19 + Three.js frontend.

```text
src/
├── api/
│   ├── Gabriel.slnx              # THE solution file (lives in src/api, not root)
│   ├── dotnet-tools.json         # THE tool manifest: swagger CLI + dotnet-ef, pinned
│   ├── Gabriel.Core/             # domain entities, repositories' interfaces, configuration types
│   ├── Gabriel.Engine/           # the agent: AgentService (ReAct loop), IChatProvider + ToolBridge
│   │                             #   (tool-call emulation), ITool registry + tools, Gabriel Sequence
│   │                             #   (avatar engine), personality stack (prompts, conversation state)
│   ├── Gabriel.Infrastructure/   # EF Core (SQLite) + migrations, ASP.NET Identity + JWT, provider
│   │                             #   impls (Grok, Mock), tool impls (web search, docs), file storage
│   └── Gabriel.API/              # controllers, DTO contracts, mapping, SSE streaming endpoint
└── webapp/                       # Vite + React 19 + Three.js; typed API client generated from OpenAPI
```

Dependency direction: `Core ← Engine ← Infrastructure ← API`. The webapp talks to the API only, through the generated client (`src/webapp/src/api/generated/` — never hand-edited, see Verification Rules).

Reference docs: `docs/SYSTEMS.md` (systems inventory), `docs/gabriel-self-docs/` (LLM-native docs — **also a runtime product surface**, served to Gabriel itself via the `docs_list`/`docs_read` tools), `docs/Gabriel.Engine/` (human-prose companion), `docs/conversations-api.md` (full API reference).

## Technology Stack

- Backend: .NET 10, ASP.NET Core, EF Core 10 (SQLite), ASP.NET Identity + JWT cookies, Serilog; Qdrant (REST) + pluggable embeddings (Mock / local ONNX all-MiniLM-L6-v2 / OpenAI) for semantic memory
- Agent: xAI Grok provider + Mock provider; tool-call emulation bridge for text-only models (`ToolMode: Native | Emulated | None`)
- Frontend: Vite, React 19, TypeScript, Three.js, react-markdown + shiki, mermaid
- Infra: Docker + compose (`docker/`), BuildKit named contexts; CI via GitHub Actions (`.github/workflows/`)

## Key Design Principles

These are deliberate decisions — do not "helpfully" redesign them away:

- **The agent loop is provider-agnostic.** `AgentService` consumes one event shape; `GabrielToolBridge` decorates text-only providers so emulated tool calling looks identical to native. Changes to the loop must not assume a specific provider.
- **Context assembly has one authority.** `AgentContext` (Engine) builds provider history AND the token-metrics breakdown from the same snapshot, so UI numbers and actual payloads agree by construction. Never assemble provider history elsewhere.
- **Persona / Project / Memory / Summary stay structurally separate** even though they all ship as system messages — the metrics legend and future prompt-cache reordering depend on the separation.
- **Memory and files are project-scoped.** Cross-project bleed must stay impossible by construction (filters at the query level, not post-hoc).
- **Path traversal hardening on all file tools** — resolve + prefix-check against the allowed root; never trust relative paths from the model.
- **The Gabriel Sequence is identity, not decoration.** Seed → output mapping must stay stable across refactors; a personality's `.gemo` (planned) is the identity file and is never overwritten by live conversation state.

## Development Commands

```powershell
scripts/dev.ps1 up                # API (dotnet watch) + webapp (vite)
scripts/dev.ps1 down              # stop them (kills the recorded process trees)
scripts/dev.ps1 up -Docker        # containerized stack (docker/docker-compose.yml)
scripts/add-migration.ps1 <Name>  # EF migration with the right project flags

dotnet build src/api/Gabriel.slnx           # also regenerates the OpenAPI client (needs npm ci once)
dotnet test src/api/Gabriel.slnx            # runs Gabriel.Tests (xUnit)
npm run typecheck                           # in src/webapp
npm run build                               # in src/webapp
```

See `docs/scripts.md` for the full script reference and encoded gotchas.

## Configuration

- Secrets never live in committed files. `appsettings*.json` are gitignored; `appsettings.example.json` and `.env.example` are the committed templates.
- Local run: user secrets / `appsettings.Development.json`. Docker: `.env` at the repo root (compose is invoked with `--env-file .env` — the compose file lives in `docker/`).
- Key values: `JWT__SIGNINGKEY` (required), `Grok__ApiKey` (xAI provider; the Mock provider needs nothing), `GABRIEL_PROJECTS_ROOT` (project-file storage root).

## Code Style

- Mechanics (LF, UTF-8, indent, braces) are enforced by `.editorconfig` + the Format Check CI gate — don't restate them in review, fix them by running `dotnet format`.
- C#: file-scoped namespaces, async methods end in `Async`, nullable enabled everywhere.
- Comments state constraints the code can't show (the "why"), not narration of the next line.
- Markdown docs: prose-led; bullets for genuinely enumerable facts; absolute dates (`2026-08-11`), never "recently".

## Maintenance Rules

- **Keep `docs/SYSTEMS.md` updated** — any added/removed/renamed controller, service, tool, or provider updates its row in the inventory, same session.
- **Keep `docs/gabriel-self-docs/` truthful** — it is served to the running model via `docs_read`; a stale claim there becomes wrong agent behavior at runtime, not just wrong documentation.
- **Never reference `.dev/` content from public surfaces** — `.dev/` is the local-only operator notebook (deliberate decision, 2026-08-11: the repo is public, session notes stay private). Public docs may not link into it or rely on it existing; describing the convention (as this file does) is fine, depending on the content is not.
- **Document changes as the final step of every session** — in the dated `.dev/sessions/session_YYYY_MM_DD/` folder: what shipped (paths + one-line why), what was found and deferred, decisions made. Not tool-call narration, not a restated diff. "Done" includes "documented".

## Task Delegation Workflow

Work is tracked in the local `.dev/` notebook (see its `README.md` index): `tasks.md` is the single-table backlog spanning all sessions; `bugs/` is the open-defect registry (current state only — fixed bugs leave it); delegation briefs live in `sessions/session_*/tasks/` and are self-contained (objective, context, file paths, acceptance criteria, gotchas) so any agent can start cold. A brief that isn't registered in `tasks.md` doesn't exist.

## Important Notes

- **`dotnet watch` holds DLL locks** — a manual `dotnet build` while the dev server runs fails at the copy step. `scripts/dev.ps1 down` first.
- **The OpenAPI pipeline runs after every API build** (swagger CLI → `swagger.json` → `npm run gen-api`); it needs `src/webapp/node_modules` present, and `SKIP_DB_INIT=true` keeps the brief host instantiation away from the database. Docker builds skip it via `/p:SkipOpenAPI=true`.
- **EF migrations are in-repo** (`Gabriel.Infrastructure/Migrations/`) — schema changes go through `scripts/add-migration.ps1`, never `EnsureCreated`, never hand-edited migration files.
- **`.gemo` files are binary** (`.gitattributes` says so) — the planned Gabriel Sequence identity format with magic + CRC.

## Verification Rules

- **Never claim something works without tracing the full flow.** Code existing ≠ code connected. Critical paths here: the auth chain (JWT cookie → `OnMessageReceived` → refresh rotation), the SSE streaming turn (request → AgentService events → webapp consumer), and tool execution (descriptor → provider → registry → observation persistence).
- **Distinguish shipped from live-validated.** Merged-but-never-run-against-a-real-provider is labelled as such in session notes ("LEFT: live smoke test").
- **Never modify generated files** — `src/webapp/src/api/generated/`, `swagger.json` (both gitignored precisely so drift is impossible), EF migration files after creation. Change the C# source and let the pipeline regenerate; editing output is erased by the next build.
- **Read the design notes before proposing architectural changes** — what looks overcomplicated (e.g. the four-way prompt-block separation) is usually a recorded decision with a rationale.

## Collaboration Rules

- **Agents never run git write operations** — no commit, push, stash, reset, amend. The owner owns all git history and authorship. Staging (`git add`) only when explicitly agreed for the owner to review and commit.
- **Ask before destructive or outward-facing actions** (deleting data, publishing, force operations); reversible in-repo edits that follow from the agreed task proceed without asking.

## Reference Documentation

- `docs/README.md` — documentation map ("start here")
- `docs/SYSTEMS.md` — systems inventory (kept current by rule)
- `docs/conversations-api.md` — full conversations + memories API reference (DTOs, SSE event union)
- `docs/gabriel-self-docs/` — LLM-native docs, runtime source for the `docs_*` tools
- `docs/Gabriel.Engine/` — human-prose deep dives (agent loop, sequence, personality stack)
- `docs/scripts.md` — script reference
