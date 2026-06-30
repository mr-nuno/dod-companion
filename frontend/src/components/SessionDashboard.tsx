import { TimelineView } from '@components/TimelineView';
import { LogEntryForm } from '@components/LogEntryForm';
import { PlayerRoster } from '@components/PlayerRoster';
import { SessionSummaryPanel } from '@components/SessionSummaryPanel';
import type { LogEntry } from '@/types';

interface SessionDashboardProps {
  entries: LogEntry[];
}

export const SessionDashboard = ({ entries }: SessionDashboardProps) => {
  return (
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

      <div className="flex flex-col gap-8">
        <section className="flex flex-col gap-4 rounded-3xl bg-white/80 p-5 shadow-lg backdrop-blur-sm dark:bg-charcoal-900/50">
          <h2 className="flex items-center gap-2 text-sm font-bold uppercase tracking-widest text-dodred-600 dark:text-dodred-500">
            <span className="h-2 w-2 rounded-full bg-dodred-500 shadow-[0_0_8px_rgba(227,28,35,0.6)]"></span>
            Players
          </h2>
          <PlayerRoster />
        </section>

        <section className="flex flex-col gap-4 rounded-3xl bg-white/80 p-5 shadow-lg backdrop-blur-sm dark:bg-charcoal-900/50">
          <h2 className="flex items-center gap-2 text-sm font-bold uppercase tracking-widest text-charcoal-600 dark:text-bonewhite-400">
            <span className="h-2 w-2 rounded-full bg-charcoal-500"></span>
            Session Summary
          </h2>
          <SessionSummaryPanel />
        </section>
      </div>
    </div>
  );
};
