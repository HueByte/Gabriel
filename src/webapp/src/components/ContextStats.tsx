import { useEffect, useMemo, useState } from 'react';
import { HiOutlineChevronDown } from 'react-icons/hi2';
import { ConversationsService, type ContextMetricsResponse } from '../api/generated';

interface ContextStatsProps {
  conversationId: string;
  /** Bump to trigger a refetch - typically incremented after a chat turn so
   *  the strip reflects the updated context size. */
  refreshKey?: number;
}

// Subtle horizontal strip under the avatar. Shows the next-turn token
// estimate as a fraction of the provider's context window, and (on click)
// expands a 16x16 grid that visualises which categories make up the
// budget - same idea as the Claude Code console indicator.
//
// Each grid cell represents contextWindowTokens / 256 tokens. Categories
// fill the grid in legend order (system → project → memory → summary →
// tools → conversation), then "free" mops up the rest.

function formatTokens(n: number): string {
  if (n >= 1000) {
    // Show one decimal up to 99.9k, then drop it. "12.4k", "187k".
    const k = n / 1000;
    return k >= 100 ? `${Math.round(k)}k` : `${k.toFixed(1)}k`;
  }
  return n.toString();
}

// Legend order is also the grid fill order. Tweaking either re-orders the
// other automatically. Colours are picked to be distinguishable on the
// dark theme without clashing with the palette accent (which the chat bubble
// and the bar already use).
type Category = {
  key: 'system' | 'project' | 'memory' | 'summary' | 'tools' | 'conversation';
  label: string;
  color: string;
  tokens: (m: ContextMetricsResponse) => number;
};

const CATEGORIES: readonly Category[] = [
  { key: 'system',       label: 'System',       color: '#7aa2f7', tokens: m => m.systemPromptTokens },
  { key: 'project',      label: 'Project',      color: '#bb9af7', tokens: m => m.projectPromptTokens },
  { key: 'memory',       label: 'Memory',       color: '#9ece6a', tokens: m => m.memoryTokens },
  { key: 'summary',      label: 'Summary',      color: '#f7768e', tokens: m => m.summaryTokens },
  { key: 'tools',        label: 'Tools',        color: '#e0af68', tokens: m => m.toolsTokens },
  { key: 'conversation', label: 'Conversation', color: '#7dcfff', tokens: m => m.conversationTokens },
];

const GRID_SIZE = 16;
const GRID_CELLS = GRID_SIZE * GRID_SIZE;

// Maps the metrics to a length-256 array of category keys (or null for free).
// Cells are assigned in legend order so adjacent rows stay the same colour
// and the grid reads top-to-bottom like a stacked bar.
function buildGridCells(metrics: ContextMetricsResponse): (Category['key'] | null)[] {
  const window = Math.max(1, metrics.contextWindowTokens);
  const tokensPerCell = window / GRID_CELLS;
  const cells: (Category['key'] | null)[] = new Array(GRID_CELLS).fill(null);

  let cursor = 0;
  for (const cat of CATEGORIES) {
    const tokens = cat.tokens(metrics);
    if (tokens <= 0) continue;
    // Always round up so anything non-zero gets at least one cell - otherwise
    // a 200-token system prompt against a 1M-token window would render
    // invisibly. Worst case we slightly over-allocate cells; the trailing
    // "free" bucket absorbs it.
    const count = Math.max(1, Math.ceil(tokens / tokensPerCell));
    const end = Math.min(GRID_CELLS, cursor + count);
    for (let i = cursor; i < end; i++) cells[i] = cat.key;
    cursor = end;
    if (cursor >= GRID_CELLS) break;
  }
  return cells;
}

