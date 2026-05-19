import { useEffect, useState } from 'react';
import { ConversationsService, type ContextMetricsResponse } from '../api/generated';

interface ContextStatsProps {
  conversationId: string;
  /** Bump to trigger a refetch — typically incremented after a chat turn so
   *  the strip reflects the updated context size. */
  refreshKey?: number;
}

// Subtle horizontal strip under the avatar. Shows the next-turn token
// estimate as a fraction of the provider's context window. Stays
// minimal — just the bar + raw token count — until we're within 90% of the
// auto-compact threshold, at which point the bar tints to the palette
// accent and a "compact in Xk" warning appears so the trigger isn't a
// surprise.

function formatTokens(n: number): string {
  if (n >= 1000) {
    // Show one decimal up to 99.9k, then drop it. "12.4k", "187k".
    const k = n / 1000;
    return k >= 100 ? `${Math.round(k)}k` : `${k.toFixed(1)}k`;
  }
  return n.toString();
}

export function ContextStats({ conversationId, refreshKey = 0 }: ContextStatsProps) {
  const [metrics, setMetrics] = useState<ContextMetricsResponse | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setFailed(false);
    ConversationsService.getApiConversationsMetrics({ id: conversationId })
      .then(m => {
        if (!cancelled) setMetrics(m);
      })
      .catch(() => {
        // Stay quiet on error — this is an auxiliary indicator, not load-bearing.
        // A 404 just means the conversation was deleted out from under us, which
        // the chat component will surface separately.
        if (!cancelled) setFailed(true);
      });
    return () => { cancelled = true; };
  }, [conversationId, refreshKey]);

  if (failed || !metrics) {
    // Reserve no space when there's nothing to show — the strip animates in
    // on its own once metrics land, so the avatar doesn't visually jump.
    return null;
  }

  const { currentTokens, contextWindowTokens, compactThresholdTokens, isSummarized } = metrics;
  const window = Math.max(1, contextWindowTokens);
  const fillPct = Math.min(100, (currentTokens / window) * 100);
  // "Approaching compact" once we're within 90% of the trigger. Both the bar
  // tint and the warning label react to this so visual + textual cues flip
  // together.
  const approaching = compactThresholdTokens > 0
    && currentTokens >= compactThresholdTokens * 0.9;
  const remaining = Math.max(0, compactThresholdTokens - currentTokens);

  return (
    <div
      className={`ctx-stats${approaching ? ' approaching' : ''}${isSummarized ? ' summarized' : ''}`}
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
    </div>
  );
}
