import { useState } from 'react';
import { useSession } from '@hooks/useSession';
import { ThemeToggle } from '@components/ThemeToggle';

const initialForm = { roomCode: '', playerName: '' };

export const JoinPage = () => {
  const { join } = useSession();
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const setField = <K extends keyof typeof initialForm>(field: K, value: (typeof initialForm)[K]) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await join(form.roomCode, form.playerName);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not join the session.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="relative flex min-h-full items-center justify-center p-6">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>

      <form
        onSubmit={onSubmit}
        className="w-full max-w-sm space-y-5 rounded-2xl border border-bonewhite-200 bg-white p-8 shadow-xl dark:border-charcoal-700 dark:bg-charcoal-900/80 dark:shadow-2xl dark:shadow-dodred-900/30"
      >
        <div className="space-y-3 text-center">
          <img src="/dod.png" alt="Drakar och Demoner" className="mx-auto h-auto w-full max-w-[16rem]" />
          <p className="text-sm text-charcoal-500 dark:text-bonewhite-300">
            Enter a room code to join your table.
          </p>
        </div>

        <label className="block space-y-1">
          <span className="text-sm font-medium text-charcoal-700 dark:text-bonewhite-400">Room code</span>
          <input
            value={form.roomCode}
            onChange={(e) => setField('roomCode', e.target.value)}
            placeholder="DRAGON"
            autoCapitalize="characters"
            className="w-full rounded-lg border border-bonewhite-200 bg-bonewhite-50 px-3 py-2 uppercase tracking-widest text-charcoal-900 outline-none focus:border-dodred-500 dark:border-charcoal-600 dark:bg-charcoal-950 dark:text-bonewhite-500"
            required
          />
        </label>

        <label className="block space-y-1">
          <span className="text-sm font-medium text-charcoal-700 dark:text-bonewhite-400">Player name</span>
          <input
            value={form.playerName}
            onChange={(e) => setField('playerName', e.target.value)}
            placeholder="Aragorn"
            className="w-full rounded-lg border border-bonewhite-200 bg-bonewhite-50 px-3 py-2 text-charcoal-900 outline-none focus:border-dodred-500 dark:border-charcoal-600 dark:bg-charcoal-950 dark:text-bonewhite-500"
            required
          />
        </label>

        {error && <p className="text-sm text-dodred-600 dark:text-dodred-300">{error}</p>}

        <button
          type="submit"
          disabled={submitting}
          className="w-full rounded-lg bg-dodred-500 px-4 py-2 font-semibold text-white transition hover:bg-dodred-600 disabled:opacity-50"
        >
          {submitting ? 'Joining…' : 'Join session'}
        </button>
      </form>
    </div>
  );
};
