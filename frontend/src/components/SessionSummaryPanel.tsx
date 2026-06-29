import { useState } from 'react';
import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { apiClient } from '@services/apiClient';
import type { SessionSummary } from '@/types';

const formatDate = (iso: string) =>
  new Date(iso).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' });

export const SessionSummaryPanel = () => {
  const [summary, setSummary] = useState<SessionSummary | null>(null);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onGenerate = async () => {
    setGenerating(true);
    setError(null);
    try {
      const result = await apiClient.generateSummary();
      setSummary(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not generate summary.');
    } finally {
      setGenerating(false);
    }
  };

  const onDownload = () => {
    if (!summary) return;
    const blob = new Blob([summary.content], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${summary.roomCode.toLowerCase()}-session.md`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center gap-2">
        <button
          onClick={onGenerate}
          disabled={generating}
          className="rounded-lg bg-charcoal-700 px-4 py-2 text-sm font-semibold text-white transition hover:bg-charcoal-600 disabled:opacity-50 dark:bg-charcoal-600 dark:hover:bg-charcoal-500"
        >
          {generating ? 'Generating…' : summary ? 'Regenerate' : 'Generate Summary'}
        </button>

        {summary && (
          <button
            onClick={onDownload}
            className="rounded-lg border border-bonewhite-300 bg-white px-4 py-2 text-sm font-semibold text-charcoal-600 transition hover:border-charcoal-400 dark:border-charcoal-600 dark:bg-charcoal-900 dark:text-bonewhite-300 dark:hover:border-charcoal-400"
          >
            Download .md
          </button>
        )}
      </div>

      {error && <p className="text-sm text-dodred-600 dark:text-dodred-300">{error}</p>}

      {summary && (
        <div className="space-y-2">
          <p className="text-xs text-charcoal-400 dark:text-bonewhite-300/60">
            {summary.entryCount} {summary.entryCount === 1 ? 'entry' : 'entries'} · Generated {formatDate(summary.generatedAt)}
          </p>
          <div className="max-h-96 overflow-y-auto rounded-lg border border-bonewhite-200 bg-white p-4 dark:border-charcoal-700 dark:bg-charcoal-900/60">
            <div className="prose prose-sm max-w-none dark:prose-invert">
              <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
                {summary.content}
              </Markdown>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
