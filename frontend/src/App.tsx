import { useEffect, useState } from 'react';
import { useSession } from '@hooks/useSession';
import { JoinPage } from '@components/JoinPage';
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

  return session ? <SessionPage session={session} /> : <JoinPage />;
};
