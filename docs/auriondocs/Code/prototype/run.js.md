# run

> **File:** `prototype/run.js`  
> **Kind:** file

A small orchestration script that regenerates the frames file and then plays it. Run this when you want a fresh frames.json produced by generate.js (optionally passing a pattern name) and immediately execute play.js to play the generated frames.

## Remarks
This script delegates work to two sibling scripts in the same directory: generate.js and play.js. It runs them synchronously using child_process.spawnSync and propagates any non-zero exit code from the children to the current process, so callers can rely on its exit status to detect failures. Standard input/output/stderr are inherited, so both scripts write directly to the terminal.

## Example
```javascript
// regenerate frames for the "spiral" pattern then play the result
// run from the repository (or the directory containing prototype/)
// shell:
//   node prototype/run.js spiral
```

## Notes
- Only generate.js receives any CLI arguments you pass to run.js; play.js is invoked without arguments.
- The script uses synchronous child processes (spawnSync) and will block until each child completes; its exit code mirrors the first non-zero child exit status.
- Because stdio is inherited, output from the child scripts appears directly in the caller's terminal (no buffering by this wrapper).