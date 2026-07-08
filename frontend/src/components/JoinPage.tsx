import { useState } from 'react';
import { useLocation } from 'wouter';
import { useSession } from '@hooks/useSession';
import { ThemeToggle } from '@components/ThemeToggle';

const inputClass =
  'w-full rounded-xl border-2 border-bonewhite-200 bg-bonewhite-50/50 px-4 py-3 text-lg text-charcoal-900 outline-none transition-colors focus:border-dodred-500 dark:border-charcoal-700 dark:bg-charcoal-950/50 dark:text-bonewhite-500 dark:focus:border-dodred-500';
const labelClass =
  'text-sm font-semibold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400';
const primaryButtonClass =
  'w-full rounded-xl bg-gradient-to-r from-dodred-500 to-dodred-600 px-4 py-3 text-lg font-bold tracking-wide text-white shadow-lg shadow-dodred-500/30 transition-all hover:scale-[1.02] hover:shadow-dodred-500/50 disabled:pointer-events-none disabled:opacity-50';

interface JoinPageProps {
  token: string;
}

export const JoinPage = ({ token }: JoinPageProps) => {
  const { join } = useSession();
  const [, navigate] = useLocation();

  const [playerName, setPlayerName] = useState('');
  const [kp, setKp] = useState<number>(10);
  const [upptackFara, setUpptackFara] = useState<number>(10);
  const [finnaDoldaTing, setFinnaDoldaTing] = useState<number>(10);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await join(token, playerName.trim(), kp, upptackFara, finnaDoldaTing, false);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not join.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="relative flex min-h-full items-center justify-center bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-bonewhite-200 via-bonewhite-100 to-bonewhite-50 p-6 dark:from-charcoal-800 dark:via-charcoal-900 dark:to-charcoal-950">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>

      <form
        onSubmit={onSubmit}
        className="w-full max-w-md space-y-6 rounded-3xl border border-bonewhite-200 bg-white/80 p-6 sm:p-10 shadow-2xl backdrop-blur-sm transition-all dark:border-charcoal-700/50 dark:bg-charcoal-900/60 dark:shadow-[0_0_40px_-10px_rgba(227,28,35,0.3)]"
      >
        <div className="space-y-4 text-center">
          <img
            src="/dod.png"
            alt="Drakar och Demoner"
            className="mx-auto h-auto w-full max-w-[20rem] drop-shadow-md transition-transform hover:scale-105"
          />
          <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">Join the session</p>
        </div>

        <label className="block space-y-2">
          <span className={labelClass}>Player name</span>
          <input
            value={playerName}
            onChange={(e) => setPlayerName(e.target.value)}
            placeholder="Aragorn"
            className={inputClass}
            required
          />
        </label>

        <div className="grid grid-cols-3 gap-3">
          <label className="block space-y-2">
            <span className={labelClass}>KP</span>
            <input
              type="number"
              min="1"
              value={kp}
              onChange={(e) => setKp(Math.max(1, Number(e.target.value)))}
              className={inputClass}
              required
            />
          </label>
          <label className="block space-y-2 cursor-help" title="Upptäcka fara">
            <span className={labelClass}>UF</span>
            <input
              type="number"
              min="1"
              value={upptackFara}
              onChange={(e) => setUpptackFara(Math.max(1, Number(e.target.value)))}
              className={inputClass}
              required
            />
          </label>
          <label className="block space-y-2 cursor-help" title="Finna dolda ting">
            <span className={labelClass}>FDT</span>
            <input
              type="number"
              min="1"
              value={finnaDoldaTing}
              onChange={(e) => setFinnaDoldaTing(Math.max(1, Number(e.target.value)))}
              className={inputClass}
              required
            />
          </label>
        </div>

        {error && (
          <div className="rounded-lg bg-dodred-50 p-3 text-sm font-medium text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
            {error}
          </div>
        )}

        <button type="submit" disabled={submitting} className={primaryButtonClass}>
          {submitting ? 'Joining…' : 'Join'}
        </button>
      </form>
    </div>
  );
};
