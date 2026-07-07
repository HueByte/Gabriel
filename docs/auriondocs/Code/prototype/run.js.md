# run

> **File:** `prototype/run.js`  
> **Kind:** file


Runs two Node.js scripts in sequence to regenerate and play a frames sequence. It first executes generate.js with an optional patternName argument to create a fresh frames.json, then executes play.js to play the generated frames. If either step exits with a non-zero status, run terminates with that status, ensuring a failure at generation or playback stops the workflow.

## Remarks
Acts as a small orchestration utility that binds generation and playback into a single command. By using `__dirname` to locate the scripts and `spawnSync` with `stdio: 'inherit'`, it preserves the user’s console I/O and ensures sequential progression. This separation allows developers to reuse `generate.js` and `play.js` independently while offering a convenient default workflow.