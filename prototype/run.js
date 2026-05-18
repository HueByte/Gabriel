// Usage: node run.js [patternName]
// Generates a fresh frames.json, then plays it.

const path = require('path');
const { spawnSync } = require('child_process');
const args = process.argv.slice(2);

for (const script of ['generate.js', 'play.js']) {
  const scriptArgs = script === 'generate.js' ? args : [];
  const r = spawnSync('node', [path.join(__dirname, script), ...scriptArgs], { stdio: 'inherit' });
  if (r.status !== 0) process.exit(r.status);
}
