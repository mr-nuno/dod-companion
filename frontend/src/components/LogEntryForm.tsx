import { useState } from 'react';
import { useTimeline } from '@hooks/useTimeline';

export const LogEntryForm = () => {
  const { post } = useTimeline();
  const [content, setContent] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const trimmed = content.trim();
    if (!trimmed) {
      return;
    }

    setSubmitting(true);
    try {
      await post(trimmed);
      setContent('');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={onSubmit} className="space-y-2">
      <textarea
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder="Log an event, note, or loot…"
        rows={2}
        className="w-full resize-none rounded-lg border border-bonewhite-200 bg-bonewhite-50 px-3 py-2 text-sm text-charcoal-900 outline-none focus:border-dragongreen-500 dark:border-charcoal-600 dark:bg-charcoal-950 dark:text-bonewhite-500"
      />
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
