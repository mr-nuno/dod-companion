import { useEffect } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useSession } from '@hooks/useSession';
import { useTimeline } from '@hooks/useTimeline';
import { TimelineView } from '@components/TimelineView';
import { LogEntryForm } from '@components/LogEntryForm';
import { RuleSearchPanel } from '@components/RuleSearchPanel';
import { ThemeToggle } from '@components/ThemeToggle';
import type { Session } from '@/types';

interface SessionPageProps {
  session: Session;
}

export const SessionPage = ({ session }: SessionPageProps) => {
  const { logout } = useSession();
  const { entries, load, reset } = useTimeline();

  useEffect(() => {
    void load();
    return () => {
      void reset();
    };
  }, [load, reset]);

  const onLogout = async () => {
    await reset();
    await logout();
  };

  return (
    <div className="mx-auto flex min-h-full max-w-6xl flex-col p-4">
      <header className="mb-6 flex flex-col gap-4 rounded-3xl bg-white/60 p-4 shadow-sm backdrop-blur-md dark:bg-charcoal-900/40 dark:shadow-charcoal-900/50 sm:p-6 md:flex-row md:items-center md:justify-between md:gap-6">
        <div className="flex flex-col gap-4 w-full md:w-auto md:flex-row md:items-center md:gap-8">
          <div className="flex justify-center w-full md:w-auto md:justify-start">
            <img
              src="/dod.png"
              alt="Drakar och Demoner"
              className="w-full h-auto max-w-[280px] drop-shadow-sm transition-transform hover:scale-105 md:h-24 md:w-auto md:max-w-none"
            />
          </div>
          <div className="hidden flex-col md:flex">
            <span className="text-sm font-bold uppercase tracking-widest text-charcoal-400 dark:text-charcoal-500">
              Playing As
            </span>
            <span className="text-2xl font-bold text-dodred-600 dark:text-dodred-500">
              {session.playerName}
            </span>
          </div>
          <div className="hidden border-l border-bonewhite-300 pl-8 dark:border-charcoal-700 md:block">
            <div
              className="cursor-help overflow-hidden rounded-xl bg-white p-2 shadow-sm transition-transform hover:scale-105"
              title={`Room: ${session.roomCode} — scan to join`}
            >
              <QRCodeSVG
                value={`${window.location.origin}/?join=${session.joinToken}`}
                size={80}
                bgColor="#ffffff"
                fgColor="#1c1c1c"
              />
            </div>
          </div>
        </div>

        <div className="flex items-center justify-center gap-3 w-full md:hidden">
          <ThemeToggle />
          <button
            onClick={onLogout}
            className="rounded-xl border border-transparent bg-bonewhite-100 px-4 py-2 text-sm font-semibold text-charcoal-600 transition-all hover:border-dodred-500 hover:bg-white hover:text-dodred-600 dark:bg-charcoal-800 dark:text-bonewhite-300 dark:hover:border-dodred-500 dark:hover:bg-charcoal-900 dark:hover:text-dodred-400"
          >
            Leave
          </button>
        </div>

        <div className="flex items-center justify-between rounded-2xl bg-white/40 px-4 py-3 dark:bg-charcoal-900/20 md:hidden">
          <div className="flex flex-col">
            <span className="text-xs font-bold uppercase tracking-widest text-charcoal-400 dark:text-charcoal-500">
              Playing As
            </span>
            <span className="text-lg font-bold text-dodred-600 dark:text-dodred-500">
              {session.playerName}
            </span>
          </div>
          <div className="flex flex-col items-end">
            <span className="text-xs font-bold uppercase tracking-widest text-charcoal-400 dark:text-charcoal-500">
              Room Code
            </span>
            <span className="text-sm font-semibold text-charcoal-600 dark:text-bonewhite-300">
              {session.roomCode}
            </span>
          </div>
        </div>

        <div className="hidden items-center gap-3 md:flex">
          <ThemeToggle />
          <button
            onClick={onLogout}
            className="rounded-xl border-2 border-transparent bg-bonewhite-100 px-4 py-2 text-sm font-semibold text-charcoal-600 transition-all hover:border-dodred-500 hover:bg-white hover:text-dodred-600 hover:shadow-md dark:bg-charcoal-800 dark:text-bonewhite-300 dark:hover:border-dodred-500 dark:hover:bg-charcoal-900 dark:hover:text-dodred-400"
          >
            Leave
          </button>
        </div>
      </header>

      <div className="grid flex-1 gap-8 md:grid-cols-2">
        <section className="flex flex-col gap-4 rounded-3xl bg-white/80 p-5 shadow-lg backdrop-blur-sm dark:bg-charcoal-900/50">
          <h2 className="flex items-center gap-2 text-sm font-bold uppercase tracking-widest text-dragongreen-600 dark:text-dragongreen-500">
            <span className="h-2 w-2 rounded-full bg-dragongreen-500 shadow-[0_0_8px_rgba(24,184,104,0.8)]"></span>
            Timeline
          </h2>
          <div className="flex-1 overflow-y-auto pr-2">
            <TimelineView entries={entries} />
          </div>
          <LogEntryForm />
        </section>

        <section className="flex flex-col gap-4 rounded-3xl bg-white/80 p-5 shadow-lg backdrop-blur-sm dark:bg-charcoal-900/50">
          <h2 className="flex items-center gap-2 text-sm font-bold uppercase tracking-widest text-runecyan-600 dark:text-runecyan-500">
            <span className="h-2 w-2 rounded-full bg-runecyan-500 shadow-[0_0_8px_rgba(52,242,207,0.8)]"></span>
            Rules Reference
          </h2>
          <RuleSearchPanel />
        </section>
      </div>
    </div>
  );
};
