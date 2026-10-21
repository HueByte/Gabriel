# Feature comparison

Where Gabriel stands against the agents people actually use day to day, and what it would take to close the gaps that matter for its mission: a generalist personal assistant that can **talk, do tasks, and learn and teach**. Compiled 2026-10-21 against the code on `dev`.

The comparison set is deliberately mixed. ChatGPT and the Claude apps are the consumer-assistant benchmark, Claude Code and Codex CLI set the bar for an agent that acts on files and shells, and Open WebUI / LibreChat are the closest self-hosted relatives. Competitor columns summarize publicly documented capabilities as of the compile date and are coarse on purpose; check the vendor's docs before quoting any single cell.

Legend: ✅ shipped · 🟡 partial or opt-in · ❌ absent.

## Talk — conversation quality and UX

| Capability | Gabriel | ChatGPT | Claude apps | Claude Code / Codex | Open WebUI / LibreChat |
| --- | --- | --- | --- | --- | --- |
| Streaming replies with visible reasoning | ✅ separate `reasoning_content` and ReAct thought channels | ✅ | ✅ | ✅ | ✅ |
| Regenerate with variant history | ✅ variant picker, delete-from-here | ✅ | ✅ | ❌ | ✅ |
| Edit a sent message and re-run | ❌ | ✅ | ✅ | 🟡 rewind | ✅ |
| Stop a reply mid-stream | ❌ aborts only on conversation switch | ✅ | ✅ | ✅ | ✅ |
| Rich rendering (markdown, code, math, diagrams) | ✅ highlight.js, KaTeX, mermaid | ✅ | ✅ | 🟡 terminal | ✅ |
| Behavior modes (concise, tutor, critic, …) | ✅ five per-conversation modes | 🟡 personalities | 🟡 styles | ❌ | 🟡 presets |
| Auto-titled conversations | ❌ manual rename only | ✅ | ✅ | n/a | ✅ |
| Search and export conversations | ❌ | ✅ | ✅ | 🟡 resume | ✅ |
| Voice input / output | ❌ | ✅ | ✅ | ❌ | ✅ |
| Image input (vision) | ❌ | ✅ | ✅ | ✅ | ✅ |
| Distinct identity / avatar | ✅ Gabriel Sequence, seed-stable per project | ❌ | ❌ | ❌ | ❌ |
| Mobile-friendly layout | 🟡 narrow-viewport sidebar unpolished | ✅ apps | ✅ apps | n/a | ✅ |

## Do tasks — tools and autonomy

| Capability | Gabriel | ChatGPT | Claude apps | Claude Code / Codex | Open WebUI / LibreChat |
| --- | --- | --- | --- | --- | --- |
| Multi-step tool loop | ✅ ReAct, capped by `Agent:MaxIterations` | ✅ | ✅ | ✅ | 🟡 |
| Parallel tool calls | ✅ `IsParallelSafe` fan-out | ✅ | ✅ | ✅ | 🟡 |
| Task plan / todo list | ✅ `todo_write` / `todo_read` | 🟡 agent mode | 🟡 | ✅ | ❌ |
| Web search + page fetch | ✅ DDG / Brave / Tavily, SSRF-guarded fetch | ✅ | ✅ | ✅ | ✅ |
| Deep research (long multi-source reports) | ❌ | ✅ | ✅ | ❌ | 🟡 |
| Read files | ✅ project files + path-hardened `list_dir` / `find` / `grep` | ✅ uploads | ✅ uploads | ✅ | ✅ |
| Write / edit files | ❌ | 🟡 canvas | 🟡 artifacts | ✅ | ❌ |
| Shell / code execution | 🟡 `shell_execute`, off by default, all-or-nothing | ✅ sandbox | ✅ sandbox | ✅ | 🟡 |
| Per-action approval prompts | ❌ | 🟡 | 🟡 | ✅ | ❌ |
| PDF / DOCX understanding | ❌ upload accepted, read refused | ✅ | ✅ | ✅ | ✅ RAG |
| External connectors / MCP | ❌ | ✅ | ✅ | ✅ | ✅ |
| Sub-agents / delegation | ❌ | 🟡 | 🟡 | ✅ | ❌ |
| Scheduled / background tasks, reminders | ❌ | ✅ | 🟡 | 🟡 | ❌ |
| Calendar, email, personal accounts | ❌ | ✅ connectors | ✅ connectors | 🟡 MCP | 🟡 tools |
| Image generation | ❌ | ✅ | ❌ | ❌ | ✅ |

## Learn and teach — memory and knowledge

