import { useEffect } from 'react';
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
      <header className="mb-4 flex items-center justify-between gap-4 border-b border-bonewhite-200 pb-3 dark:border-charcoal-700">
        <div className="flex items-center gap-4">
          <img src="/dod.png" alt="Drakar och Demoner" className="h-8 w-auto" />
          <p className="text-sm text-charcoal-500 dark:text-bonewhite-300">
            Playing as{' '}
            <span className="font-medium text-dodred-600 dark:text-dodred-400">{session.playerName}</span>
          </p>
        </div>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          <button
            onClick={onLogout}
            className="rounded-lg border border-bonewhite-200 px-3 py-1.5 text-sm text-charcoal-600 transition hover:bg-bonewhite-50 dark:border-charcoal-600 dark:text-bonewhite-300 dark:hover:bg-charcoal-800"
          >
            Leave
          </button>
        </div>
      </header>

      <div className="grid flex-1 gap-6 md:grid-cols-2">
        <section className="flex flex-col gap-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-dragongreen-600 dark:text-dragongreen-500">
            Timeline
          </h2>
          <div className="flex-1 overflow-y-auto pr-1">
            <TimelineView entries={entries} />
          </div>
          <LogEntryForm />
        </section>

        <section className="flex flex-col gap-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-runecyan-600 dark:text-runecyan-500">
            Rules
          </h2>
          <RuleSearchPanel />
        </section>
      </div>
    </div>
  );
};
