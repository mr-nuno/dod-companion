import { useState } from 'react';
import { MarkdownEditor } from '@components/MarkdownEditor';
import { useTimeline } from '@hooks/useTimeline';
import type { LogEntry } from '@/types';

const PREDEFINED_TAGS = ['Strid', 'Loot', 'Event', 'Anteckning', 'Dödsfall'] as const;

const TAG_ACTIVE: Record<string, string> = {
  Strid: 'bg-dodred-500 text-white border-dodred-500',
  Dödsfall: 'bg-dodred-900 text-white border-dodred-900',
  Loot: 'bg-dragongreen-600 text-white border-dragongreen-600',
  Anteckning: 'bg-runecyan-600 text-charcoal-950 border-runecyan-600',
  Event: 'bg-charcoal-600 text-white border-charcoal-600',
};

interface LogEntryFormProps {
  /** When provided, the form edits this entry instead of creating a new one. */
  entry?: LogEntry;
  /** Called after a successful edit save or a cancel (edit mode only). */
  onDone?: () => void;
}

export const LogEntryForm = ({ entry, onDone }: LogEntryFormProps) => {
  const { post, update } = useTimeline();
  const isEdit = entry !== undefined;

  const [title, setTitle] = useState(entry?.title ?? '');
  const [content, setContent] = useState(entry?.content ?? '');
  const [selectedTags, setSelectedTags] = useState<string[]>(entry?.tags ?? []);
  const [submitting, setSubmitting] = useState(false);
  // Bumped after a create to remount the editor and clear its content.
  const [editorKey, setEditorKey] = useState(0);

  const toggleTag = (tag: string) => {
    setSelectedTags((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag],
    );
  };

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const trimmedContent = content.trim();
    const trimmedTitle = title.trim();
    if (!trimmedContent) return;

    setSubmitting(true);
    try {
      if (isEdit) {
        await update(entry.id, trimmedTitle, trimmedContent, selectedTags);
        onDone?.();
      } else {
        await post(trimmedTitle, trimmedContent, selectedTags);
        setTitle('');
        setContent('');
        setSelectedTags([]);
        setEditorKey((k) => k + 1);
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={onSubmit} className="space-y-2">
      <input
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="Title (optional)"
        maxLength={120}
        className="w-full rounded-lg border border-bonewhite-200 bg-bonewhite-50 px-3 py-2 text-base font-semibold text-charcoal-900 outline-none placeholder:font-normal focus:border-dragongreen-500 dark:border-charcoal-600 dark:bg-charcoal-950 dark:text-bonewhite-200"
      />

      <div className="flex flex-wrap gap-1">
        {PREDEFINED_TAGS.map((tag) => {
          const active = selectedTags.includes(tag);
          return (
            <button
              key={tag}
              type="button"
              onClick={() => toggleTag(tag)}
              className={`rounded-full border px-3 py-1 text-xs font-semibold transition ${
                active
                  ? (TAG_ACTIVE[tag] ?? 'bg-charcoal-600 text-white border-charcoal-600')
                  : 'border-bonewhite-300 bg-white text-charcoal-600 hover:border-charcoal-400 dark:border-charcoal-600 dark:bg-charcoal-900 dark:text-bonewhite-300 dark:hover:border-charcoal-400'
              }`}
            >
              {tag}
            </button>
          );
        })}
      </div>

      <MarkdownEditor key={editorKey} value={entry?.content ?? ''} onChange={setContent} />

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={submitting || content.trim().length === 0}
          className="rounded-lg bg-dragongreen-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-dragongreen-500 disabled:opacity-50"
        >
          {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Add to timeline'}
        </button>
        {isEdit && (
          <button
            type="button"
            onClick={() => onDone?.()}
            disabled={submitting}
            className="rounded-lg border border-bonewhite-300 px-4 py-2 text-sm font-semibold text-charcoal-600 transition hover:border-charcoal-400 disabled:opacity-50 dark:border-charcoal-600 dark:text-bonewhite-300"
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
};
