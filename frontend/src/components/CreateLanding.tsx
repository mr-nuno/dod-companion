import { useEffect, useRef, useState } from 'react';
import { useLocation } from 'wouter';
import { useSession } from '@hooks/useSession';

export const CreateLanding = () => {
  const { consumeCreateLink } = useSession();
  const [, navigate] = useLocation();
  const [error, setError] = useState<string | null>(null);
  // Guard against React StrictMode running the effect twice (the link is single-use).
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const token = new URLSearchParams(window.location.search).get('token') ?? '';
    if (!token) {
      setError('This link is missing its token.');
      return;
    }

    void consumeCreateLink(token)
      .then(() => navigate('/'))
      .catch((err) => setError(err instanceof Error ? err.message : 'This link is invalid or has expired.'));
  }, [consumeCreateLink, navigate]);

  return (
    <div className="flex min-h-full items-center justify-center bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-bonewhite-200 via-bonewhite-100 to-bonewhite-50 p-6 dark:from-charcoal-800 dark:via-charcoal-900 dark:to-charcoal-950">
      <div className="w-full max-w-md space-y-4 rounded-3xl border border-bonewhite-200 bg-white/80 p-10 text-center shadow-2xl backdrop-blur-sm dark:border-charcoal-700/50 dark:bg-charcoal-900/60">
        <img src="/dod.png" alt="Drakar och Demoner" className="mx-auto h-auto w-full max-w-[16rem] drop-shadow-md" />
        {error ? (
          <>
            <div className="rounded-2xl bg-dodred-50 p-5 text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
              <p className="text-lg font-bold">Link unavailable</p>
              <p className="mt-1 text-sm">{error}</p>
            </div>
            <button
              type="button"
              onClick={() => navigate('/')}
              className="text-sm font-semibold text-dodred-600 hover:underline dark:text-dodred-400"
            >
              Back to start
            </button>
          </>
        ) : (
          <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">
            Creating your session…
          </p>
        )}
      </div>
    </div>
  );
};
