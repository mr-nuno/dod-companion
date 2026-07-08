import { Link } from 'wouter';
import { QRCodeSVG } from 'qrcode.react';
import { ThemeToggle } from '@components/ThemeToggle';

/**
 * QR-display page at <c>/join?token={token}</c>. Shows only the room's join QR so the Game Master can
 * present it on a shared screen; scanning it opens the join form at <c>/join/{token}</c>.
 */
export const JoinQrPage = () => {
  const token = new URLSearchParams(window.location.search).get('token') ?? '';
  const joinUrl = `${window.location.origin}/join/${token}`;

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
            className="mx-auto h-auto w-full max-w-[20rem] drop-shadow-md"
          />
          <p className="text-lg font-medium text-charcoal-500 dark:text-bonewhite-300">Scan to join</p>
        </div>

        {token ? (
          <div className="flex flex-col items-center justify-center space-y-3">
            <div className="overflow-hidden rounded-2xl bg-white p-3 sm:p-5 shadow-md flex justify-center w-full max-w-[320px]">
              <QRCodeSVG
                value={joinUrl}
                size={280}
                className="w-full h-auto max-w-[280px]"
                bgColor="#ffffff"
                fgColor="#1c1c1c"
              />
            </div>
            <p className="text-xs font-medium text-charcoal-500 dark:text-bonewhite-300">
              Point your phone camera here to join the session
            </p>
            <Link
              href="/"
              className="text-sm font-semibold text-dodred-600 hover:underline dark:text-dodred-400"
            >
              ← Back to session
            </Link>
          </div>
        ) : (
          <div className="rounded-lg bg-dodred-50 p-3 text-center text-sm font-medium text-dodred-700 dark:bg-dodred-900/30 dark:text-dodred-300">
            This link is missing its room token.
          </div>
        )}
      </div>
    </div>
  );
};