| Capability | Gabriel | ChatGPT | Claude apps | Claude Code / Codex | Open WebUI / LibreChat |
| --- | --- | --- | --- | --- | --- |
| Long-term memory | ✅ user + project scope, user-curated | ✅ | ✅ | 🟡 files | 🟡 |
| Semantic recall over memory | 🟡 Qdrant + local ONNX embeddings, off by default | ✅ | ✅ | ❌ | ✅ |
| Projects with own prompt + files | ✅ project-scoped by construction | ✅ | ✅ | ✅ repo | ✅ |
| RAG over uploaded documents | ❌ files are read whole, text only | ✅ | ✅ | 🟡 | ✅ |
| Context management on long chats | ✅ rolling-summary auto-compact with live overlay | ✅ | ✅ | ✅ | 🟡 |
| Token budget visibility | ✅ per-bucket context breakdown | ❌ | 🟡 | ✅ | 🟡 |
| Self-knowledge (docs about itself) | ✅ `docs_list` / `docs_read` over LLM-native self-docs | ❌ | ❌ | 🟡 | ❌ |
| Tutoring mode | 🟡 `tutor` mode prompt | ✅ study mode | ✅ learning style | ❌ | ❌ |
| Quizzes, flashcards, spaced repetition | ❌ | 🟡 | 🟡 | ❌ | ❌ |

## Platform

| Capability | Gabriel | ChatGPT | Claude apps | Claude Code / Codex | Open WebUI / LibreChat |
| --- | --- | --- | --- | --- | --- |
| Model / provider choice | 🟡 Grok + Mock; emulated tool bridge for text-only models | ❌ | ❌ | ❌ | ✅ any OpenAI-compatible, Ollama |
| Local / offline models | ❌ bridge exists, no Ollama provider yet | ❌ | ❌ | ❌ | ✅ |
| Self-hosted, own data | ✅ SQLite + Docker compose | ❌ | ❌ | 🟡 local CLI | ✅ |
| Multi-user auth | ✅ Identity + JWT cookies, refresh rotation | ✅ | ✅ | n/a | ✅ |
| Accurate token counting | 🟡 chars/4 estimate | ✅ | ✅ | ✅ | 🟡 |
| Prompt caching | ❌ prompt order not yet cache-friendly | ✅ | ✅ | ✅ | 🟡 |
| Usage diagnostics | ✅ web-search metrics page | 🟡 | 🟡 | ✅ | ✅ |

## What to build next

Gabriel's strengths are real but narrow. The agent loop, memory model, context accounting, and self-documentation are on par with or ahead of the consumer apps. Its identity layer has no equivalent anywhere in the comparison set. The gaps cluster in three places: Gabriel can only think with one vendor's models, it can look but not touch, and it can't take in the documents and images people actually learn from. Ranked by leverage against the mission, roughly cheapest-high-impact first:

1. **OpenAI-compatible provider.** One `IChatProvider` over the `/v1/chat/completions` shape covers OpenAI, OpenRouter, Ollama, LM Studio, and vLLM in one move. Pointed at Ollama, it is also the first real exercise of the emulated tool bridge. This is the single biggest gap against Open WebUI.
2. **Stop button, edit-and-resend, auto-titles.** Three small UI and loop changes that people expect from every chat app. The abort plumbing already exists in `Chat.tsx`, it just isn't exposed.
3. **Per-call tool approval.** A pending-approval state in the loop and an approve/deny card in the UI. This is what makes it safe to turn `shell_execute` on by default, and it is the prerequisite for anything that writes.
4. **Project-scoped write and edit tools.** `write_file` / `edit_file` behind the same path hardening as the read tools and gated by approval, so Gabriel can produce artifacts instead of pasting them.
5. **Document ingestion.** Text extraction for PDF and DOCX at upload time, then chunked retrieval through the existing embedding pipeline. That turns semantic memory infrastructure into RAG over project files, which is the core of "learn".
6. **MCP client.** Mounting MCP servers as `ITool`s buys calendar, email, GitHub, and everything else in the ecosystem without bespoke integrations — the fastest route to personal-assistant reach.
7. **Image input.** Multimodal message content through providers that support it, so screenshots, diagrams, and photos of homework become first-class.
8. **Teaching loop.** Building on `tutor` mode with quiz and flashcard tools backed by memory (what the user got wrong, when to revisit). None of the comparison set does spaced repetition well, so this is where Gabriel can lead rather than catch up.
9. **Scheduled tasks and reminders.** A background runner that starts agent turns on a timer, the piece that makes an assistant proactive.
10. **Real tokenizer, then prompt caching.** Precision for the compact trigger first, then a cache-friendly prompt order (stable persona ahead of per-turn state) once real-workload cost is measured.

Voice, image generation, sub-agents, and deep research are deliberately left off this list. Each is worth having, but each depends on items above it (provider breadth, approvals, document handling) and none moves the talk / do / learn needle as much per unit of work.
