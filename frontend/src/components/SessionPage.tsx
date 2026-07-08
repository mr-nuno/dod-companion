import { useEffect, useState } from 'react';
import { Link, Route, Switch, useLocation } from 'wouter';
import { QRCodeSVG } from 'qrcode.react';
import { useSession } from '@hooks/useSession';
import { useTimeline } from '@hooks/useTimeline';
import { useRoster } from '@hooks/useRoster';
import { RuleSearchPanel } from '@components/RuleSearchPanel';
import { SessionDashboard } from '@components/SessionDashboard';
import { ThemeToggle } from '@components/ThemeToggle';
import type { Session } from '@/types';

interface SessionPageProps {
  session: Session;
}

export const SessionPage = ({ session }: SessionPageProps) => {
  const { logout } = useSession();
  const { entries, load, reset } = useTimeline();
  const { load: loadRoster, reset: resetRoster } = useRoster();
  const [location] = useLocation();
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

  useEffect(() => {
    void load();
    void loadRoster();
    return () => {
      void reset();
      resetRoster();
    };
  }, [load, reset, loadRoster, resetRoster]);

  const onLogout = async () => {
    await reset();
    resetRoster();
    await logout();
  };

  const navLinkClass = (path: string) => 
    `block w-full text-left rounded-xl px-4 py-3 text-sm font-semibold transition-all ${
      location === path 
        ? 'bg-dodred-50 text-dodred-600 dark:bg-dodred-500/10 dark:text-dodred-400' 
        : 'text-charcoal-600 hover:bg-bonewhite-100 dark:text-bonewhite-300 dark:hover:bg-charcoal-800'
    }`;

  const closeDrawer = () => setIsDrawerOpen(false);

  return (
    <div className="mx-auto flex min-h-full max-w-6xl flex-col p-4 relative">
      <header className="relative mb-6 flex flex-col gap-4 rounded-3xl bg-white/60 p-4 shadow-sm backdrop-blur-md dark:bg-charcoal-900/40 dark:shadow-charcoal-900/50 sm:p-6 md:flex-row md:items-center md:justify-between md:gap-6">
        <div className="flex flex-col gap-4 w-full md:w-auto md:flex-row md:items-center md:gap-8">
          <div className="flex justify-start w-full pr-16 md:w-auto md:justify-start md:pr-0">
            <img
              src="/dod.png"
              alt="Drakar och Demoner"
              className="w-full h-auto max-w-[240px] drop-shadow-sm transition-transform hover:scale-105 md:h-24 md:w-auto md:max-w-none"
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
            <Link
              href={`/join?token=${session.joinToken}`}
              className="block cursor-pointer overflow-hidden rounded-xl bg-white p-2 shadow-sm transition-transform hover:scale-105"
              title={`Room: ${session.roomCode} — open the shareable join QR`}
            >
              <QRCodeSVG
                value={`${window.location.origin}/join/${session.joinToken}`}
                size={80}
                bgColor="#ffffff"
                fgColor="#1c1c1c"
              />
            </Link>
          </div>
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

        <div className="absolute top-4 right-4 z-30 sm:top-6 sm:right-6 md:relative md:top-auto md:right-auto">
          <button
            onClick={() => setIsDrawerOpen(true)}
            className="flex h-12 w-12 items-center justify-center rounded-xl bg-white/80 text-charcoal-600 shadow-sm backdrop-blur-md transition-all hover:scale-105 hover:text-dodred-600 dark:bg-charcoal-800/80 dark:text-bonewhite-300 dark:hover:text-dodred-400"
            aria-label="Open menu"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>
      </header>

      {/* Drawer Overlay */}
      <div 
        className={`fixed inset-0 z-40 bg-charcoal-900/40 backdrop-blur-sm transition-opacity duration-300 ${isDrawerOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'}`}
        onClick={closeDrawer}
      ></div>

      {/* Drawer Panel */}
      <aside 
        className={`fixed top-0 right-0 z-50 h-full w-72 transform bg-bonewhite-50 p-6 shadow-2xl transition-transform duration-300 ease-in-out dark:bg-charcoal-950 ${isDrawerOpen ? 'translate-x-0' : 'translate-x-full'}`}
      >
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-lg font-bold uppercase tracking-widest text-charcoal-900 dark:text-bonewhite-50">Menu</h2>
          <button
            onClick={closeDrawer}
            className="flex h-10 w-10 items-center justify-center rounded-xl bg-bonewhite-200/50 text-charcoal-500 transition-colors hover:bg-bonewhite-200 hover:text-dodred-600 dark:bg-charcoal-800 dark:text-bonewhite-400 dark:hover:bg-charcoal-700 dark:hover:text-dodred-400"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <nav className="flex flex-col gap-2">
          <Link href="/" onClick={closeDrawer} className={navLinkClass('/')}>
            Dashboard
          </Link>
          <Link href="/rules" onClick={closeDrawer} className={navLinkClass('/rules')}>
            Rules Reference
          </Link>
          <Link
            href={`/join?token=${session.joinToken}`}
            onClick={closeDrawer}
            className={navLinkClass('/join')}
          >
            Join QR
          </Link>
        </nav>

        <div className="my-6 h-px w-full bg-bonewhite-200 dark:bg-charcoal-800"></div>

        <div className="flex flex-col gap-4">
          <div className="flex items-center justify-between rounded-xl px-4 py-3 bg-white shadow-sm dark:bg-charcoal-900/60">
            <span className="text-sm font-semibold text-charcoal-700 dark:text-bonewhite-300">Theme</span>
            <ThemeToggle />
          </div>

          <button
            onClick={() => { closeDrawer(); void onLogout(); }}
            className="w-full rounded-xl border-2 border-transparent bg-bonewhite-200/50 px-4 py-3 text-sm font-semibold text-charcoal-700 transition-all hover:border-dodred-500 hover:bg-white hover:text-dodred-600 hover:shadow-md dark:bg-charcoal-800 dark:text-bonewhite-200 dark:hover:border-dodred-500 dark:hover:bg-charcoal-900 dark:hover:text-dodred-400"
          >
            Leave Session
          </button>
        </div>
      </aside>

      <Switch>
        <Route path="/rules">
          <section className="flex flex-col flex-1 gap-4 rounded-3xl bg-white/80 p-5 shadow-lg backdrop-blur-sm dark:bg-charcoal-900/50">
            <h2 className="flex items-center gap-2 text-sm font-bold uppercase tracking-widest text-runecyan-600 dark:text-runecyan-500">
              <span className="h-2 w-2 rounded-full bg-runecyan-500 shadow-[0_0_8px_rgba(52,242,207,0.8)]"></span>
              Rules Reference
            </h2>
            <div className="flex-1 min-h-[500px]">
              <RuleSearchPanel />
            </div>
          </section>
        </Route>
        
        <Route path="/">
          <SessionDashboard entries={entries} />
        </Route>
      </Switch>
    </div>
  );
};
