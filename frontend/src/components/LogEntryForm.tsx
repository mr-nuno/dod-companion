import { useState } from 'react';
import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeSanitize from 'rehype-sanitize';
import { useTimeline } from '@hooks/useTimeline';

const PREDEFINED_TAGS = ['Strid', 'Loot', 'Event', 'Anteckning', 'Dödsfall'] as const;

const TAG_ACTIVE: Record<string, string> = {
  Strid: 'bg-dodred-500 text-white border-dodred-500',
  Dödsfall: 'bg-dodred-900 text-white border-dodred-900',
  Loot: 'bg-dragongreen-600 text-white border-dragongreen-600',
  Anteckning: 'bg-runecyan-600 text-charcoal-950 border-runecyan-600',
  Event: 'bg-charcoal-600 text-white border-charcoal-600',
};

export const LogEntryForm = () => {
  const { post } = useTimeline();
  const [content, setContent] = useState('');
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [mode, setMode] = useState<'write' | 'preview'>('write');
  const [submitting, setSubmitting] = useState(false);

  const toggleTag = (tag: string) => {
    setSelectedTags((prev) =>
      prev.includes(tag) ? prev.filter((t) => t !== tag) : [...prev, tag],
    );
  };

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const trimmed = content.trim();
    if (!trimmed) return;

    setSubmitting(true);
    try {
      await post(trimmed, selectedTags);
      setContent('');
      setSelectedTags([]);
      setMode('write');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={onSubmit} className="space-y-2">
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

      <div className="overflow-hidden rounded-lg border border-bonewhite-200 dark:border-charcoal-600">
        <div className="flex border-b border-bonewhite-200 dark:border-charcoal-600">
          {(['write', 'preview'] as const).map((m) => (
            <button
              key={m}
              type="button"
              onClick={() => setMode(m)}
              className={`px-4 py-1.5 text-xs font-semibold capitalize transition ${
                mode === m
                  ? 'bg-bonewhite-100 text-charcoal-900 dark:bg-charcoal-800 dark:text-bonewhite-200'
                  : 'text-charcoal-400 hover:text-charcoal-700 dark:text-bonewhite-400 dark:hover:text-bonewhite-200'
              }`}
            >
              {m}
            </button>
          ))}
        </div>

        {mode === 'write' ? (
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Log an event, note, or loot… (markdown supported)"
            rows={3}
            className="w-full resize-none bg-bonewhite-50 px-3 py-2 text-sm text-charcoal-900 outline-none dark:bg-charcoal-950 dark:text-bonewhite-500"
          />
        ) : (
          <div className="prose prose-sm min-h-[5rem] max-w-none px-3 py-2 dark:prose-invert">
            {content.trim() ? (
              <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeSanitize]}>
                {content}
              </Markdown>
            ) : (
              <span className="text-charcoal-400 dark:text-bonewhite-300/40">Nothing to preview.</span>
            )}
          </div>
        )}
      </div>

      <button
        type="submit"
        disabled={submitting || content.trim().length === 0}
        className="rounded-lg bg-dragongreen-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-dragongreen-500 disabled:opacity-50"
      >
        {submitting ? 'Logging…' : 'Add to timeline'}
      </button>
    </form>
  );
};
