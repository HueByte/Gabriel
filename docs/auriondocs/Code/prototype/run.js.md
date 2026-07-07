# run

> **File:** `prototype/run.js`  
> **Kind:** file


Orchestrates two steps: it first generates a fresh frames.json by invoking generate.js (optionally receiving a patternName), then immediately runs play.js to playback the generated frames. It passes the provided patternName only to generate.js and streams the combined output in real time by inheriting stdio, so developers see the full lifecycle from generation to playback in a single command. If either step exits with a non-zero status, run.js terminates with the same code to avoid attempting playback on incomplete data.

## Remarks

Centralizes the workflow of regenerating frames and playing them back into one convenient CLI entry point. This makes it easy to reproduce results for a specific patternName without remembering or typing the two separate commands. The script resolves the target scripts relative to its own directory (using __dirname) so it behaves consistently regardless of the current working directory. By inheriting stdio from the parent process, it preserves the live console output from both generate.js and play.js.

## Example

```javascript
// Example usage: regenerate frames for "myPattern" and play them
node run.js myPattern
```

## Notes

- If generate.js fails, play.js is never invoked.
- The wrapper expects to find generate.js and play.js in the same directory as run.js; if not, it will fail to locate them.
- The patternName argument is forwarded only to generate.js; play.js does not receive any extra arguments.