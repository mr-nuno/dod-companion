import { useState } from 'react';
import { useSession } from '@hooks/useSession';
import { ThemeToggle } from '@components/ThemeToggle';

const inputClass =
  'w-full rounded-xl border-2 border-bonewhite-200 bg-bonewhite-50/50 px-4 py-3 text-lg text-charcoal-900 outline-none transition-colors focus:border-dodred-500 dark:border-charcoal-700 dark:bg-charcoal-950/50 dark:text-bonewhite-500 dark:focus:border-dodred-500';
const labelClass =
  'text-sm font-semibold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400';
const primaryButtonClass =
  'w-full rounded-xl bg-gradient-to-r from-dodred-500 to-dodred-600 px-4 py-3 text-lg font-bold tracking-wide text-white shadow-lg shadow-dodred-500/30 transition-all hover:scale-[1.02] hover:shadow-dodred-500/50 disabled:pointer-events-none disabled:opacity-50';

export const StartPage = () => {
  const { requestCreateLink } = useSession();

  const [email, setEmail] = useState('');
  const [roomName, setRoomName] = useState('');
  const [sentTo, setSentTo] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await requestCreateLink(email.trim(), roomName.trim());
      setSentTo(email.trim());
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not send the link.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="relative flex min-h-full items-center justify-center bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-bonewhite-200 via-bonewhite-100 to-bonewhite-50 p-6 dark:from-charcoal-800 dark:via-charcoal-900 dark:to-charcoal-950">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>

      <div className="w-full max-w-md space-y-6 rounded-3xl border border-bonewhite-200 bg-white/80 p-6 sm:p-10 shadow-2xl backdrop-blur-sm transition-all dark:border-charcoal-700/50 dark:bg-charcoal-900/60 dark:shadow-[0_0_40px_-10px_rgba(227,28,35,0.3)]">
        <div className="space-y-4 text-center">
          <img
            src="/dod.png"
            alt="Drakar och Demoner"
            className="mx-auto h-auto w-full max-w-[20rem] drop-shadow-md transition-transform hover:scale-105"
          />
          <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">Start a session</p>
        </div>

        {sentTo ? (
          <div className="space-y-4 text-center">
            <div className="rounded-2xl bg-dragongreen-50 p-5 text-dragongreen-800 dark:bg-dragongreen-900/20 dark:text-dragongreen-300">
              <p className="text-lg font-bold">Check your email</p>
              <p className="mt-1 text-sm">
                A create-session link was sent to <strong>{sentTo}</strong>. Open it to create the room and
                sign in as Game Master (SL). The link expires shortly.
              </p>
            </div>
            <button
              type="button"
              onClick={() => { setSentTo(null); setRoomName(''); }}
              className="text-sm font-semibold text-dodred-600 hover:underline dark:text-dodred-400"
            >
              Send another link
            </button>
          </div>
        ) : (
          <form onSubmit={onSubmit} className="space-y-6">
            <label className="block space-y-2">
              <span className={labelClass}>Game Master email</span>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="sl@example.com"
                className={inputClass}
                required
              />
            </label>

            <label className="block space-y-2">
              <span className={labelClass}>Room name</span>
              <input
                value={roomName}
                onChange={(e) => setRoomName(e.target.value)}
                placeholder="DRAGON"
                autoCapitalize="characters"
                maxLength={32}
                className={`${inputClass} uppercase tracking-widest`}
                required
              />
            </label>

            {error && (
              <div className="rounded-lg bg-dodred-50 p-3 text-sm font-medium text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
                {error}
              </div>
            )}

            <button type="submit" disabled={submitting} className={primaryButtonClass}>
              {submitting ? 'Sending…' : 'Email me a create link'}
            </button>

            <p className="text-center text-xs font-medium text-charcoal-500 dark:text-bonewhite-300">
              Joining a game? Scan the QR code your Game Master shows.
            </p>
          </form>
        )}
      </div>
    </div>
  );
};
