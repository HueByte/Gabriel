# run

> **File:** `prototype/run.js`  
> **Kind:** file

A tiny CLI orchestration script that runs generate.js to produce a fresh frames.json and then runs play.js to play it. Use this when you want a single command to regenerate frames (optionally with a pattern name) and immediately play the result instead of invoking the two scripts separately.

## Remarks
This file exists as a convenience wrapper that enforces the sequence: generation must complete successfully before playback begins. It runs both child scripts synchronously (blocking) and forwards the parent process's stdio to each child so output and errors appear directly in the console. If either child exits with a non-zero status, this script terminates with the same exit code.

## Example
```javascript
// Run with an optional pattern name forwarded to generate.js
// (from the package root or the prototype directory):
// $ node prototype/run.js spiral
```

## Notes
- Arguments passed on the command line are forwarded only to generate.js; play.js is invoked with no arguments.
- The script uses spawnSync, so it blocks the Node event loop until each child finishes — this is intentional to preserve ordering and exit-code propagation.
- Both generate.js and play.js are expected to live in the same directory as this file; the script resolves them using __dirname.
- Because stdio is inherited, child processes write directly to the parent terminal (no buffering or capturing of output in this script).