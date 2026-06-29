import type { LogEntry } from '@/types';

interface TimelineViewProps {
  entries: LogEntry[];
}

const formatTime = (iso: string) =>
  new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

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
          <p className="mt-1 whitespace-pre-wrap text-sm text-charcoal-700 dark:text-bonewhite-400">
            {entry.content}
          </p>
        </li>
      ))}
    </ol>
  );
};
