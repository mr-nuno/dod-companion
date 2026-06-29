import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import type { LogEntry } from '@/types';

interface TimelineViewProps {
  entries: LogEntry[];
}

const formatTime = (iso: string) =>
  new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

const TAG_COLORS: Record<string, string> = {
  Strid: 'bg-dodred-500/15 text-dodred-700 dark:text-dodred-400',
  Dödsfall: 'bg-dodred-900/20 text-dodred-900 dark:text-dodred-300',
  Loot: 'bg-dragongreen-500/15 text-dragongreen-700 dark:text-dragongreen-400',
  Anteckning: 'bg-runecyan-500/15 text-runecyan-700 dark:text-runecyan-400',
  Event: 'bg-charcoal-200/60 text-charcoal-700 dark:bg-charcoal-700/40 dark:text-bonewhite-300',
};

const tagColor = (tag: string) =>
  TAG_COLORS[tag] ?? 'bg-bonewhite-200 text-charcoal-600 dark:bg-charcoal-700 dark:text-bonewhite-400';

export const TimelineView = ({ entries }: TimelineViewProps) => {
  if (entries.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-charcoal-400 dark:text-bonewhite-300/60">
        No events logged yet.
      </p>
    );
  }

  return (
    <ol className="space-y-3">
      {entries.map((entry) => (
        <li
          key={entry.id}
          className="rounded-lg border border-bonewhite-200 bg-white p-3 dark:border-charcoal-700 dark:bg-charcoal-900/60"
        >
          <div className="flex items-baseline justify-between gap-2">
            <span className="font-semibold text-dragongreen-600 dark:text-dragongreen-400">
              {entry.playerName}
            </span>
            <time className="text-xs text-charcoal-400 dark:text-bonewhite-300/60">
              {formatTime(entry.timestamp)}
            </time>
          </div>
          <div className="prose prose-sm mt-1 max-w-none dark:prose-invert">
            <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
              {entry.content}
            </Markdown>
          </div>
          {entry.tags?.length > 0 && (
            <div className="mt-2 flex flex-wrap gap-1">
              {entry.tags.map((tag) => (
                <span
                  key={tag}
                  className={`rounded-full px-2 py-0.5 text-xs font-semibold ${tagColor(tag)}`}
                >
                  {tag}
                </span>
              ))}
            </div>
          )}
        </li>
      ))}
    </ol>
  );
};
