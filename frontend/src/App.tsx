import { useEffect, useState } from 'react';
import { Route, Switch } from 'wouter';
import { useSession } from '@hooks/useSession';
import { StartPage } from '@components/StartPage';
import { JoinQrPage } from '@components/JoinQrPage';
import { JoinPage } from '@components/JoinPage';
import { CreateLanding } from '@components/CreateLanding';
import { SessionPage } from '@components/SessionPage';

export const App = () => {
  const { session, loadMe } = useSession();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    void loadMe().finally(() => setReady(true));
  }, [loadMe]);

  if (!ready) {
    return (
      <div className="flex min-h-full items-center justify-center text-charcoal-400 dark:text-bonewhite-300/60">
        Loading…
      </div>
    );
  }

  return (
    <Switch>
      {/* Join form (name + properties) — where scanning the QR lands. Available regardless of session. */}
      <Route path="/join/:token">{(params) => <JoinPage token={params.token} />}</Route>

      {/* QR-display page (/join?token=…): the GM shows this; scanning it opens the form above. */}
      <Route path="/join">
        <JoinQrPage />
      </Route>

      {/* Magic-link landing: consumes ?token= and signs in as SL, then redirects home. */}
      <Route path="/create">
        <CreateLanding />
      </Route>

      {/* Everything else: the session dashboard when signed in, otherwise the create/start page. */}
      <Route>{session ? <SessionPage session={session} /> : <StartPage />}</Route>
    </Switch>
  );
};