export function ContextStats({ conversationId, refreshKey = 0 }: ContextStatsProps) {
  const [metrics, setMetrics] = useState<ContextMetricsResponse | null>(null);
  const [failed, setFailed] = useState(false);
  const [expanded, setExpanded] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setFailed(false);
    ConversationsService.getApiConversationsMetrics({ id: conversationId })
      .then(m => {
        if (!cancelled) setMetrics(m);
      })
      .catch(() => {
        // Stay quiet on error - this is an auxiliary indicator, not load-bearing.
        // A 404 just means the conversation was deleted out from under us, which
        // the chat component will surface separately.
        if (!cancelled) setFailed(true);
      });
    return () => { cancelled = true; };
  }, [conversationId, refreshKey]);

  // Memoise the grid layout so re-renders during expansion don't recompute it.
  // Recomputes only when metrics actually change.
  const gridCells = useMemo(() => metrics ? buildGridCells(metrics) : null, [metrics]);

  if (failed || !metrics) {
    // Reserve no space when there's nothing to show - the strip animates in
    // on its own once metrics land, so the avatar doesn't visually jump.
    return null;
  }

  const { currentTokens, contextWindowTokens, compactThresholdTokens, isSummarized } = metrics;
  const window = Math.max(1, contextWindowTokens);
  const fillPct = Math.min(100, (currentTokens / window) * 100);
  const approaching = compactThresholdTokens > 0
    && currentTokens >= compactThresholdTokens * 0.9;
  const remaining = Math.max(0, compactThresholdTokens - currentTokens);
  const freeTokens = Math.max(0, contextWindowTokens - currentTokens);

  return (
    <div
      className={`ctx-stats${approaching ? ' approaching' : ''}${isSummarized ? ' summarized' : ''}${expanded ? ' expanded' : ''}`}
      role="status"
      aria-label={`Context: ${currentTokens.toLocaleString()} of ${contextWindowTokens.toLocaleString()} tokens used`}
    >
      <div className="ctx-stats-bar">
        <div
          className="ctx-stats-fill"
          style={{ width: `${fillPct}%` }}
        />
      </div>
      <div className="ctx-stats-meta">
        <span className="ctx-stats-tokens">
          {formatTokens(currentTokens)}
          <span className="ctx-stats-sep"> / </span>
          {formatTokens(contextWindowTokens)}
        </span>
        {approaching && (
          <>
            <span className="ctx-stats-dot" aria-hidden="true">·</span>
            <span className="ctx-stats-warning">
              compact in {formatTokens(remaining)}
            </span>
          </>
        )}
        {isSummarized && (
          <>
            <span className="ctx-stats-dot" aria-hidden="true">·</span>
            <span className="ctx-stats-note" title="Earlier turns have been replaced by a rolling summary">
              summarized
            </span>
          </>
        )}
      </div>

      <button
        type="button"
        className="ctx-stats-toggle"
        onClick={() => setExpanded(e => !e)}
        aria-expanded={expanded}
        aria-controls="ctx-stats-details"
        title={expanded ? 'Hide breakdown' : 'Show context breakdown'}
      >
        <span>details</span>
        <HiOutlineChevronDown aria-hidden="true" className="ctx-stats-chev" />
      </button>

      {expanded && gridCells && (
        <div id="ctx-stats-details" className="ctx-stats-details">
          <div
            className="ctx-stats-grid"
            role="img"
            aria-label={`Context breakdown grid: ${formatTokens(Math.round(contextWindowTokens / GRID_CELLS))} per cell`}
          >
            {gridCells.map((key, i) => {
              const color = key ? CATEGORIES.find(c => c.key === key)!.color : undefined;
              return (
                <span
                  key={i}
                  className={`ctx-stats-cell${key ? '' : ' free'}`}
                  style={color ? { backgroundColor: color } : undefined}
                />
              );
            })}
          </div>
          <ul className="ctx-stats-legend">
            {CATEGORIES.map(cat => (
              <li key={cat.key}>
                <span className="ctx-stats-swatch" style={{ backgroundColor: cat.color }} aria-hidden="true" />
                <span className="ctx-stats-legend-label">{cat.label}</span>
                <span className="ctx-stats-legend-tokens">{formatTokens(cat.tokens(metrics))}</span>
              </li>
            ))}
            <li className="ctx-stats-legend-free">
              <span className="ctx-stats-swatch free" aria-hidden="true" />
              <span className="ctx-stats-legend-label">Free</span>
              <span className="ctx-stats-legend-tokens">{formatTokens(freeTokens)}</span>
            </li>
          </ul>
        </div>
      )}
    </div>
  );
}
