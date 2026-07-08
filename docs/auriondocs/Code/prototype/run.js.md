# run

> **File:** `prototype/run.js`  
> **Kind:** file


run.js is a tiny Node.js harness that orchestrates a two-step workflow: generate frames for a given pattern and then play back those frames. It accepts an optional patternName argument, which it forwards to generate.js to produce a fresh frames.json; after successful generation, it runs play.js without additional arguments to render or replay the generated frames. If either step terminates with a non-zero exit code, run.js exits immediately with that code, propagating the failure to the caller. Use this when you want a repeatable, end-to-end test consisting of generation followed by playback without manual intervention.

## Remarks
This script centralizes the workflow as a single, repeatable command. By using spawnSync and streaming stdio, it preserves the interactive logging of the two sub-scripts while ensuring strict sequential ordering—the playback never starts until generation completes successfully. The small surface area and reliance on the existing scripts keeps concerns localized to orchestration rather than the generation or playback logic.

## Notes
- The optional patternName is only passed to generate.js; playback relies on the freshly produced frames.json.
- If generate.js or play.js fails, run.js exits with the exact exit code, which helps downstream tooling react appropriately.
- Because run.js uses synchronous child process spawning, the Node process will be blocked until each step finishes; long-running steps will appear unresponsive from the perspective of the parent shell until completion.