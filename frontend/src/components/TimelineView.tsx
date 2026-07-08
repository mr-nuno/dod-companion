import { useState } from 'react';
import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { LogEntryForm } from '@components/LogEntryForm';
import { ConfirmModal } from '@components/ConfirmModal';
import { useSession } from '@hooks/useSession';
import { useTimeline } from '@hooks/useTimeline';
import type { LogEntry } from '@/types';

interface TimelineViewProps {
  entries: LogEntry[];
}

const formatDate = (iso: string) =>
  new Date(iso).toLocaleString([], {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });

const TAG_COLORS: Record<string, string> = {
  Strid: 'bg-dodred-500/15 text-dodred-700 dark:text-dodred-400',
  Dödsfall: 'bg-dodred-900/20 text-dodred-900 dark:text-dodred-300',
  Loot: 'bg-dragongreen-500/15 text-dragongreen-700 dark:text-dragongreen-400',
  Anteckning: 'bg-runecyan-500/15 text-runecyan-700 dark:text-runecyan-400',
  Event: 'bg-charcoal-200/60 text-charcoal-700 dark:bg-charcoal-700/40 dark:text-bonewhite-300',
  info: 'bg-bonewhite-300/60 text-charcoal-600 dark:bg-charcoal-800/60 dark:text-bonewhite-400',
};

const tagColor = (tag: string) =>
  TAG_COLORS[tag] ?? 'bg-bonewhite-200 text-charcoal-600 dark:bg-charcoal-700 dark:text-bonewhite-400';

// Fallback gradients per banner key, shown behind (and instead of) the image asset.
const BANNER_GRADIENTS: Record<string, string> = {
  ruins: 'from-charcoal-500 to-charcoal-900',
  forest: 'from-dragongreen-700 to-charcoal-900',
  dungeon: 'from-charcoal-700 to-charcoal-950',
  tavern: 'from-dodred-800 to-charcoal-900',
  battlefield: 'from-dodred-700 to-charcoal-900',
  cave: 'from-runecyan-800 to-charcoal-950',
};

const HeroBanner = ({ banner }: { banner: string }) => {
  const [failed, setFailed] = useState(false);
  const gradient = BANNER_GRADIENTS[banner] ?? 'from-charcoal-600 to-charcoal-900';

  return (
    <div className={`relative h-28 w-full overflow-hidden rounded-t-lg bg-gradient-to-br ${gradient}`}>
      {!failed && (
        <img
          src={`/banners/${banner}.jpg`}
          alt=""
          onError={() => setFailed(true)}
          className="h-full w-full object-cover"
        />
      )}
    </div>
  );
};

const SystemRow = ({ entry }: { entry: LogEntry }) => (
  <li className="px-1 py-1 text-center text-xs text-charcoal-400 dark:text-bonewhite-300/50">
    <span className="font-semibold text-dragongreen-600 dark:text-dragongreen-400">{entry.playerName}</span>{' '}
    {entry.content}
  </li>
);

interface ArticleProps {
  entry: LogEntry;
  isOwn: boolean;
  onDelete: (id: string) => void;
}

const Article = ({ entry, isOwn, onDelete }: ArticleProps) => {
  const [menuOpen, setMenuOpen] = useState(false);
  const [editing, setEditing] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);

  if (editing) {
    return (
      <li className="rounded-lg border border-bonewhite-200 bg-white p-3 dark:border-charcoal-700 dark:bg-charcoal-900/60">
        <LogEntryForm entry={entry} onDone={() => setEditing(false)} />
      </li>
    );
  }

  return (
    <li
      className={`relative rounded-lg border border-bonewhite-200 bg-white dark:border-charcoal-700 dark:bg-charcoal-900/60 ${
        menuOpen ? 'z-30' : ''
      }`}
    >
      {entry.title && entry.heroImage && <HeroBanner banner={entry.heroImage} />}

      <div className="p-4">
        <div className="flex items-start justify-between gap-2">
          <div>
            {entry.title && (
              <h3 className="text-lg font-bold leading-tight text-charcoal-900 dark:text-bonewhite-100">
                {entry.title}
              </h3>
            )}
            <p className="mt-0.5 text-xs text-charcoal-400 dark:text-bonewhite-300/60">
              <span className="font-semibold text-dragongreen-600 dark:text-dragongreen-400">
                {entry.playerName}
              </span>
              {' · '}
              {formatDate(entry.timestamp)}
              {entry.updatedAt && ' · edited'}
            </p>
          </div>

          {isOwn && (
            <div className="relative shrink-0">
              <button
                type="button"
                onClick={() => setMenuOpen((o) => !o)}
                aria-label="Post actions"
                className="rounded px-2 py-1 text-charcoal-400 transition hover:bg-bonewhite-200 dark:text-bonewhite-400 dark:hover:bg-charcoal-700"
              >
                ⋯
              </button>
              {menuOpen && (
                <div className="absolute right-0 z-50 mt-1 w-28 overflow-hidden rounded-lg border border-bonewhite-200 bg-white shadow-xl dark:border-charcoal-600 dark:bg-charcoal-800">
                  <button
                    type="button"
                    onClick={() => {
                      setMenuOpen(false);
                      setEditing(true);
                    }}
                    className="block w-full px-3 py-2 text-left text-sm text-charcoal-700 hover:bg-bonewhite-100 dark:text-bonewhite-200 dark:hover:bg-charcoal-700"
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setMenuOpen(false);
                      setConfirmDelete(true);
                    }}
                    className="block w-full px-3 py-2 text-left text-sm text-dodred-600 hover:bg-bonewhite-100 dark:text-dodred-400 dark:hover:bg-charcoal-700"
                  >
                    Delete
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        <div className="prose prose-sm mt-2 max-w-none dark:prose-invert">
          <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
            {entry.content}
          </Markdown>
        </div>

        {entry.tags?.length > 0 && (
          <div className="mt-3 flex flex-wrap gap-1">
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
      </div>

      <ConfirmModal
        isOpen={confirmDelete}
        title="Delete Chronicle Entry?"
        description="Are you sure you want to permanently remove this entry from the session timeline? This action cannot be undone."
        confirmLabel="Delete Entry"
        cancelLabel="Cancel"
        onConfirm={() => {
          setConfirmDelete(false);
          onDelete(entry.id);
        }}
        onClose={() => setConfirmDelete(false)}
      >
        {entry.title && (
          <div className="font-bold text-charcoal-900 dark:text-white">
            {entry.title}
          </div>
        )}
        <div className={`line-clamp-2 text-charcoal-700 dark:text-bonewhite-100 ${entry.title ? 'mt-1' : ''}`}>
          {entry.content}
        </div>
      </ConfirmModal>
    </li>
  );
};

export const TimelineView = ({ entries }: TimelineViewProps) => {
  const { session } = useSession();
  const { remove } = useTimeline();

  if (entries.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-charcoal-400 dark:text-bonewhite-300/60">
        No events logged yet.
      </p>
    );
  }

  return (
    <ol className="space-y-4">
      {entries.map((entry) =>
        entry.id.startsWith('join-') ? (
          <SystemRow key={entry.id} entry={entry} />
        ) : (
          <Article
            key={entry.id}
            entry={entry}
            isOwn={entry.playerName === session?.playerName}
            onDelete={remove}
          />
        ),
      )}
    </ol>
  );
};
