import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useSession } from '@hooks/useSession';
import { ThemeToggle } from '@components/ThemeToggle';

const getInitialRoomCode = () => {
  const searchParams = new URLSearchParams(window.location.search);
  const room = searchParams.get('room');
  if (room) return room;
  const match = window.location.pathname.match(/^\/room\/([^/]+)/);
  if (match) return decodeURIComponent(match[1]);
  return '';
};

const initialForm = { roomCode: getInitialRoomCode(), playerName: '' };

export const JoinPage = () => {
  const { join } = useSession();
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [step, setStep] = useState(initialForm.roomCode ? 2 : 1);

  const setField = <K extends keyof typeof initialForm>(field: K, value: (typeof initialForm)[K]) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const onSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (step === 1) {
      if (form.roomCode.trim()) setStep(2);
      return;
    }

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
    <div className="relative flex min-h-full items-center justify-center bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-bonewhite-200 via-bonewhite-100 to-bonewhite-50 p-6 dark:from-charcoal-800 dark:via-charcoal-900 dark:to-charcoal-950">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>

      <form
        onSubmit={onSubmit}
        className="w-full max-w-md space-y-6 rounded-3xl border border-bonewhite-200 bg-white/80 p-10 shadow-2xl backdrop-blur-sm transition-all dark:border-charcoal-700/50 dark:bg-charcoal-900/60 dark:shadow-[0_0_40px_-10px_rgba(227,28,35,0.3)]"
      >
        <div className="space-y-4 text-center">
          <img
            src="/dod.png"
            alt="Drakar och Demoner"
            className="mx-auto h-auto w-full max-w-[20rem] drop-shadow-md transition-transform hover:scale-105"
          />
          {step === 1 && (
            <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">
              Create a room
            </p>
          )}
        </div>

        {step === 1 ? (
          <>
            <label className="block space-y-2">
              <span className="text-sm font-semibold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400">
                Room name
              </span>
              <input
                value={form.roomCode}
                onChange={(e) => setField('roomCode', e.target.value)}
                placeholder="DRAGON"
                autoCapitalize="characters"
                className="w-full rounded-xl border-2 border-bonewhite-200 bg-bonewhite-50/50 px-4 py-3 text-lg uppercase tracking-widest text-charcoal-900 outline-none transition-colors focus:border-dodred-500 dark:border-charcoal-700 dark:bg-charcoal-950/50 dark:text-bonewhite-500 dark:focus:border-dodred-500"
                required
              />
            </label>

            <button
              type="submit"
              className="w-full rounded-xl bg-gradient-to-r from-dodred-500 to-dodred-600 px-4 py-3 text-lg font-bold tracking-wide text-white shadow-lg shadow-dodred-500/30 transition-all hover:scale-[1.02] hover:shadow-dodred-500/50"
            >
              Next
            </button>
          </>
        ) : (
          <>
            <div className="flex flex-col items-center justify-center space-y-3">
              <span className="text-sm font-bold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400">
                Room name:{' '}
                <span className="text-lg text-runecyan-600 dark:text-runecyan-500 uppercase">
                  {form.roomCode}
                </span>
              </span>
              <div className="overflow-hidden rounded-2xl bg-white p-4 shadow-md">
                <QRCodeSVG
                  value={`${window.location.origin}/?room=${form.roomCode}`}
                  size={160}
                  bgColor="#ffffff"
                  fgColor="#1c1c1c"
                />
              </div>
              <p className="text-xs font-medium text-charcoal-500 dark:text-bonewhite-300">
                Scan to join this table directly
              </p>
            </div>

            <label className="block space-y-2">
              <span className="text-sm font-semibold uppercase tracking-wider text-charcoal-700 dark:text-bonewhite-400">
                Player name
              </span>
              <input
                value={form.playerName}
                onChange={(e) => setField('playerName', e.target.value)}
                placeholder="Aragorn"
                className="w-full rounded-xl border-2 border-bonewhite-200 bg-bonewhite-50/50 px-4 py-3 text-lg text-charcoal-900 outline-none transition-colors focus:border-dodred-500 dark:border-charcoal-700 dark:bg-charcoal-950/50 dark:text-bonewhite-500 dark:focus:border-dodred-500"
                required
              />
            </label>

            {error && (
              <div className="rounded-lg bg-dodred-50 p-3 text-sm font-medium text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
                {error}
              </div>
            )}

            <div className="flex gap-3">
              <button
                type="button"
                onClick={() => setStep(1)}
                className="w-1/3 rounded-xl bg-bonewhite-200 px-4 py-3 text-lg font-bold tracking-wide text-charcoal-700 transition-all hover:bg-bonewhite-300 dark:bg-charcoal-700 dark:text-bonewhite-200 dark:hover:bg-charcoal-600"
              >
                Back
              </button>
              <button
                type="submit"
                disabled={submitting}
                className="w-2/3 rounded-xl bg-gradient-to-r from-dodred-500 to-dodred-600 px-4 py-3 text-lg font-bold tracking-wide text-white shadow-lg shadow-dodred-500/30 transition-all hover:scale-[1.02] hover:shadow-dodred-500/50 disabled:pointer-events-none disabled:opacity-50"
              >
                {submitting ? 'Joining…' : 'Join session'}
              </button>
            </div>
          </>
        )}
      </form>
    </div>
  );
};
